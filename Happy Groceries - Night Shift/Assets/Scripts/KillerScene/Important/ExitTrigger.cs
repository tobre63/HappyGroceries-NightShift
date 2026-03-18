using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Para manipular o TextMeshPro

[RequireComponent(typeof(BoxCollider2D))]
public class ExitTrigger : MonoBehaviour
{
    [Header("UI da Vitória")]
    [Tooltip("Painel preto que tapa o ecrã")]
    public CanvasGroup fadeCanvas;

    [Tooltip("Texto para mostrar a mensagem")]
    public TextMeshProUGUI winTextUI;

    [Tooltip("O que queres que diga quando ganhas")]
    public string winMessage = "Happy Ending! - Thanks for playing.";

    [Header("Configurações de Tempo")]
    public float fadeSpeed = 2.0f; // Quão rápido a tela fica preta
    public float timeToReadText = 4.0f; // Segundos que a mensagem fica no ecrã

    private bool hasTriggered = false;

    private void Start()
    {
        // Garante que o painel e o texto estão escondidos no arranque
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }

        if (winTextUI != null)
        {
            winTextUI.text = "";
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            StartCoroutine(HappyEndingSequence());
        }
    }

    private IEnumerator HappyEndingSequence()
    {
        hasTriggered = true;

        // Opcional: Pausa o jogo para o assassino não continuar a andar enquanto o ecrã faz Fade
        Time.timeScale = 0f;

        // Escreve o teu texto no UI
        if (winTextUI != null)
        {
            winTextUI.text = winMessage;
        }

        // FADE IN (Fica Preto e mostra o texto)
        float t = 0f;
        while (t < fadeSpeed)
        {
            t += Time.unscaledDeltaTime;
            if (fadeCanvas != null) fadeCanvas.alpha = Mathf.Clamp01(t / fadeSpeed);
            yield return null;
        }

        // Deixa o jogador ler a mensagem de parabéns
        yield return new WaitForSecondsRealtime(timeToReadText);

        // Volta a pôr o tempo ao normal e Carrega a CENA DO MENU (Build Index 0)
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}