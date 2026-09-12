using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Filtro para un zocalo: solo deja encastrar piezas del tipo indicado.
/// Se engancha en la lista Starting Select Filters del XRSocketInteractor,
/// asi la pieza equivocada ni siquiera llega a entrar.
/// </summary>
public class FiltroPieza : MonoBehaviour, IXRSelectFilter
{
    [Tooltip("Unica familia de componente que este zocalo acepta.")]
    [SerializeField] private TipoPieza tipoAceptado = TipoPieza.Bateria;

    public TipoPieza TipoAceptado => tipoAceptado;

    /// <summary>Con el filtro desactivado el zocalo vuelve a aceptar cualquier cosa.</summary>
    public bool canProcess => isActiveAndEnabled;

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        var componente = interactable as Component;
        if (componente == null)
            return false;

        // GetComponentInChildren para que valga tambien si la marca esta en un hijo
        // del modelo, no en la raiz del objeto agarrable.
        var pieza = componente.GetComponentInChildren<PiezaElectrica>();

        return pieza != null && pieza.Tipo == tipoAceptado;
    }
}
