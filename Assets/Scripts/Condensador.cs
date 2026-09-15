using UnityEngine;

/// <summary>
/// Condensador fisico: guarda energia mientras pasa corriente y la suelta cuando
/// se abre el circuito, asi la bombilla no se corta de golpe sino que baja poco a poco.
///
/// La carga es un valor 0..1 y se mueve con la curva real de un RC: la constante
/// de tiempo es tau = R por C, asi que con mas resistencia tarda mas en cargarse
/// y tambien alumbra mas rato al descargarse. Cambiar la resistencia de la mesa
/// cambia la duracion del apagado, que es justo lo que hay que ver.
///
/// Solo guarda el estado; quien lo hace avanzar es <see cref="CircuitoSimple"/>.
/// </summary>
public class Condensador : MonoBehaviour
{
    [Header("Valor")]
    [Tooltip("Capacidad en faradios. 0.1 F con 18 ohmios da algo menos de 2 s de apagado.")]
    [SerializeField] private float faradios = 0.1f;

    [Tooltip("Montado en serie dentro del lazo. Con corriente continua deja pasar " +
             "mientras se carga y despues corta. Desmarcado va en paralelo con la carga, " +
             "que es lo que sostiene la bombilla al abrir el interruptor.")]
    [SerializeField] private bool enSerie;

    [Header("Aviso visual")]
    [Tooltip("LED de carga. Opcional: es una Bombilla pequena con otro color.")]
    [SerializeField] private Bombilla indicadorCarga;

    private bool avisoLleno;

    /// <summary>Capacidad, siempre positiva.</summary>
    public float Faradios => Mathf.Max(0.0001f, faradios);

    /// <summary>Cuanta energia tiene guardada, de 0 (vacio) a 1 (lleno).</summary>
    public float Carga { get; private set; }

    /// <summary>true si esta intercalado en el lazo en vez de en paralelo con la carga.</summary>
    public bool EnSerie => enSerie;

    /// <summary>Constante de tiempo del circuito: tau = R por C, en segundos.</summary>
    public float Constante(float resistencia)
    {
        return Mathf.Max(0.05f, Mathf.Max(0f, resistencia) * Faradios);
    }

    /// <summary>Lo carga durante un intervalo. Curva exponencial, como el componente real.</summary>
    public void Cargar(float intervalo, float resistencia)
    {
        float tau = Constante(resistencia);
        Carga = 1f - (1f - Carga) * Mathf.Exp(-intervalo / tau);

        if (!avisoLleno && Carga > 0.98f)
        {
            avisoLleno = true;
            SonidosCircuito.Reproducir(SonidoLab.CondensadorCargado, this);
        }

        Refrescar();
    }

    /// <summary>Lo descarga durante un intervalo.</summary>
    public void Descargar(float intervalo, float resistencia)
    {
        float tau = Constante(resistencia);
        Carga *= Mathf.Exp(-intervalo / tau);

        if (Carga < 0.001f)
            Carga = 0f;

        if (Carga < 0.5f)
            avisoLleno = false;

        Refrescar();
    }

    /// <summary>Lo deja vacio de golpe, por ejemplo al sacarlo del zocalo.</summary>
    public void Vaciar()
    {
        Carga = 0f;
        avisoLleno = false;
        Refrescar();
    }

    private void Refrescar()
    {
        if (indicadorCarga != null)
            indicadorCarga.AplicarBrillo(Carga);
    }
}
