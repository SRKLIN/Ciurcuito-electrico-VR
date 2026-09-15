using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Lista de objetivos para el estudiante, en orden. Cada reto pide dejar el
/// brillo de la bombilla dentro de un rango y se da por cumplido cuando la
/// condicion se sostiene un momento, para que no cuente un roce al pasar.
///
/// Los tres retos de la mesa forman un arco a proposito: primero armar el
/// circuito, despues llevarlo al maximo, y al final dejarlo encendido pero
/// bajo. El tercero es el que obliga a entender, porque no se resuelve
/// poniendo la pila mas grande.
///
/// No calcula nada electrico: lee <see cref="CircuitoSimple.Brillo"/>.
/// </summary>
public class ControlRetos : MonoBehaviour
{
    /// <summary>Un objetivo: el brillo tiene que quedar entre los dos valores.</summary>
    [System.Serializable]
    public class Reto
    {
        [Tooltip("Lo que se le pide al estudiante, en una linea.")]
        public string descripcion = "Enciende la bombilla.";

        [Range(0f, 1f)] public float brilloMinimo = 0.01f;
        [Range(0f, 1f)] public float brilloMaximo = 1f;

        // Sin serializar: asi cada Play arranca con los retos sin cumplir.
        [System.NonSerialized] public bool cumplido;
    }

    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;
    [SerializeField] private TMP_Text panel;

    [Header("Retos")]
    [SerializeField] private Reto[] retos;

    [Tooltip("Segundos que hay que sostener la condicion para darla por buena.")]
    [SerializeField] private float tiempoSostenido = 0.6f;

    [Header("Textos")]
    [SerializeField] private string tituloEnCurso = "RETOS";
    [SerializeField] private string tituloFinal = "TODOS SUPERADOS";
    [SerializeField] private bool mostrarBrillo = true;

    private readonly StringBuilder texto = new StringBuilder();
    private int actual;
    private float sostenido;
    private string ultimoTexto;

    /// <summary>Indice del reto en curso. Si llega al total, estan todos hechos.</summary>
    public int Actual => actual;

    /// <summary>true cuando no queda ninguno pendiente.</summary>
    public bool Completos => retos == null || actual >= retos.Length;

    private void Update()
    {
        if (circuito == null)
            return;

        Comprobar();
        Refrescar();
    }

    private void Comprobar()
    {
        if (Completos)
            return;

        Reto reto = retos[actual];
        float brillo = circuito.Brillo;

        if (brillo < reto.brilloMinimo || brillo > reto.brilloMaximo)
        {
            sostenido = 0f;
            return;
        }

        sostenido += Time.deltaTime;

        if (sostenido < tiempoSostenido)
            return;

        reto.cumplido = true;
        sostenido = 0f;
        actual++;

        SonidosCircuito.Reproducir(Completos ? SonidoLab.RetosCompletos : SonidoLab.RetoSuperado, this);
    }

    /// <summary>Vuelve a dejar todos los retos por hacer.</summary>
    public void Reiniciar()
    {
        if (retos != null)
            foreach (var r in retos)
                r.cumplido = false;

        actual = 0;
        sostenido = 0f;
    }

    private void Refrescar()
    {
        if (panel == null || retos == null)
            return;

        string nuevo = Componer();

        if (nuevo == ultimoTexto)
            return;

        ultimoTexto = nuevo;
        panel.text = nuevo;
    }

    private string Componer()
    {
        texto.Clear();
        texto.Append(Completos ? tituloFinal : tituloEnCurso);

        for (int i = 0; i < retos.Length; i++)
        {
            // Corchetes en vez de un tilde: no todas las fuentes traen el glifo,
            // y una caja vacia en la pizarra se lee como un error.
            string marca = retos[i].cumplido ? "[X]" : (i == actual ? "[>]" : "[  ]");
            texto.Append('\n').Append(marca).Append(' ').Append(retos[i].descripcion);
        }

        if (mostrarBrillo)
            texto.Append("\n\nbrillo ahora: ").Append((circuito.Brillo * 100f).ToString("0")).Append(" %");

        return texto.ToString();
    }
}
