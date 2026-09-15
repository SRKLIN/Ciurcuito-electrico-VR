using UnityEngine;

/// <summary>
/// Motor de corriente continua. Es parte visual: recibe la corriente que le llega
/// y hace girar la helice mas o menos rapido. No calcula nada electrico; de eso se
/// encarga <see cref="CircuitoSimple"/>.
///
/// Dos detalles que lo hacen creible sin complicarlo: por debajo de una corriente
/// minima no arranca, como uno de verdad que zumba pero no llega a moverse, y
/// tiene inercia, asi que ni arranca ni frena de golpe.
///
/// La resistencia interna del motor no vive aca: la pieza lleva ademas un
/// <see cref="ValorResistencia"/>, igual que una resistencia normal, y el zocalo
/// la recoge sola.
/// </summary>
public class Motor : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Lo que gira. Normalmente la helice, no el cuerpo del motor.")]
    [SerializeField] private Transform helice;

    [Header("Giro")]
    [Tooltip("Eje de giro en coordenadas locales de la helice.")]
    [SerializeField] private Vector3 eje = Vector3.forward;

    [Tooltip("Vueltas por minuto a la corriente nominal.")]
    [SerializeField] private float rpmMaximas = 900f;

    [Tooltip("Corriente, en amperios, a la que el motor gira a tope.")]
    [SerializeField] private float corrienteNominal = 0.5f;

    [Tooltip("Por debajo de esta corriente no llega a arrancar.")]
    [SerializeField] private float corrienteMinima = 0.06f;

    [Tooltip("Cuanto mas alto, antes coge y suelta las vueltas.")]
    [SerializeField] private float inercia = 2.5f;

    [Header("Sonido")]
    [Tooltip("AudioSource propio para el giro en bucle. Opcional.")]
    [SerializeField] private AudioSource fuenteGiro;

    [Tooltip("Clip en bucle del motor girando. Vacio = sin sonido.")]
    [SerializeField] private AudioClip giro;

    [Range(0f, 1f)]
    [SerializeField] private float volumenMaximo = 0.4f;

    [SerializeField] private float tonoMinimo = 0.7f;
    [SerializeField] private float tonoMaximo = 1.3f;

    private float marchaObjetivo;
    private float marcha;
    private bool girando;

    /// <summary>Velocidad actual normalizada, de 0 a 1.</summary>
    public float Marcha => marcha;

    private void Awake()
    {
        if (fuenteGiro != null)
        {
            fuenteGiro.clip = giro;
            fuenteGiro.loop = true;
            fuenteGiro.playOnAwake = false;
            fuenteGiro.volume = 0f;
        }
    }

    /// <summary>Le pasa la corriente que circula, en amperios.</summary>
    public void AplicarCorriente(float amperios)
    {
        if (amperios < corrienteMinima)
        {
            marchaObjetivo = 0f;
            return;
        }

        marchaObjetivo = corrienteNominal > 0f ? Mathf.Clamp01(amperios / corrienteNominal) : 0f;
    }

    private void Update()
    {
        marcha = Mathf.Lerp(marcha, marchaObjetivo, 1f - Mathf.Exp(-inercia * Time.deltaTime));

        if (marcha < 0.001f)
            marcha = 0f;

        if (helice != null && marcha > 0f)
        {
            // De rpm a grados por segundo: una vuelta son 360 grados y un minuto 60 s.
            float gradosPorSegundo = rpmMaximas * marcha * 6f;
            helice.Rotate(eje.normalized, gradosPorSegundo * Time.deltaTime, Space.Self);
        }

        AvisarArranque();
        SonarGiro();
    }

    private void AvisarArranque()
    {
        bool ahoraGira = marcha > 0.05f;

        if (ahoraGira == girando)
            return;

        girando = ahoraGira;
        SonidosCircuito.Reproducir(girando ? SonidoLab.MotorArranca : SonidoLab.MotorPara, this);
    }

    private void SonarGiro()
    {
        if (fuenteGiro == null || giro == null)
            return;

        bool deberiaSonar = marcha > 0.01f;

        if (deberiaSonar && !fuenteGiro.isPlaying)
            fuenteGiro.Play();
        else if (!deberiaSonar && fuenteGiro.isPlaying)
            fuenteGiro.Stop();

        fuenteGiro.volume = volumenMaximo * marcha;
        fuenteGiro.pitch = Mathf.Lerp(tonoMinimo, tonoMaximo, marcha);
    }
}
