using UnityEngine;

/// <summary>
/// Parte visual de una bombilla: traduce un brillo normalizado (0..1) en
/// emision del material y en intensidad de una luz real. Tambien sabe quemarse.
/// No sabe nada de electricidad; de eso se encarga <see cref="CircuitoSimple"/>.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class Bombilla : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Luz puntual que ilumina el entorno. Opcional.")]
    [SerializeField] private Light luz;

    [Tooltip("Particulas que se disparan al quemarse. Opcional.")]
    [SerializeField] private ParticleSystem efectoExplosion;

    [Header("Apariencia")]
    [SerializeField] private Color colorEncendida = new Color(1f, 0.95f, 0.7f);

    [Tooltip("Color del vidrio una vez quemada.")]
    [SerializeField] private Color colorQuemada = new Color(0.18f, 0.16f, 0.15f);

    [SerializeField] private float intensidadLuzMaxima = 2f;
    [SerializeField] private float emisionMaxima = 4f;

    private Material material;
    private Color colorOriginal;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    /// <summary>true si se paso de voltaje y ya no sirve.</summary>
    public bool Quemada { get; private set; }

    private void Awake()
    {
        // .material (no .sharedMaterial) crea una instancia propia,
        // asi no se pisa el material del asset ni el de otras bombillas.
        material = GetComponent<Renderer>().material;
        material.EnableKeyword("_EMISSION");
        colorOriginal = LeerColorBase();
        AplicarBrillo(0f);
    }

    /// <summary>Ajusta el brillo. 0 = apagada, 1 = brillo nominal. Una bombilla quemada ignora esto.</summary>
    public void AplicarBrillo(float brillo)
    {
        if (Quemada)
            brillo = 0f;

        brillo = Mathf.Max(0f, brillo);

        material.SetColor(EmissionColor, colorEncendida * emisionMaxima * brillo);

        if (luz != null)
        {
            luz.intensity = intensidadLuzMaxima * brillo;
            luz.enabled = brillo > 0.001f;
        }
    }

    /// <summary>La quema: se apaga, se ennegrece y dispara las particulas.</summary>
    public void Quemar()
    {
        if (Quemada)
            return;

        Quemada = true;
        AplicarBrillo(0f);
        PintarBase(colorQuemada);

        if (efectoExplosion != null)
            efectoExplosion.Play();
    }

    /// <summary>La reemplaza por una nueva.</summary>
    public void Reparar()
    {
        if (!Quemada)
            return;

        Quemada = false;
        PintarBase(colorOriginal);
    }

    private Color LeerColorBase()
    {
        return material.HasProperty(BaseColor) ? material.GetColor(BaseColor) : material.color;
    }

    private void PintarBase(Color c)
    {
        if (material.HasProperty(BaseColor))
            material.SetColor(BaseColor, c);
        else
            material.color = c;
    }
}
