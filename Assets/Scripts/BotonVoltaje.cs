using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Boton de la fuente de voltaje. Al tocarlo sube o baja el voltaje segun
/// como este configurado. Solo dispara la accion: el aspecto va aparte.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class BotonVoltaje : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ControlVoltaje control;

    [Header("Comportamiento")]
    [Tooltip("Marcado sube el voltaje, desmarcado lo baja.")]
    [SerializeField] private bool sube = true;

    [Tooltip("Segundos que ignora toques seguidos, para que un dedo apoyado no dispare en rafaga.")]
    [SerializeField] private float tiempoMinimoEntreToques = 0.25f;

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
        if (control == null)
            return;

        if (Time.time - ultimoToque < tiempoMinimoEntreToques)
            return;

        ultimoToque = Time.time;

        if (sube)
            control.Subir();
        else
            control.Bajar();

        SonidosCircuito.Reproducir(SonidoLab.BotonVoltaje, this);
    }
}
