using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(BoxCollider2D))]
public class ExitTrigger : MonoBehaviour
{
    [Header("UI da Vitória")]
    public CanvasGroup fadeCanvas;
    public TextMeshProUGUI winTextUI;
    public string winMessage = "Happy Ending! - Thanks for playing.";

    [Header("Configurações de Tempo")]
    public float fadeSpeed = 2.0f;
    public float timeToReadText = 4.0f;

    private bool hasTriggered = false;

    private void Start()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }

        if (winTextUI != null) winTextUI.text = "";
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

        // 1. Congela o tempo e CORTA TODO O ÁUDIO do jogo!
        Time.timeScale = 0f;
        AudioListener.pause = true; // Silêncio total!

        if (winTextUI != null) winTextUI.text = winMessage;

        // 2. FADE IN (Fica Preto)
        float t = 0f;
        while (t < fadeSpeed)
        {
            t += Time.unscaledDeltaTime;
            if (fadeCanvas != null) fadeCanvas.alpha = Mathf.Clamp01(t / fadeSpeed);
            yield return null;
        }

        // 3. Lê o texto
        yield return new WaitForSecondsRealtime(timeToReadText);

        // 4. Repõe o som (para o Menu não ficar mudo) e Carrega Menu
        AudioListener.pause = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}