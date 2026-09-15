using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Mesa de armado. El circuito se cierra solo cuando cada zocalo tiene la pieza
/// que le toca, y la bombilla encastrada es la que se enciende.
///
/// Los zocalos de bateria, interruptor y bombilla son la base. El de resistencia
/// es opcional, pero si se asigna hace falta llenarlo. Los tres ultimos
/// (condensador, diodo y motor) son de agregado: el circuito cierra igual sin
/// ellos y cada pieza que se encastra suma su efecto, asi que una sola mesa
/// sirve para probarlos sueltos o combinados.
/// </summary>
public class CircuitoArmable : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CircuitoSimple circuito;

    [Header("Zocalos")]
    [SerializeField] private XRSocketInteractor zocaloBateria;

    [Tooltip("Opcional. Si se deja vacio, la mesa no pide interruptor.")]
    [SerializeField] private XRSocketInteractor zocaloInterruptor;

    [SerializeField] private XRSocketInteractor zocaloBombilla;

    [Tooltip("Opcional. Si se asigna, la mesa exige una resistencia y toma su valor.")]
    [SerializeField] private XRSocketInteractor zocaloResistencia;

    [Tooltip("Opcional. La pieza que se encastre aca sostiene el brillo al abrir el circuito.")]
    [SerializeField] private XRSocketInteractor zocaloCondensador;

    [Tooltip("Opcional. Con un diodo o LED encastrado hay que respetar la polaridad.")]
    [SerializeField] private XRSocketInteractor zocaloDiodo;

    [Tooltip("Opcional. El motor gira con la corriente. Sirve como unica carga o junto a la bombilla.")]
    [SerializeField] private XRSocketInteractor zocaloMotor;

    [Header("Montaje")]
    [Tooltip("Sin filtros: cualquier pieza entra en cualquier hueco y el orden da igual, " +
             "porque en un circuito en serie la corriente es la misma en todo el lazo. " +
             "Se exige llenar todos los huecos con una pieza de cada familia.")]
    [SerializeField] private bool montajeLibre;

    private void OnEnable()
    {
        Suscribir(true);
        Revisar();
    }

    private void OnDisable()
    {
        Suscribir(false);
    }

    private void Suscribir(bool activar)
    {
        Suscribir(zocaloBateria, activar);
        Suscribir(zocaloInterruptor, activar);
        Suscribir(zocaloBombilla, activar);
        Suscribir(zocaloResistencia, activar);
        Suscribir(zocaloCondensador, activar);
        Suscribir(zocaloDiodo, activar);
        Suscribir(zocaloMotor, activar);
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

    /// <summary>
    /// Avisa cada vez que cambia lo que hay en los zocalos. La pizarra se
    /// engancha aca en vez de mirar los zocalos en cada frame, que obligaria a
    /// un GetComponentInChildren por zocalo sesenta veces por segundo.
    /// </summary>
    public event System.Action Cambio;

    /// <summary>true si esta mesa tiene hueco para esa familia de pieza.</summary>
    public bool TieneZocalo(TipoPieza tipo)
    {
        return ZocaloDe(tipo) != null;
    }

    // Cuantas piezas de cada familia hay sobre la mesa. Solo se usa en montaje
    // libre, donde una pieza puede estar en cualquier hueco.
    private readonly int[] conteo = new int[16];

    /// <summary>
    /// true si esa familia esta presente. En montaje libre da igual en que hueco:
    /// lo que importa es que la pieza este en el lazo.
    /// </summary>
    public bool TienePieza(TipoPieza tipo)
    {
        if (montajeLibre)
            return conteo[(int)tipo] > 0;

        return TieneTipo(ZocaloDe(tipo), tipo);
    }

    /// <summary>
    /// true si en ese zocalo hay algo, pero no es lo que corresponde.
    /// Solo puede pasar en las mesas de montaje libre, donde los zocalos no
    /// llevan FiltroPieza y aceptan cualquier cosa.
    /// </summary>
    public bool PiezaMalPuesta(TipoPieza tipo)
    {
        // En montaje libre no existe el "hueco equivocado": el error es repetir
        // una familia, porque entonces falta otra para cerrar el lazo.
        if (montajeLibre)
            return conteo[(int)tipo] > 1;

        var zocalo = ZocaloDe(tipo);

        if (zocalo == null || !zocalo.hasSelection)
            return false;

        var pieza = PiezaEn(zocalo);
        return pieza == null || pieza.Tipo != tipo;
    }

    /// <summary>
    /// true si alguna pieza esta en el hueco equivocado. Se calcula una sola vez
    /// en Revisar() y se guarda: la pizarra lo consulta en cada repintado y
    /// recorrerlo cada frame significaria siete GetComponentInChildren por frame.
    /// </summary>
    public bool MalMontado { get; private set; }

    private XRSocketInteractor ZocaloDe(TipoPieza tipo)
    {
        switch (tipo)
        {
            case TipoPieza.Bateria: return zocaloBateria;
            case TipoPieza.Interruptor: return zocaloInterruptor;
            case TipoPieza.Bombilla: return zocaloBombilla;
            case TipoPieza.Resistencia: return zocaloResistencia;
            case TipoPieza.Condensador: return zocaloCondensador;
            case TipoPieza.Diodo: return zocaloDiodo;
            case TipoPieza.Motor: return zocaloMotor;
            default: return null;
        }
    }

    private void OnPiezaPuesta(SelectEnterEventArgs args)
    {
        SonidosCircuito.Reproducir(SonidoLab.PiezaEncastrada, args.interactorObject.transform.position);
        Revisar();
    }

    private void OnPiezaSacada(SelectExitEventArgs args)
    {
        SonidosCircuito.Reproducir(SonidoLab.PiezaSacada, args.interactorObject.transform.position);
        Revisar();
    }

    private void Revisar()
    {
        if (circuito == null)
            return;

        if (montajeLibre)
        {
            RevisarLibre();
            Avisar();
            return;
        }

        // Cada zocalo tiene que tener SU pieza. Que este ocupado no alcanza:
        // sin comprobar el tipo, un interruptor en el hueco de la bateria
        // cerraria el circuito igual.
        bool bateriaLista = TieneTipo(zocaloBateria, TipoPieza.Bateria);

        // El interruptor sigue la misma regla que la resistencia: si la mesa no
        // tiene ese zocalo, no se exige. En la mesa de retos se quito a proposito,
        // porque encastrado no conmuta nada: es solo una pieza mas que poner.
        bool interruptorListo = zocaloInterruptor == null
            || TieneTipo(zocaloInterruptor, TipoPieza.Interruptor);

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

        Condensador deposito = null;

        if (zocaloCondensador != null)
        {
            deposito = TieneTipo(zocaloCondensador, TipoPieza.Condensador)
                ? BuscarEnZocalo<Condensador>(zocaloCondensador)
                : null;
        }

        Diodo valvula = null;

        if (zocaloDiodo != null)
        {
            valvula = TieneTipo(zocaloDiodo, TipoPieza.Diodo)
                ? BuscarEnZocalo<Diodo>(zocaloDiodo)
                : null;
        }

        Motor maquina = null;

        if (zocaloMotor != null)
        {
            maquina = TieneTipo(zocaloMotor, TipoPieza.Motor)
                ? BuscarEnZocalo<Motor>(zocaloMotor)
                : null;

            // El motor trae su propia resistencia interna. Solo manda cuando la
            // mesa no tiene zocalo de resistencia; si lo tiene, la pieza suelta
            // sigue siendo la que fija el valor.
            if (maquina != null && zocaloResistencia == null)
            {
                var interna = BuscarEnZocalo<ValorResistencia>(zocaloMotor);

                if (interna != null)
                    circuito.Resistencia = interna.Ohmios;
            }
        }

        circuito.Foco = foco;
        circuito.Deposito = deposito;
        circuito.Valvula = valvula;
        circuito.Maquina = maquina;

        // Condensador, diodo y motor NO son obligatorios aunque su zocalo este
        // asignado: el circuito cierra igual sin ellos y cada pieza que se
        // encastra agrega su efecto. Asi una sola mesa sirve para probarlos por
        // separado o combinados, sin obligar a llenar los seis huecos.
        //
        // Hace falta algo que consuma: la bombilla, el motor, o los dos.
        bool hayCarga = foco != null || maquina != null;

        circuito.Establecer(bateriaLista && interruptorListo && resistenciaLista && hayCarga);

        MalMontado = PiezaMalPuesta(TipoPieza.Bateria)
            || PiezaMalPuesta(TipoPieza.Interruptor)
            || PiezaMalPuesta(TipoPieza.Bombilla)
            || PiezaMalPuesta(TipoPieza.Resistencia)
            || PiezaMalPuesta(TipoPieza.Condensador)
            || PiezaMalPuesta(TipoPieza.Diodo)
            || PiezaMalPuesta(TipoPieza.Motor);

        Avisar();
    }

    /// <summary>
    /// Montaje libre: no importa en que hueco esta cada pieza. En un circuito en
    /// serie la corriente es la misma en todo el lazo, asi que pila, resistencia
    /// y bombilla dan el mismo circuito en cualquier orden. Lo unico que se pide
    /// es llenar todos los huecos sin repetir familia.
    /// </summary>
    private void RevisarLibre()
    {
        for (int i = 0; i < conteo.Length; i++)
            conteo[i] = 0;

        ValorBateria pila = null;
        ValorResistencia ohmios = null;
        Bombilla foco = null;
        Motor maquina = null;
        Diodo valvula = null;
        Condensador deposito = null;
        int huecos = 0, ocupados = 0;

        foreach (var zocalo in Todos())
        {
            if (zocalo == null)
                continue;

            huecos++;
            var pieza = PiezaEn(zocalo);

            if (pieza == null)
                continue;

            ocupados++;
            conteo[(int)pieza.Tipo]++;

            if (pila == null) pila = BuscarEnZocalo<ValorBateria>(zocalo);
            if (ohmios == null) ohmios = BuscarEnZocalo<ValorResistencia>(zocalo);
            if (foco == null) foco = BuscarEnZocalo<Bombilla>(zocalo);
            if (maquina == null) maquina = BuscarEnZocalo<Motor>(zocalo);
            if (valvula == null) valvula = BuscarEnZocalo<Diodo>(zocalo);
            if (deposito == null) deposito = BuscarEnZocalo<Condensador>(zocalo);
        }

        if (pila != null)
            circuito.Voltaje = pila.Voltios;

        if (ohmios != null)
            circuito.Resistencia = ohmios.Ohmios;

        circuito.Foco = foco;
        circuito.Deposito = deposito;
        circuito.Valvula = valvula;
        circuito.Maquina = maquina;

        bool repetida = false;
        for (int i = 0; i < conteo.Length; i++)
            if (conteo[i] > 1) repetida = true;

        MalMontado = repetida;

        bool hayCarga = foco != null || maquina != null;
        circuito.Establecer(ocupados == huecos && !repetida && pila != null && hayCarga);
    }

    /// <summary>Los zocalos de la mesa, en un array fijo para no reservar memoria por frame.</summary>
    private XRSocketInteractor[] Todos()
    {
        if (todos == null)
            todos = new[] { zocaloBateria, zocaloInterruptor, zocaloBombilla,
                            zocaloResistencia, zocaloCondensador, zocaloDiodo, zocaloMotor };

        return todos;
    }

    private XRSocketInteractor[] todos;
    private bool malMontadoAntes;

    private void Avisar()
    {
        if (MalMontado && !malMontadoAntes)
            SonidosCircuito.Reproducir(SonidoLab.Cortocircuito, this);

        malMontadoAntes = MalMontado;

        if (Cambio != null)
            Cambio();
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
