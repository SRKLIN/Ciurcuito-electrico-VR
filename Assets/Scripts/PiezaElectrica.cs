using UnityEngine;

/// <summary>Familia de un componente electrico.</summary>
public enum TipoPieza
{
    Bateria,
    Interruptor,
    Bombilla,
    Resistencia,
    Cable
}

/// <summary>
/// Identifica que clase de componente es una pieza. Los zocalos la usan para
/// aceptar solo lo que corresponde, en vez de tragarse cualquier objeto.
/// </summary>
public class PiezaElectrica : MonoBehaviour
{
    [SerializeField] private TipoPieza tipo = TipoPieza.Bateria;

    public TipoPieza Tipo => tipo;
}
