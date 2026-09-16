using UnityEngine;

/// <summary>
/// Puerta corredera de dos hojas, como las de un supermercado: se abre sola
/// cuando el jugador se acerca y se cierra cuando se aleja.
///
/// La deteccion es por distancia en el plano, ignorando la altura, para que
/// agacharse o estirarse no cuente. Y abre y cierra a distancias distintas
/// (histeresis): sin eso, quedarse parado justo en el limite hace que la puerta
/// tiemble abriendose y cerrandose sin parar.
///
/// Las hojas llevan su propio collider, asi que cerrada frena de verdad al
/// CharacterController del rig: no hace falta nada mas para que no se pase.
/// </summary>
public class PuertaAutomatica : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform hojaIzquierda;
    [SerializeField] private Transform hojaDerecha;

    [Tooltip("A quien detecta. Vacio = busca la camara principal al arrancar.")]
    [SerializeField] private Transform jugador;

    [Tooltip("Otros que tambien la abren, como el guia. Vacio = solo el jugador.")]
    [SerializeField] private Transform[] tambienAbren;

    [Header("Comportamiento")]
    [Tooltip("A esta distancia o menos, la puerta se abre.")]
    [SerializeField] private float distanciaApertura = 2.5f;

    [Tooltip("Cuanto mas lejos hay que irse para que se cierre. Evita el temblequeo.")]
    [SerializeField] private float histeresis = 0.5f;

    [Tooltip("Cuanto se corre cada hoja, en metros.")]
    [SerializeField] private float recorrido = 0.9f;

    [Tooltip("Metros por segundo del deslizamiento.")]
    [SerializeField] private float velocidad = 2.2f;

    private Vector3 cerradaIzquierda;
    private Vector3 cerradaDerecha;
    private float apertura;
    private bool abierta;

    /// <summary>0 cerrada del todo, 1 abierta del todo.</summary>
    public float Apertura => apertura;

    private void Awake()
    {
        if (hojaIzquierda != null)
            cerradaIzquierda = hojaIzquierda.localPosition;

        if (hojaDerecha != null)
            cerradaDerecha = hojaDerecha.localPosition;

        if (jugador == null && Camera.main != null)
            jugador = Camera.main.transform;
    }

    private void Update()
    {
        float cerca = DistanciaAlMasCercano();

        if (float.IsPositiveInfinity(cerca))
            return;

        float limite = abierta ? distanciaApertura + histeresis : distanciaApertura;
        bool deberiaAbrir = cerca <= limite;

        if (deberiaAbrir != abierta)
        {
            abierta = deberiaAbrir;
            SonidosCircuito.Reproducir(abierta ? SonidoLab.PuertaAbre : SonidoLab.PuertaCierra, this);
        }

        float objetivo = abierta ? 1f : 0f;

        if (Mathf.Approximately(apertura, objetivo))
            return;

        apertura = Mathf.MoveTowards(apertura, objetivo, velocidad * Time.deltaTime);

        if (hojaIzquierda != null)
            hojaIzquierda.localPosition = cerradaIzquierda + Vector3.left * (recorrido * apertura);

        if (hojaDerecha != null)
            hojaDerecha.localPosition = cerradaDerecha + Vector3.right * (recorrido * apertura);
    }

    /// <summary>
    /// Lo mas cerca que hay alguien que pueda abrirla. Infinito si no hay nadie
    /// asignado. La puerta la abre el primero que llegue, sea el jugador o el guia.
    /// </summary>
    private float DistanciaAlMasCercano()
    {
        float menor = float.PositiveInfinity;

        menor = Mathf.Min(menor, DistanciaEnPlanta(jugador));

        if (tambienAbren != null)
            foreach (Transform quien in tambienAbren)
                menor = Mathf.Min(menor, DistanciaEnPlanta(quien));

        return menor;
    }

    /// <summary>
    /// Distancia ignorando la altura: agacharse o estirarse no tiene que contar.
    /// </summary>
    private float DistanciaEnPlanta(Transform quien)
    {
        if (quien == null)
            return float.PositiveInfinity;

        Vector3 aqui = transform.position;
        Vector3 alla = quien.position;
        aqui.y = 0f;
        alla.y = 0f;

        return Vector3.Distance(aqui, alla);
    }
}
