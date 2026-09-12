using TMPro;
using UnityEngine;

/// <summary>
/// Multimetro de la mesa: muestra voltaje, resistencia, corriente y potencia
/// del circuito. Solo redibuja cuando algun valor cambia, para no generar
/// basura en cada frame.
/// </summary>
public class PanelMedidas : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;
    [SerializeField] private TMP_Text display;

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
        return "V   " + circuito.Voltaje.ToString("0.0") + " V\n"
             + "R   " + circuito.Resistencia.ToString("0") + " Ω\n"
             + "I   " + circuito.Corriente.ToString("0.00") + " A\n"
             + "P   " + circuito.Potencia.ToString("0.0") + " W";
    }
}
