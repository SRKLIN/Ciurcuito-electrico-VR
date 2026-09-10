using UnityEngine;

/// <summary>
/// Parte visual de una bombilla: traduce un brillo normalizado (0..1) en
/// emisión del material y en intensidad de una luz real.
/// No sabe nada de electricidad; de eso se encarga <see cref="CircuitoSimple"/>.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class Bombilla : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Luz puntual que ilumina el entorno. Opcional: si está vacío solo brilla el material.")]
    [SerializeField] private Light luz;

    [Header("Apariencia")]
    [SerializeField] private Color colorEncendida = new Color(1f, 0.95f, 0.7f);

    [Tooltip("Intensidad de la luz cuando el brillo es 1.")]
    [SerializeField] private float intensidadLuzMaxima = 2f;

    [Tooltip("Multiplicador de emisión del material cuando el brillo es 1.")]
    [SerializeField] private float emisionMaxima = 4f;

    private Material material;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        // .material (no .sharedMaterial) crea una instancia propia,
        // así no se pisa el material del asset ni el de otras bombillas.
        material = GetComponent<Renderer>().material;
        material.EnableKeyword("_EMISSION");
        AplicarBrillo(0f);
    }

    /// <summary>Ajusta el brillo. 0 = apagada, 1 = brillo nominal.</summary>
    public void AplicarBrillo(float brillo)
    {
        brillo = Mathf.Max(0f, brillo);

        material.SetColor(EmissionColor, colorEncendida * emisionMaxima * brillo);

        if (luz != null)
        {
            luz.intensity = intensidadLuzMaxima * brillo;
            luz.enabled = brillo > 0.001f;
        }
    }
}
