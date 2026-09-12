using UnityEngine;

/// <summary>
/// Valor ohmico de una resistencia fisica. Al encastrarla en un zocalo,
/// este valor pasa a ser la resistencia del circuito.
/// </summary>
public class ValorResistencia : MonoBehaviour
{
    [Tooltip("Resistencia de esta pieza, en ohmios.")]
    [SerializeField] private float ohmios = 18f;

    public float Ohmios => Mathf.Max(0.0001f, ohmios);
}
