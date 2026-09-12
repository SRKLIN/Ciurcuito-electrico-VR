using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Mesa de armado. El circuito se cierra solo cuando cada zocalo tiene la pieza
/// que le toca, y la bombilla encastrada es la que se enciende.
///
/// El zocalo de resistencia es opcional: si esta asignado, la mesa exige ademas
/// una resistencia y su valor pasa a ser el del circuito, con lo que cambiar de
/// pieza cambia el brillo. Si se deja vacio, la mesa funciona con tres zocalos.
/// </summary>
public class CircuitoArmable : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;

    [Header("Zocalos")]
    [SerializeField] private XRSocketInteractor zocaloBateria;
    [SerializeField] private XRSocketInteractor zocaloInterruptor;
    [SerializeField] private XRSocketInteractor zocaloBombilla;

    [Tooltip("Opcional. Si se asigna, la mesa exige una resistencia y toma su valor.")]
    [SerializeField] private XRSocketInteractor zocaloResistencia;

    private void OnEnable()
    {
        Suscribir(zocaloBateria, true);
        Suscribir(zocaloInterruptor, true);
        Suscribir(zocaloBombilla, true);
        Suscribir(zocaloResistencia, true);
        Revisar();
    }

    private void OnDisable()
    {
        Suscribir(zocaloBateria, false);
        Suscribir(zocaloInterruptor, false);
        Suscribir(zocaloBombilla, false);
        Suscribir(zocaloResistencia, false);
    }

    private void Suscribir(XRSocketInteractor zocalo, bool activar)
    {
        if (zocalo == null)
            return;

        if (activar)
        {
            zocalo.selectEntered.AddListener(OnPiezaPuesta);
            zocalo.selectExited.AddListener(OnPiezaSacada);
        }
        else
        {
            zocalo.selectEntered.RemoveListener(OnPiezaPuesta);
            zocalo.selectExited.RemoveListener(OnPiezaSacada);
        }
    }

    private void OnPiezaPuesta(SelectEnterEventArgs args)
    {
        Revisar();
    }

    private void OnPiezaSacada(SelectExitEventArgs args)
    {
        Revisar();
    }

    private void Revisar()
    {
        if (circuito == null)
            return;

        // Cada zocalo tiene que tener SU pieza. Que este ocupado no alcanza:
        // sin comprobar el tipo, un interruptor en el hueco de la bateria
        // cerraria el circuito igual.
        bool bateriaLista = TieneTipo(zocaloBateria, TipoPieza.Bateria);
        bool interruptorListo = TieneTipo(zocaloInterruptor, TipoPieza.Interruptor);

        // Si la pila declara su tension, manda ella. Una bateria sin marcar
        // deja el voltaje como estaba, para no romper las piezas antiguas.
        if (bateriaLista)
        {
            var pila = BuscarEnZocalo<ValorBateria>(zocaloBateria);

            if (pila != null)
                circuito.Voltaje = pila.Voltios;
        }

        Bombilla foco = TieneTipo(zocaloBombilla, TipoPieza.Bombilla)
            ? BuscarEnZocalo<Bombilla>(zocaloBombilla)
            : null;

        bool resistenciaLista = true;

        if (zocaloResistencia != null)
        {
            var valor = TieneTipo(zocaloResistencia, TipoPieza.Resistencia)
                ? BuscarEnZocalo<ValorResistencia>(zocaloResistencia)
                : null;

            resistenciaLista = valor != null;

            if (valor != null)
                circuito.Resistencia = valor.Ohmios;
        }

        circuito.Foco = foco;
        circuito.Establecer(bateriaLista && interruptorListo && resistenciaLista && foco != null);
    }

    private static PiezaElectrica PiezaEn(XRSocketInteractor zocalo)
    {
        if (zocalo == null || !zocalo.hasSelection)
            return null;

        var componente = zocalo.firstInteractableSelected as Component;
        return componente != null ? componente.GetComponentInChildren<PiezaElectrica>() : null;
    }

    private static bool TieneTipo(XRSocketInteractor zocalo, TipoPieza esperado)
    {
        var pieza = PiezaEn(zocalo);
        return pieza != null && pieza.Tipo == esperado;
    }

    private static T BuscarEnZocalo<T>(XRSocketInteractor zocalo) where T : Component
    {
        if (zocalo == null || !zocalo.hasSelection)
            return null;

        var componente = zocalo.firstInteractableSelected as Component;
        return componente != null ? componente.GetComponentInChildren<T>() : null;
    }
}
