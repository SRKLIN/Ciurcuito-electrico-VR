using UnityEngine;

/// <summary>
/// Diodo o LED: deja pasar la corriente en un solo sentido. Si la pila esta al
/// reves respecto de como esta montado el diodo, no circula nada y la bombilla
/// se queda apagada aunque el interruptor este cerrado.
///
/// Ademas se come su caida de tension (0.7 V en un diodo de silicio, unos 2 V en
/// un LED), asi que el resto del circuito trabaja con algo menos de voltaje. Con
/// pilas de 1.5 V eso se nota mucho, que es justo la gracia.
/// </summary>
public class Diodo : MonoBehaviour
{
    [Header("Valor")]
    [Tooltip("Tension que se pierde en el diodo al conducir, en voltios. 0.7 diodo, 2.0 LED rojo.")]
    [SerializeField] private float caidaDirecta = 0.7f;

    [Tooltip("Marcado, la pieza esta montada al reves: conduce con la pila invertida.")]
    [SerializeField] private bool montadoAlReves;

    [Header("Aviso visual")]
    [Tooltip("Cuerpo del LED, si esta pieza es un LED. Opcional.")]
    [SerializeField] private Bombilla luzLed;

    [Tooltip("Corriente, en amperios, a la que el LED llega a su brillo maximo.")]
    [SerializeField] private float corrienteNominal = 0.3f;

    /// <summary>Tension que se pierde al conducir.</summary>
    public float CaidaDirecta => Mathf.Max(0f, caidaDirecta);

    /// <summary>true si esta puesto del reves en el zocalo.</summary>
    public bool MontadoAlReves => montadoAlReves;

    /// <summary>Si esta montado al reves, conduce justo con la pila invertida.</summary>
    public bool Conduce(bool polaridadInvertida) => polaridadInvertida == montadoAlReves;

    /// <summary>Da la vuelta a la pieza sin tener que sacarla del zocalo.</summary>
    public void Girar()
    {
        montadoAlReves = !montadoAlReves;
    }

    /// <summary>Enciende el LED segun la corriente que lo atraviesa.</summary>
    public void MostrarPaso(float corriente)
    {
        if (luzLed == null)
            return;

        float brillo = corrienteNominal > 0f ? Mathf.Clamp01(corriente / corrienteNominal) : 0f;
        luzLed.AplicarBrillo(brillo);
    }
}
