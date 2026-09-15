using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Multimetro de la mesa: muestra voltaje, resistencia, corriente y potencia
/// del circuito. Solo redibuja cuando algun valor cambia, para no generar
/// basura en cada frame.
///
/// Si la mesa tiene condensador o diodo, agrega sus lineas solas: la caida del
/// diodo, la carga guardada y el aviso de polaridad al reves. En una mesa sin
/// esas piezas el panel se ve exactamente igual que antes.
/// </summary>
public class PanelMedidas : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;
    [SerializeField] private TMP_Text display;

    [Header("Lineas extra")]
    [Tooltip("Muestra la carga del condensador cuando la mesa tiene uno.")]
    [SerializeField] private bool mostrarCarga = true;

    [Tooltip("Muestra la caida del diodo y avisa cuando la pila esta al reves.")]
    [SerializeField] private bool mostrarDiodo = true;

    private readonly StringBuilder texto = new StringBuilder();
    private string ultimaLectura;

    private void Update()
    {
        if (circuito == null || display == null)
            return;

        string lectura = Componer();

        if (lectura == ultimaLectura)
            return;

        ultimaLectura = lectura;
        display.text = lectura;
    }

    private string Componer()
    {
        texto.Clear();

        texto.Append("V   ").Append(circuito.Voltaje.ToString("0.0")).Append(" V\n");
        texto.Append("R   ").Append(circuito.Resistencia.ToString("0")).Append(" Ω\n");
        texto.Append("I   ").Append(circuito.Corriente.ToString("0.00")).Append(" A\n");
        texto.Append("P   ").Append(circuito.Potencia.ToString("0.0")).Append(" W");

        // Sin esta linea el panel parece equivocado: con un diodo puesto,
        // V / R no da la corriente que se lee, porque el diodo se queda con su parte.
        if (mostrarDiodo && circuito.Valvula != null)
            texto.Append("\nD   -").Append(circuito.Valvula.CaidaDirecta.ToString("0.0")).Append(" V");

        if (mostrarCarga && circuito.Deposito != null)
            texto.Append("\nC   ").Append((circuito.CargaCondensador * 100f).ToString("0")).Append(" %");

        if (mostrarDiodo && circuito.BloqueadoPorPolaridad)
            texto.Append("\nPILA AL REVES");

        return texto.ToString();
    }
}
