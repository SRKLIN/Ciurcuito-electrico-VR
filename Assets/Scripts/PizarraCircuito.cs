using TMPro;
using UnityEngine;

/// <summary>
/// Pizarra viva. Hace dos cosas, las dos de vista: enciende los bloques del
/// esquema segun lo que hay encastrado en la mesa, y lanza puntos que recorren
/// la malla a la velocidad de la corriente.
///
/// No calcula nada electrico: lee <see cref="CircuitoSimple"/> para la corriente
/// y <see cref="CircuitoArmable"/> para saber que zocalo tiene su pieza. En una
/// mesa de circuito fijo, el armable se deja vacio y todos los bloques cuentan
/// como puestos.
///
/// Los puntos van en el sentido convencional de la corriente: salen del polo
/// positivo, atraviesan los componentes y vuelven por el negativo.
/// </summary>
public class PizarraCircuito : MonoBehaviour
{
    /// <summary>Un bloque del esquema atado a una familia de pieza.</summary>
    [System.Serializable]
    public class Bloque
    {
        public TipoPieza tipo;
        public Renderer caja;
        public TMP_Text etiqueta;
    }

    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;

    [Tooltip("Opcional. Sin el, todos los bloques se dan por puestos.")]
    [SerializeField] private CircuitoArmable armable;

    [SerializeField] private Bloque[] bloques;

    [Header("Colores del bloque")]
    [SerializeField] private Color faltaPieza = new Color(0.38f, 0.40f, 0.44f);
    [SerializeField] private Color piezaPuesta = new Color(0.88f, 0.88f, 0.84f);
    [SerializeField] private Color conCorriente = new Color(1f, 0.88f, 0.58f);
    [SerializeField] private Color polaridadMal = new Color(0.85f, 0.35f, 0.30f);

    [Tooltip("Bloque cuyo hueco tiene una pieza que no le corresponde.")]
    [SerializeField] private Color piezaEquivocada = new Color(0.80f, 0.22f, 0.18f);

    [SerializeField] private Color textoNormal = new Color(0.13f, 0.14f, 0.16f);
    [SerializeField] private Color textoApagado = new Color(0.62f, 0.64f, 0.68f);

    [Header("Aviso de mal montaje")]
    [Tooltip("Opcional. Cartel que aparece cuando hay una pieza en el hueco equivocado.")]
    [SerializeField] private TMP_Text aviso;

    [SerializeField] private string textoCorto = "CORTO: hay una pieza mal puesta";

    [Header("Flujo de corriente")]
    [Tooltip("El punto que se clona. Vacio = sin animacion de flujo.")]
    [SerializeField] private GameObject plantillaPunto;

    [Tooltip("Vertices de la malla, en coordenadas locales de la pizarra y en orden de circulacion.")]
    [SerializeField] private Vector3[] recorrido;

    [SerializeField] private int cantidadPuntos = 14;

    [Tooltip("Corriente, en amperios, a la que los puntos van a su velocidad maxima.")]
    [SerializeField] private float corrienteNominal = 0.5f;

    [Tooltip("Unidades locales por segundo. Con poca corriente baja hasta la minima.")]
    [SerializeField] private float velocidadMaxima = 0.85f;
    [SerializeField] private float velocidadMinima = 0.12f;

    private Transform[] puntos;
    private float[] acumulado;
    private float largoTotal;
    private float avance;
    private bool puntosVisibles = true;

    private MaterialPropertyBlock pincel;
    private bool sucio = true;
    private bool habiaCorriente;
    private bool estabaTrabado;

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        pincel = new MaterialPropertyBlock();
        MedirRecorrido();
        CrearPuntos();
    }

    private void OnEnable()
    {
        if (armable != null)
            armable.Cambio += Ensuciar;

        Ensuciar();
    }

    private void OnDisable()
    {
        if (armable != null)
            armable.Cambio -= Ensuciar;
    }

    private void Ensuciar()
    {
        sucio = true;
    }

    private void Update()
    {
        if (circuito == null)
            return;

        float corriente = circuito.Corriente;
        Refrescar(corriente);
        MoverPuntos(corriente);
    }

    /// <summary>
    /// Repinta los bloques, pero solo cuando algo cambio de verdad: en un frame
    /// normal esto sale en la primera linea.
    /// </summary>
    private void Refrescar(float corriente)
    {
        bool hayCorriente = corriente > 0.0001f;
        bool trabado = circuito.BloqueadoPorPolaridad;

        if (!sucio && hayCorriente == habiaCorriente && trabado == estabaTrabado)
            return;

        sucio = false;
        habiaCorriente = hayCorriente;
        estabaTrabado = trabado;

        bool malMontado = armable != null && armable.MalMontado;

        if (aviso != null)
        {
            aviso.text = textoCorto;
            aviso.gameObject.SetActive(malMontado);
        }

        if (bloques == null)
            return;

        foreach (var b in bloques)
        {
            if (b == null || b.caja == null)
                continue;

            // Sin mesa de armado, o sin zocalo para esa familia, la pieza es
            // parte fija del circuito y siempre cuenta como puesta.
            bool puesta = armable == null
                || !armable.TieneZocalo(b.tipo)
                || armable.TienePieza(b.tipo);

            Color color;

            // El hueco con la pieza cambiada manda sobre todo lo demas: es el
            // unico estado que hay que corregir para que el circuito funcione.
            if (armable != null && armable.PiezaMalPuesta(b.tipo))
                color = piezaEquivocada;
            else if (!puesta)
                color = faltaPieza;
            else if (trabado && b.tipo == TipoPieza.Diodo)
                color = polaridadMal;
            else if (hayCorriente)
                color = conCorriente;
            else
                color = piezaPuesta;

            Pintar(b.caja, color);

            if (b.etiqueta != null)
                b.etiqueta.color = puesta ? textoNormal : textoApagado;
        }
    }

    private void Pintar(Renderer r, Color color)
    {
        // MaterialPropertyBlock en vez de .material: los bloques comparten un
        // unico material y no hace falta instanciar uno por caja.
        r.GetPropertyBlock(pincel);
        pincel.SetColor(BaseColor, color);
        r.SetPropertyBlock(pincel);
    }

    private void MoverPuntos(float corriente)
    {
        if (puntos == null || puntos.Length == 0 || largoTotal <= 0f)
            return;

        bool visible = corriente > 0.0001f;

        if (visible != puntosVisibles)
        {
            foreach (var p in puntos)
                p.gameObject.SetActive(visible);

            puntosVisibles = visible;
        }

        if (!visible)
            return;

        float t = corrienteNominal > 0f ? Mathf.Clamp01(corriente / corrienteNominal) : 0f;
        float velocidad = Mathf.Lerp(velocidadMinima, velocidadMaxima, t);

        avance = Mathf.Repeat(avance + velocidad * Time.deltaTime, largoTotal);

        float paso = largoTotal / puntos.Length;

        for (int i = 0; i < puntos.Length; i++)
            puntos[i].localPosition = PosicionEn(Mathf.Repeat(avance + i * paso, largoTotal));
    }

    /// <summary>Punto de la malla que queda a esa distancia del arranque.</summary>
    private Vector3 PosicionEn(float distancia)
    {
        for (int i = 0; i < recorrido.Length; i++)
        {
            if (distancia > acumulado[i])
                continue;

            float inicioTramo = i == 0 ? 0f : acumulado[i - 1];
            float largoTramo = acumulado[i] - inicioTramo;
            Vector3 a = recorrido[i];
            Vector3 b = recorrido[(i + 1) % recorrido.Length];

            return Vector3.Lerp(a, b, largoTramo > 0f ? (distancia - inicioTramo) / largoTramo : 0f);
        }

        return recorrido[0];
    }

    private void MedirRecorrido()
    {
        if (recorrido == null || recorrido.Length < 2)
            return;

        acumulado = new float[recorrido.Length];
        float suma = 0f;

        // Se cierra la malla: del ultimo vertice se vuelve al primero.
        for (int i = 0; i < recorrido.Length; i++)
        {
            suma += Vector3.Distance(recorrido[i], recorrido[(i + 1) % recorrido.Length]);
            acumulado[i] = suma;
        }

        largoTotal = suma;
    }

    private void CrearPuntos()
    {
        if (plantillaPunto == null || largoTotal <= 0f || cantidadPuntos < 1)
            return;

        puntos = new Transform[cantidadPuntos];
        puntos[0] = plantillaPunto.transform;

        for (int i = 1; i < cantidadPuntos; i++)
        {
            var copia = Instantiate(plantillaPunto, plantillaPunto.transform.parent);
            copia.name = "Punto " + (i + 1);
            puntos[i] = copia.transform;
        }

        foreach (var p in puntos)
            p.gameObject.SetActive(false);

        puntosVisibles = false;
    }
}
