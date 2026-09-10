using TMPro;
using UnityEngine;

/// <summary>
/// Fuente de alimentacion variable: sube y baja el voltaje del circuito
/// y lo muestra en un display.
/// </summary>
public class ControlVoltaje : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;
    [SerializeField] private TMP_Text display;

    [Header("Rango")]
    [SerializeField] private float voltajeMinimo = 0f;
    [SerializeField] private float voltajeMaximo = 12f;

    [Tooltip("Cuanto cambia el voltaje con cada toque, en voltios.")]
    [SerializeField] private float paso = 0.5f;

    private void Start()
    {
        RefrescarDisplay();
    }

    /// <summary>Sube el voltaje un paso.</summary>
    public void Subir()
    {
        Ajustar(paso);
    }

    /// <summary>Baja el voltaje un paso.</summary>
    public void Bajar()
    {
        Ajustar(-paso);
    }

    private void Ajustar(float delta)
    {
        if (circuito == null)
            return;

        circuito.Voltaje = Mathf.Clamp(circuito.Voltaje + delta, voltajeMinimo, voltajeMaximo);
        RefrescarDisplay();
    }

    private void RefrescarDisplay()
    {
        if (display == null || circuito == null)
            return;

        display.text = circuito.Voltaje.ToString("0.0") + " V";
    }
}
