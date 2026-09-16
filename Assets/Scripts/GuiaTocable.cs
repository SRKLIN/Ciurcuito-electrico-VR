using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Hablarle al guia tocandolo, con el dedo o con el rayo. Mismo patron que
/// <see cref="InterruptorCircuito"/>: solo dispara la accion, con anti-rebote
/// para que un dedo apoyado no la repita en rafaga.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class GuiaTocable : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GuiaVirtual guia;

    [Header("Anti-rebote")]
    [Tooltip("Segundos que ignora toques seguidos.")]
    [SerializeField] private float tiempoMinimoEntreToques = 0.3f;

    private XRSimpleInteractable interactable;
    private float ultimoToque = -999f;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();

        if (guia == null)
            guia = GetComponentInParent<GuiaVirtual>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnTocar);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnTocar);
    }

    private void OnTocar(SelectEnterEventArgs args)
    {
        if (guia == null)
            return;

        if (Time.time - ultimoToque < tiempoMinimoEntreToques)
            return;

        ultimoToque = Time.time;

        guia.Hablarle();
    }
}
