using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Objeto que abre o cierra el circuito al tocarlo, ya sea con el dedo (poke)
/// o seleccionandolo con el rayo.
/// Solo se ocupa de disparar la accion: la animacion y el aspecto van aparte.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class InterruptorCircuito : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;

    [Header("Anti-rebote")]
    [Tooltip("Segundos que ignora toques seguidos. Evita que un dedo apoyado lo haga parpadear.")]
    [SerializeField] private float tiempoMinimoEntreToques = 0.3f;

    private XRSimpleInteractable interactable;
    private float ultimoToque = -999f;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
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
        if (circuito == null)
            return;

        if (Time.time - ultimoToque < tiempoMinimoEntreToques)
            return;

        ultimoToque = Time.time;
        circuito.Alternar();
    }
}
