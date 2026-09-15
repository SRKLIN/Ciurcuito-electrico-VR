using UnityEngine;

/// <summary>Cada aviso sonoro que puede pedir el laboratorio.</summary>
public enum SonidoLab
{
    ClicInterruptor,
    BombillaQuemada,
    BombillaCambiada,
    BotonVoltaje,
    PiezaEncastrada,
    PiezaSacada,
    CambioPolaridad,
    DiodoBloqueado,
    MotorArranca,
    MotorPara,
    CondensadorCargado,
    RetoSuperado,
    RetosCompletos,
    Cortocircuito,
    PuertaAbre,
    PuertaCierra
}

/// <summary>
/// Central de sonido del laboratorio. Guarda un hueco por cada aviso; el audio
/// se arrastra al Inspector cuando se tenga. Todo lo que no tenga clip puesto
/// se ignora sin ruido ni errores, asi que el laboratorio funciona igual con
/// la lista entera vacia.
///
/// Se llama desde cualquier script con una linea, sin referencias cruzadas:
///     SonidosCircuito.Reproducir(SonidoLab.ClicInterruptor, transform.position);
///
/// Los sonidos continuos (el zumbido de la corriente, el giro del motor) NO van
/// aca: necesitan un AudioSource propio en bucle. Ver <see cref="ZumbidoCircuito"/>
/// y el AudioSource de <see cref="Motor"/>.
/// </summary>
public class SonidosCircuito : MonoBehaviour
{
    /// <summary>Un clip con su volumen. Dejarlo vacio desactiva ese aviso.</summary>
    [System.Serializable]
    public class Ranura
    {
        [Tooltip("Arrastrar aca el archivo de audio. Vacio = sin sonido.")]
        public AudioClip clip;

        [Range(0f, 1f)] public float volumen = 1f;

        [Tooltip("Variacion de tono al azar, para que repetirlo no canse. 0 = siempre igual.")]
        [Range(0f, 0.5f)] public float variacionTono = 0.05f;
    }

    [Header("Salida")]
    [Tooltip("Fuente para los avisos que no van pegados a un sitio. Opcional.")]
    [SerializeField] private AudioSource fuente;

    [Tooltip("Marcado, cada aviso suena en el punto donde ocurre (3D). Desmarcado, todo sale por la fuente.")]
    [SerializeField] private bool sonarEnElSitio = true;

    [Header("Interruptor y fuente")]
    [SerializeField] private Ranura clicInterruptor = new Ranura();
    [SerializeField] private Ranura botonVoltaje = new Ranura();

    [Header("Bombilla")]
    [SerializeField] private Ranura bombillaQuemada = new Ranura();
    [SerializeField] private Ranura bombillaCambiada = new Ranura();

    [Header("Zocalos")]
    [SerializeField] private Ranura piezaEncastrada = new Ranura();
    [SerializeField] private Ranura piezaSacada = new Ranura();

    [Header("Componentes nuevos")]
    [SerializeField] private Ranura cambioPolaridad = new Ranura();
    [SerializeField] private Ranura diodoBloqueado = new Ranura();
    [SerializeField] private Ranura motorArranca = new Ranura();
    [SerializeField] private Ranura motorPara = new Ranura();
    [SerializeField] private Ranura condensadorCargado = new Ranura();

    [Header("Retos")]
    [SerializeField] private Ranura retoSuperado = new Ranura();
    [SerializeField] private Ranura retosCompletos = new Ranura();

    [Tooltip("Una pieza en el hueco equivocado. Un chasquido seco viene bien aca.")]
    [SerializeField] private Ranura cortocircuito = new Ranura();

    [Header("Puerta")]
    [SerializeField] private Ranura puertaAbre = new Ranura();
    [SerializeField] private Ranura puertaCierra = new Ranura();

    private static SonidosCircuito instancia;

    private void Awake()
    {
        // El ultimo que despierta manda. Con uno solo en la escena da igual.
        instancia = this;

        if (fuente == null)
            fuente = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        if (instancia == this)
            instancia = null;
    }

    /// <summary>Dispara un aviso en un punto del mundo. Sin central o sin clip, no hace nada.</summary>
    public static void Reproducir(SonidoLab cual, Vector3 posicion)
    {
        if (instancia != null)
            instancia.Lanzar(cual, posicion);
    }

    /// <summary>Atajo: suena donde este el componente que lo pide.</summary>
    public static void Reproducir(SonidoLab cual, Component origen)
    {
        Reproducir(cual, origen != null ? origen.transform.position : Vector3.zero);
    }

    private void Lanzar(SonidoLab cual, Vector3 posicion)
    {
        Ranura ranura = Buscar(cual);

        if (ranura == null || ranura.clip == null)
            return;

        if (sonarEnElSitio)
        {
            // Crea una fuente temporal en ese punto y se borra sola al terminar.
            AudioSource.PlayClipAtPoint(ranura.clip, posicion, ranura.volumen);
            return;
        }

        if (fuente == null)
            return;

        fuente.pitch = 1f + Random.Range(-ranura.variacionTono, ranura.variacionTono);
        fuente.PlayOneShot(ranura.clip, ranura.volumen);
    }

    private Ranura Buscar(SonidoLab cual)
    {
        switch (cual)
        {
            case SonidoLab.ClicInterruptor: return clicInterruptor;
            case SonidoLab.BombillaQuemada: return bombillaQuemada;
            case SonidoLab.BombillaCambiada: return bombillaCambiada;
            case SonidoLab.BotonVoltaje: return botonVoltaje;
            case SonidoLab.PiezaEncastrada: return piezaEncastrada;
            case SonidoLab.PiezaSacada: return piezaSacada;
            case SonidoLab.CambioPolaridad: return cambioPolaridad;
            case SonidoLab.DiodoBloqueado: return diodoBloqueado;
            case SonidoLab.MotorArranca: return motorArranca;
            case SonidoLab.MotorPara: return motorPara;
            case SonidoLab.CondensadorCargado: return condensadorCargado;
            case SonidoLab.RetoSuperado: return retoSuperado;
            case SonidoLab.RetosCompletos: return retosCompletos;
            case SonidoLab.Cortocircuito: return cortocircuito;
            case SonidoLab.PuertaAbre: return puertaAbre;
            case SonidoLab.PuertaCierra: return puertaCierra;
            default: return null;
        }
    }
}
