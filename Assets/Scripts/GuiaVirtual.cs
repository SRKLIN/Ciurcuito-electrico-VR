using System.Collections;
using UnityEngine;

/// <summary>Los tramos de la visita, en el orden en que ocurren.</summary>
public enum EtapaGuia
{
    Dormido,
    Bienvenida,
    YendoAPractica,
    EnPractica,
    EsperandoRetos,
    YendoARetos,
    EnRetos,
    Fin
}

/// <summary>
/// El guia del laboratorio. Espera en el cuarto de entrada, da la bienvenida
/// cuando el jugador se acerca, lo lleva caminando hasta la zona de practica
/// explicando por el camino, y ahi se queda hasta que le hablen. Al tocarlo
/// arranca la segunda mitad: camina hasta la mesa de retos, explica los retos
/// y se queda parado.
///
/// La visita entera es UNA corrutina que se lee de arriba abajo, como el guion:
/// esperar, hablar, caminar, esperar el toque, caminar, hablar. No hay maquina
/// de estados repartida por Update ni banderas cruzadas; la etapa publica existe
/// solo para que otros scripts puedan mirarla.
///
/// No sabe nada de como se muestra lo que dice: eso es del
/// <see cref="BocadilloGuia"/>. Y no sabe nada de como lo tocan: eso es del
/// <see cref="GuiaTocable"/>.
/// </summary>
public class GuiaVirtual : MonoBehaviour
{
    /// <summary>Una linea del guion: lo que dice, con que voz, y cuanto dura.</summary>
    [System.Serializable]
    public class Parlamento
    {
        [TextArea(2, 4)]
        [Tooltip("Lo que dice. Se muestra en el bocadillo y sirve de subtitulo.")]
        public string texto;

        [Tooltip("Arrastrar aca el audio grabado. Vacio = solo texto, sin voz.")]
        public AudioClip voz;

        [Tooltip("Segundos que dura si no hay voz. 0 = calculado por el largo del texto.")]
        public float duracionSinVoz;
    }

    [Header("Referencias")]
    [Tooltip("A quien sigue y mira. Vacio = busca la camara principal al arrancar.")]
    [SerializeField] private Transform jugador;

    [Tooltip("El panel de texto que flota sobre el guia. Opcional.")]
    [SerializeField] private BocadilloGuia bocadillo;

    [Tooltip("Fuente para la voz. Opcional: sin ella el guia funciona mudo.")]
    [SerializeField] private AudioSource voz;

    [Tooltip("La parte visible que se bambolea al caminar. Opcional.")]
    [SerializeField] private Transform cuerpo;

    [Header("Ruta")]
    [Tooltip("Puntos por los que pasa desde el cuarto hasta la zona de practica.")]
    [SerializeField] private Transform[] rutaPractica;

    [Tooltip("Puntos por los que pasa desde la practica hasta la mesa de retos.")]
    [SerializeField] private Transform[] rutaRetos;

    [Header("Caminar")]
    [Tooltip("Metros por segundo. Por debajo de los 2.5 del jugador, para que se lo pueda seguir.")]
    [SerializeField] private float velocidad = 1.1f;

    [Tooltip("Grados por segundo al girar hacia donde va o hacia el jugador.")]
    [SerializeField] private float velocidadGiro = 180f;

    [Tooltip("A esta distancia del punto lo da por alcanzado.")]
    [SerializeField] private float toleranciaPunto = 0.25f;

    [Tooltip("Si el jugador se queda mas atras que esto, el guia frena y lo espera.")]
    [SerializeField] private float distanciaEspera = 4.5f;

    [Tooltip("Cuanto sube y baja el cuerpo al caminar. 0 = quieto.")]
    [SerializeField] private float bamboleo = 0.035f;

    [Header("Arranque")]
    [Tooltip("A esta distancia o menos, el guia empieza a hablar solo. Tambien es el radio " +
             "en el que hay que quedarse para que arranque el viaje a los retos.")]
    [SerializeField] private float distanciaSaludo = 2.2f;

    [Tooltip("Segundos que hay que quedarse cerca para que te lleve a los retos.")]
    [SerializeField] private float tiempoParaSeguir = 3f;

    [Tooltip("Lo que muestra el cartel mientras espera que el jugador se acerque.")]
    [TextArea(1, 3)]
    [SerializeField] private string textoLlamada = "Acercate y te explico";

    [Header("Guion")]
    [Tooltip("En el cuarto de entrada, parado. Los mandos y como agarrar.")]
    [SerializeField] private Parlamento[] bienvenida;

    [Tooltip("Mientras camina del cuarto a la zona de practica.")]
    [SerializeField] private Parlamento[] deCamino;

    [Tooltip("Parado frente a las mesas de armado.")]
    [SerializeField] private Parlamento[] enPractica;

    [Tooltip("Lo que muestra el cartel mientras espera que lo toquen.")]
    [TextArea(1, 3)]
    [SerializeField] private string invitacionRetos = "Tocame cuando quieras ir a la zona de retos";

    [Tooltip("Mientras camina de la practica a la mesa de retos.")]
    [SerializeField] private Parlamento[] deCaminoARetos;

    [Tooltip("Parado frente a la mesa de retos.")]
    [SerializeField] private Parlamento[] enRetos;

    [Tooltip("Lo que queda en el cartel cuando termino la visita.")]
    [TextArea(1, 3)]
    [SerializeField] private string textoFinal = "Suerte con los retos";

    private EtapaGuia etapa = EtapaGuia.Dormido;
    private bool hablando;
    private bool leHablaron;
    private bool caminando;
    private float alturaCuerpo;
    private float paso;

    /// <summary>En que tramo de la visita va. Para que lo lean otros scripts.</summary>
    public EtapaGuia Etapa => etapa;

    /// <summary>
    /// Que el jugador lo toco. Solo hace algo donde el guion espera un toque:
    /// para despertarlo antes de tiempo y para arrancar el viaje a los retos.
    /// </summary>
    public void Hablarle()
    {
        leHablaron = true;
    }

    private void Awake()
    {
        if (jugador == null && Camera.main != null)
            jugador = Camera.main.transform;

        if (cuerpo != null)
            alturaCuerpo = cuerpo.localPosition.y;
    }

    private void Start()
    {
        StartCoroutine(Visita());
    }

    private void Update()
    {
        // Quieto y hablando: se da vuelta hacia quien lo escucha.
        if (!caminando && jugador != null)
            GirarHacia(jugador.position - transform.position);
    }

    // ---------------------------------------------------------------- guion

    private IEnumerator Visita()
    {
        etapa = EtapaGuia.Dormido;
        Mostrar(textoLlamada);

        leHablaron = false;
        yield return new WaitUntil(() => leHablaron || JugadorCerca(distanciaSaludo));

        etapa = EtapaGuia.Bienvenida;
        yield return Decir(bienvenida);

        etapa = EtapaGuia.YendoAPractica;
        StartCoroutine(Decir(deCamino));
        yield return Caminar(rutaPractica);
        yield return new WaitWhile(() => hablando);

        etapa = EtapaGuia.EnPractica;
        yield return Decir(enPractica);

        etapa = EtapaGuia.EsperandoRetos;
        leHablaron = false;
        Mostrar(invitacionRetos);
        yield return Acompanar();

        etapa = EtapaGuia.YendoARetos;
        StartCoroutine(Decir(deCaminoARetos));
        yield return Caminar(rutaRetos);
        yield return new WaitWhile(() => hablando);

        etapa = EtapaGuia.EnRetos;
        yield return Decir(enRetos);

        etapa = EtapaGuia.Fin;
        Mostrar(textoFinal);
    }

    // --------------------------------------------------------------- hablar

    private IEnumerator Decir(Parlamento[] lista)
    {
        hablando = true;

        if (lista != null)
        {
            foreach (Parlamento p in lista)
            {
                if (p == null || string.IsNullOrEmpty(p.texto))
                    continue;

                Mostrar(p.texto);

                if (p.voz != null && voz != null)
                {
                    voz.Stop();
                    voz.clip = p.voz;
                    voz.Play();
                }

                yield return new WaitForSeconds(Duracion(p));
            }
        }

        hablando = false;
    }

    /// <summary>
    /// Espera a que el jugador se quede cerca un rato seguido. Si se aleja, la
    /// cuenta vuelve a cero: asi pasar por al lado camino a otra cosa no lo
    /// arrastra a los retos sin querer. Es el mismo criterio de
    /// <see cref="ControlRetos"/>, que tampoco da por bueno un roce al pasar.
    ///
    /// Tocarlo sigue valiendo como atajo para el que ya sabe.
    /// </summary>
    private IEnumerator Acompanar()
    {
        float dentro = 0f;
        int puntosPuestos = -1;

        while (!leHablaron && dentro < tiempoParaSeguir)
        {
            dentro = JugadorCerca(distanciaSaludo) ? dentro + Time.deltaTime : 0f;

            // Puntos suspensivos que crecen: sin esto la espera no se distingue
            // de que el guia este colgado. Solo se reescribe cuando cambian,
            // porque reasignar el texto regenera la malla de TMP.
            int puntos = Mathf.Clamp(Mathf.FloorToInt(dentro / tiempoParaSeguir * 4f), 0, 3);
            if (puntos != puntosPuestos)
            {
                puntosPuestos = puntos;
                Mostrar(invitacionRetos + new string('.', puntos));
            }

            yield return null;
        }
    }

    /// <summary>
    /// Cuanto se queda en pantalla una linea. Con voz manda el clip; sin voz,
    /// el largo del texto a ritmo de lectura, para poder probar la visita
    /// entera antes de tener un solo audio grabado.
    /// </summary>
    private float Duracion(Parlamento p)
    {
        if (p.voz != null)
            return p.voz.length;

        if (p.duracionSinVoz > 0f)
            return p.duracionSinVoz;

        return Mathf.Max(2.5f, p.texto.Length / 14f);
    }

    private void Mostrar(string texto)
    {
        if (bocadillo != null)
            bocadillo.Mostrar(texto);
    }

    // -------------------------------------------------------------- caminar

    private IEnumerator Caminar(Transform[] ruta)
    {
        if (ruta == null)
            yield break;

        caminando = true;

        foreach (Transform punto in ruta)
        {
            if (punto == null)
                continue;

            // La altura no cuenta: el guia camina por el piso, no vuela hacia
            // el punto si alguien lo dejo a media altura.
            Vector3 destino = new Vector3(punto.position.x, transform.position.y, punto.position.z);

            while (Vector3.Distance(transform.position, destino) > toleranciaPunto)
            {
                // Si el jugador se quedo atras, lo espera mirandolo.
                if (!JugadorCerca(distanciaEspera))
                {
                    if (jugador != null)
                        GirarHacia(jugador.position - transform.position);

                    yield return null;
                    continue;
                }

                GirarHacia(destino - transform.position);
                transform.position = Vector3.MoveTowards(
                    transform.position, destino, velocidad * Time.deltaTime);

                Bambolear();
                yield return null;
            }
        }

        Asentar();
        caminando = false;
    }

    private void GirarHacia(Vector3 direccion)
    {
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(direccion),
            velocidadGiro * Time.deltaTime);
    }

    /// <summary>
    /// Sube y baja el cuerpo al ritmo del avance. Va sobre el hijo, nunca sobre
    /// la raiz: la raiz es la que sigue la ruta, y moverle la altura la sacaria
    /// del piso.
    /// </summary>
    private void Bambolear()
    {
        if (cuerpo == null || bamboleo <= 0f)
            return;

        paso += Time.deltaTime * velocidad * 4f;
        cuerpo.localPosition = new Vector3(
            cuerpo.localPosition.x,
            alturaCuerpo + Mathf.Abs(Mathf.Sin(paso)) * bamboleo,
            cuerpo.localPosition.z);
    }

    /// <summary>Deja el cuerpo a su altura de reposo al terminar de caminar.</summary>
    private void Asentar()
    {
        if (cuerpo == null)
            return;

        cuerpo.localPosition = new Vector3(
            cuerpo.localPosition.x, alturaCuerpo, cuerpo.localPosition.z);
    }

    private bool JugadorCerca(float limite)
    {
        if (jugador == null)
            return true;

        // Distancia en planta: que el jugador se agache no tiene que contar.
        Vector3 aqui = transform.position;
        Vector3 alla = jugador.position;
        aqui.y = 0f;
        alla.y = 0f;

        return Vector3.Distance(aqui, alla) <= limite;
    }

    private void OnDrawGizmosSelected()
    {
        DibujarRuta(rutaPractica, Color.cyan, transform.position);

        if (rutaPractica != null && rutaPractica.Length > 0)
        {
            Transform ultimo = rutaPractica[rutaPractica.Length - 1];
            if (ultimo != null)
                DibujarRuta(rutaRetos, Color.yellow, ultimo.position);
        }
    }

    private void DibujarRuta(Transform[] ruta, Color color, Vector3 desde)
    {
        if (ruta == null)
            return;

        Gizmos.color = color;

        foreach (Transform punto in ruta)
        {
            if (punto == null)
                continue;

            Gizmos.DrawLine(desde, punto.position);
            Gizmos.DrawWireSphere(punto.position, 0.2f);
            desde = punto.position;
        }
    }
}
