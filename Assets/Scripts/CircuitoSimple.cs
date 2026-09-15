using UnityEngine;

/// <summary>
/// Circuito serie de una malla: bateria, interruptor y bombilla.
/// El brillo sale de la ley de Ohm, asi que subir el voltaje o bajar la
/// resistencia se nota de verdad en la bombilla. Pasarse del voltaje que
/// aguanta la quema.
///
/// Los tres componentes de abajo son opcionales y no se estorban entre si:
/// un diodo obliga a respetar la polaridad y se come su caida de tension,
/// un condensador hace que al abrir el circuito la bombilla baje poco a poco
/// en vez de cortarse, y un motor gira con la corriente que le llega.
/// Sin ninguno de ellos asignado, el circuito se comporta como siempre.
/// </summary>
public class CircuitoSimple : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Bombilla bombilla;

    [Tooltip("Opcional. Gira con la corriente del circuito.")]
    [SerializeField] private Motor motor;

    [Tooltip("Opcional. Obliga a respetar la polaridad y resta su caida de tension.")]
    [SerializeField] private Diodo diodo;

    [Tooltip("Opcional. Sostiene el brillo un rato despues de abrir el circuito.")]
    [SerializeField] private Condensador condensador;

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

    [Tooltip("Pila puesta del reves. Solo cambia algo si hay un diodo o un LED.")]
    [SerializeField] private bool polaridadInvertida;

    // Brillo que tenia la bombilla justo antes de cortarse la corriente.
    // Es el punto desde el que arranca el apagado lento del condensador.
    private float brilloSostenido;

    /// <summary>true si el interruptor deja pasar corriente.</summary>
    public bool Cerrado => cerrado;

    /// <summary>true si la pila esta puesta al reves.</summary>
    public bool PolaridadInvertida => polaridadInvertida;

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

    /// <summary>Motor conectado. Al desconectarlo se para.</summary>
    public Motor Maquina
    {
        get => motor;
        set
        {
            if (motor == value)
                return;

            if (motor != null)
                motor.AplicarCorriente(0f);

            motor = value;
            Actualizar();
        }
    }

    /// <summary>Diodo o LED conectado. Al desconectarlo se apaga su luz.</summary>
    public Diodo Valvula
    {
        get => diodo;
        set
        {
            if (diodo == value)
                return;

            if (diodo != null)
                diodo.MostrarPaso(0f);

            diodo = value;
            Actualizar();
        }
    }

    /// <summary>Condensador conectado. Al desconectarlo pierde la carga.</summary>
    public Condensador Deposito
    {
        get => condensador;
        set
        {
            if (condensador == value)
                return;

            if (condensador != null)
                condensador.Vaciar();

            condensador = value;
            brilloSostenido = 0f;
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

    /// <summary>Un diodo montado al reves de como esta la pila no deja pasar nada.</summary>
    public bool PolaridadCorrecta => diodo == null || diodo.Conduce(polaridadInvertida);

    /// <summary>true cuando lo unico que frena la corriente es la polaridad. Para avisar en el panel.</summary>
    public bool BloqueadoPorPolaridad => cerrado && HayCarga && !FocoRoto && !PolaridadCorrecta;

    /// <summary>
    /// Brillo que se le esta aplicando a la bombilla, de 0 a 1. Se guarda al
    /// calcularlo en vez de recalcularlo: BrilloAhora() tiene efecto de lado
    /// sobre el condensador y no puede llamarse desde fuera sin ensuciarlo.
    /// </summary>
    public float Brillo { get; private set; }

    /// <summary>Carga del condensador, de 0 a 1. Sin condensador, 0.</summary>
    public float CargaCondensador => condensador != null ? condensador.Carga : 0f;

    /// <summary>Lo que queda de tension despues de la caida del diodo.</summary>
    public float VoltajeUtil => diodo != null
        ? Mathf.Max(0f, voltaje - diodo.CaidaDirecta)
        : voltaje;

    /// <summary>Hace falta algo que consuma: una bombilla, un motor, o los dos.</summary>
    private bool HayCarga => bombilla != null || motor != null;

    private bool FocoRoto => bombilla != null && bombilla.Quemada;

    /// <summary>Una bombilla quemada corta el circuito, asi que no circula corriente.</summary>
    private bool HayPaso => cerrado && HayCarga && !FocoRoto && PolaridadCorrecta;

    /// <summary>
    /// Un condensador intercalado en el lazo deja pasar mientras se carga y
    /// despues corta: con corriente continua, un condensador cargado es un
    /// circuito abierto. En paralelo (lo normal aca) no frena nada.
    /// </summary>
    private float PasoCondensador => (condensador != null && condensador.EnSerie)
        ? Mathf.Clamp01(1f - condensador.Carga)
        : 1f;

    /// <summary>Ley de Ohm: I = V / R.</summary>
    public float Corriente => (HayPaso && resistencia > 0f)
        ? VoltajeUtil / resistencia * PasoCondensador
        : 0f;

    /// <summary>Potencia disipada: P = V por I.</summary>
    public float Potencia => VoltajeUtil * Corriente;

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

    /// <summary>Da la vuelta a la pila.</summary>
    public void InvertirPolaridad()
    {
        polaridadInvertida = !polaridadInvertida;

        if (BloqueadoPorPolaridad)
            SonidosCircuito.Reproducir(SonidoLab.DiodoBloqueado, this);

        Actualizar();
    }

    /// <summary>Fuerza un sentido concreto de la pila.</summary>
    public void EstablecerPolaridad(bool invertida)
    {
        if (polaridadInvertida == invertida)
            return;

        polaridadInvertida = invertida;
        Actualizar();
    }

    // El condensador es lo unico del circuito que depende del tiempo. Sin el,
    // este Update sale en la primera linea y todo sigue siendo por eventos.
    private void Update()
    {
        if (condensador == null)
            return;

        if (HayPaso)
            condensador.Cargar(Time.deltaTime, resistencia);
        else
            condensador.Descargar(Time.deltaTime, resistencia);

        Actualizar();
    }

    private void Actualizar()
    {
        // Sobretension: con el circuito cerrado y demasiado voltaje, la bombilla no aguanta.
        if (bombilla != null && cerrado && !bombilla.Quemada && PolaridadCorrecta && VoltajeUtil > voltajeMaximo)
            bombilla.Quemar();

        Brillo = BrilloAhora();

        if (bombilla != null)
            bombilla.AplicarBrillo(Brillo);

        if (motor != null)
            motor.AplicarCorriente(Corriente);

        if (diodo != null)
            diodo.MostrarPaso(Corriente);
    }

    private float BrilloAhora()
    {
        float directo = potenciaNominal > 0f ? Mathf.Clamp01(Potencia / potenciaNominal) : 0f;

        if (HayPaso)
        {
            // Mientras circula corriente el condensador solo acompana:
            // el brillo lo manda la ley de Ohm, y se guarda para el apagado.
            brilloSostenido = directo;
            return directo;
        }

        // El que sostiene el brillo es el condensador en paralelo. Uno en serie
        // no alimenta nada: cuando el lazo se abre, se apaga y punto.
        if (condensador == null || condensador.EnSerie || FocoRoto)
        {
            brilloSostenido = 0f;
            return 0f;
        }

        // Circuito abierto con condensador: la bombilla vive de lo que quedo guardado.
        return brilloSostenido * condensador.Carga;
    }

    // Permite tocar voltaje o resistencia desde el Inspector en Play y verlo al instante.
    private void OnValidate()
    {
        if (Application.isPlaying)
            Actualizar();
    }
}
