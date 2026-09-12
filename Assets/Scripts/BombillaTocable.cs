using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Al tocar una bombilla quemada la reemplaza por una nueva.
/// Deja el circuito abierto para que no se vuelva a quemar al instante
/// si el voltaje sigue alto.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class BombillaTocable : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Bombilla bombilla;
    [SerializeField] private CircuitoSimple circuito;

    [Header("Anti-rebote")]
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
        if (bombilla == null || !bombilla.Quemada)
            return;

        if (Time.time - ultimoToque < tiempoMinimoEntreToques)
            return;

        ultimoToque = Time.time;

        bombilla.Reparar();

        // Se reemplaza con el circuito abierto: si no, con el voltaje todavia
        // alto se quemaria de nuevo en el mismo frame.
        if (circuito != null)
            circuito.Establecer(false);
    }
}
