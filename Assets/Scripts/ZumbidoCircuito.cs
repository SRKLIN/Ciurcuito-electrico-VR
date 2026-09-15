using UnityEngine;

/// <summary>
/// Zumbido continuo de un circuito con corriente. El volumen y el tono siguen a
/// la potencia, asi que se oye subir cuando la bombilla brilla mas.
///
/// Es el hueco para el audio en bucle: mientras no se le ponga clip, el script
/// no toca nada. Va en un GameObject propio dentro de la mesa, con su AudioSource.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ZumbidoCircuito : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;

    [Tooltip("Clip en bucle. Vacio = el zumbido queda desactivado.")]
    [SerializeField] private AudioClip zumbido;

    [Header("Respuesta")]
    [Tooltip("Potencia, en vatios, a la que el zumbido llega a su volumen maximo.")]
    [SerializeField] private float potenciaReferencia = 4.5f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenMaximo = 0.35f;

    [SerializeField] private float tonoMinimo = 0.85f;
    [SerializeField] private float tonoMaximo = 1.15f;

    [Tooltip("Cuanto mas alto, mas rapido sigue el sonido a los cambios de potencia.")]
    [SerializeField] private float suavizado = 6f;

    private AudioSource fuente;
    private float nivel;

    private void Awake()
    {
        fuente = GetComponent<AudioSource>();
        fuente.clip = zumbido;
        fuente.loop = true;
        fuente.playOnAwake = false;
        fuente.volume = 0f;
    }

    private void Update()
    {
        if (circuito == null || zumbido == null)
            return;

        float objetivo = potenciaReferencia > 0f
            ? Mathf.Clamp01(circuito.Potencia / potenciaReferencia)
            : 0f;

        nivel = Mathf.Lerp(nivel, objetivo, 1f - Mathf.Exp(-suavizado * Time.deltaTime));

        // Por debajo de esto el zumbido no se oiria igual: se para la fuente
        // para no dejar un AudioSource sonando en silencio todo el rato.
        bool deberiaSonar = nivel > 0.01f;

        if (deberiaSonar && !fuente.isPlaying)
            fuente.Play();
        else if (!deberiaSonar && fuente.isPlaying)
            fuente.Stop();

        fuente.volume = volumenMaximo * nivel;
        fuente.pitch = Mathf.Lerp(tonoMinimo, tonoMaximo, nivel);
    }
}
