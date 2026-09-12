using UnityEngine;

/// <summary>
/// Circuito serie de una malla: bateria, interruptor y bombilla.
/// El brillo sale de la ley de Ohm, asi que subir el voltaje o bajar la
/// resistencia se nota de verdad en la bombilla. Pasarse del voltaje que
/// aguanta la quema.
/// </summary>
public class CircuitoSimple : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Bombilla bombilla;

    [Header("Valores electricos")]
    [Tooltip("Voltaje de la bateria, en voltios (V).")]
    [SerializeField] private float voltaje = 9f;

    [Tooltip("Resistencia de la bombilla, en ohmios.")]
    [SerializeField] private float resistencia = 18f;

    [Tooltip("Potencia a la que la bombilla llega a su brillo maximo, en vatios (W).")]
    [SerializeField] private float potenciaNominal = 4.5f;

    [Tooltip("Voltaje maximo que aguanta la bombilla. Por encima se quema.")]
    [SerializeField] private float voltajeMaximo = 12f;

    [Header("Estado")]
    [Tooltip("Si el interruptor esta cerrado, circula corriente.")]
    [SerializeField] private bool cerrado;

    /// <summary>true si el interruptor deja pasar corriente.</summary>
    public bool Cerrado => cerrado;

    /// <summary>Bombilla conectada. Cambiarla recalcula el circuito.</summary>
    public Bombilla Foco
    {
        get => bombilla;
        set
        {
            if (bombilla == value)
                return;

            // La que se va tiene que apagarse aca: una vez desconectada
            // Actualizar() ya no la alcanza y se quedaria encendida para siempre.
            if (bombilla != null)
                bombilla.AplicarBrillo(0f);

            bombilla = value;
            Actualizar();
        }
    }

    /// <summary>Voltaje de la bateria, en voltios. Al cambiarlo se recalcula el brillo.</summary>
    public float Voltaje
    {
        get => voltaje;
        set
        {
            voltaje = Mathf.Max(0f, value);
            Actualizar();
        }
    }

    /// <summary>Resistencia de la bombilla, en ohmios.</summary>
    public float Resistencia
    {
        get => resistencia;
        set
        {
            resistencia = Mathf.Max(0.0001f, value);
            Actualizar();
        }
    }

    /// <summary>Una bombilla quemada corta el circuito, asi que no circula corriente.</summary>
    private bool HayPaso => cerrado && bombilla != null && !bombilla.Quemada;

    /// <summary>Ley de Ohm: I = V / R.</summary>
    public float Corriente => (HayPaso && resistencia > 0f) ? voltaje / resistencia : 0f;

    /// <summary>Potencia disipada: P = V · I.</summary>
    public float Potencia => voltaje * Corriente;

    private void Start()
    {
        Actualizar();
    }

    /// <summary>Abre el circuito si estaba cerrado y viceversa.</summary>
    public void Alternar()
    {
        cerrado = !cerrado;
        Actualizar();
    }

    /// <summary>Fuerza un estado concreto del interruptor.</summary>
    public void Establecer(bool circuitoCerrado)
    {
        cerrado = circuitoCerrado;
        Actualizar();
    }

    private void Actualizar()
    {
        if (bombilla == null)
            return;

        // Sobretension: con el circuito cerrado y demasiado voltaje, la bombilla no aguanta.
        if (cerrado && !bombilla.Quemada && voltaje > voltajeMaximo)
            bombilla.Quemar();

        float brillo = potenciaNominal > 0f ? Mathf.Clamp01(Potencia / potenciaNominal) : 0f;
        bombilla.AplicarBrillo(brillo);
    }

    // Permite tocar voltaje o resistencia desde el Inspector en Play y verlo al instante.
    private void OnValidate()
    {
        if (Application.isPlaying)
            Actualizar();
    }
}
