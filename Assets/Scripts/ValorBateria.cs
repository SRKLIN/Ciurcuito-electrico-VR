using UnityEngine;

/// <summary>
/// Tension nominal de una bateria fisica. Al encastrarla en un zocalo,
/// este valor pasa a ser el voltaje del circuito.
/// </summary>
public class ValorBateria : MonoBehaviour
{
    [Tooltip("Tension de esta pila, en voltios.")]
    [SerializeField] private float voltios = 9f;

    public float Voltios => Mathf.Max(0f, voltios);
}
