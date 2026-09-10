using UnityEngine;

/// <summary>
/// Circuito serie de una malla: batería, interruptor y bombilla.
/// El brillo sale de la ley de Ohm, así que subir el voltaje o bajar la
/// resistencia se nota de verdad en la bombilla.
/// </summary>
public class CircuitoSimple : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Bombilla bombilla;

    [Header("Valores eléctricos")]
    [Tooltip("Voltaje de la batería, en voltios (V).")]
    [SerializeField] private float voltaje = 9f;

    [Tooltip("Resistencia de la bombilla, en ohmios (Ω).")]
    [SerializeField] private float resistencia = 18f;

    [Tooltip("Potencia a la que la bombilla llega a su brillo máximo, en vatios (W).")]
    [SerializeField] private float potenciaNominal = 4.5f;

    [Header("Estado")]
    [Tooltip("Si el interruptor está cerrado, circula corriente.")]
    [SerializeField] private bool cerrado;

    /// <summary>true si el interruptor deja pasar corriente.</summary>
    public bool Cerrado => cerrado;

    /// <summary>Voltaje de la batería, en voltios. Al cambiarlo se recalcula el brillo.</summary>
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

    /// <summary>Ley de Ohm: I = V / R. Con el circuito abierto no circula nada.</summary>
    public float Corriente => (cerrado && resistencia > 0f) ? voltaje / resistencia : 0f;

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
