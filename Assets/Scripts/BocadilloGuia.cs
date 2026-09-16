using TMPro;
using UnityEngine;

/// <summary>
/// El cartel de texto que flota sobre el guia. Solo sabe mostrar y ocultar una
/// frase, y darse vuelta hacia el jugador: no sabe nada del guion ni de la
/// visita. El <see cref="GuiaVirtual"/> le pasa el texto y se olvida.
///
/// Sirve de subtitulo tambien cuando haya voz grabada, asi que conviene dejarlo
/// puesto aunque los audios esten en su sitio.
/// </summary>
public class BocadilloGuia : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshPro texto;

    [Tooltip("Lo que se enciende y se apaga con el cartel: el fondo, el marco.")]
    [SerializeField] private GameObject panel;

    [Tooltip("A quien mira. Vacio = busca la camara principal al arrancar.")]
    [SerializeField] private Transform jugador;

    [Header("Comportamiento")]
    [Tooltip("Desmarcado, el cartel queda fijo mirando siempre para el mismo lado.")]
    [SerializeField] private bool mirarAlJugador = true;

    private void Awake()
    {
        if (jugador == null && Camera.main != null)
            jugador = Camera.main.transform;

        Ocultar();
    }

    /// <summary>Pone una frase en el cartel y lo enciende.</summary>
    public void Mostrar(string frase)
    {
        if (string.IsNullOrEmpty(frase))
        {
            Ocultar();
            return;
        }

        if (texto != null)
            texto.text = frase;

        Encender(true);
    }

    /// <summary>Apaga el cartel.</summary>
    public void Ocultar()
    {
        Encender(false);
    }

    private void Encender(bool si)
    {
        if (texto != null)
            texto.gameObject.SetActive(si);

        if (panel != null)
            panel.SetActive(si);
    }

    private void LateUpdate()
    {
        if (!mirarAlJugador || jugador == null)
            return;

        // El texto de TMP se lee desde su -Z, asi que el cartel tiene que darle
        // la ESPALDA al jugador para quedar de frente. De ahi el orden restado.
        Vector3 fuera = transform.position - jugador.position;
        fuera.y = 0f;

        if (fuera.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(fuera);
    }
}
