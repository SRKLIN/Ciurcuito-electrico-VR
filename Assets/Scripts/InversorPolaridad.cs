using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Boton que da la vuelta a la pila del circuito. Con un diodo o un LED puesto,
/// invertir la polaridad corta la corriente: es la forma de ver la polaridad sin
/// tener que encastrar la pieza del reves, que con las manos es una pelea.
///
/// Solo dispara la accion y refresca su etiqueta; el aspecto va aparte.
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
public class InversorPolaridad : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;

    [Tooltip("Etiqueta que muestra como esta puesta la pila. Opcional.")]
    [SerializeField] private TMP_Text etiqueta;

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
        RefrescarEtiqueta();
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

        circuito.InvertirPolaridad();
        SonidosCircuito.Reproducir(SonidoLab.CambioPolaridad, this);
        RefrescarEtiqueta();
    }

    private void RefrescarEtiqueta()
    {
        if (etiqueta == null || circuito == null)
            return;

        etiqueta.text = circuito.PolaridadInvertida ? "- +" : "+ -";
    }
}
