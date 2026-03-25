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

    [Header("Áudio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

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

        // 1. Congela o tempo e pausa apenas os AudioSources específicos
        Time.timeScale = 0f;
        
        if (musicSource != null) musicSource.Pause();
        if (sfxSource != null) sfxSource.Pause();

        if (winTextUI != null) winTextUI.text = winMessage;

        // 2. FADE IN (Fica Preto)
        float t = 0f;
        while (t < fadeSpeed)
        {
            t += Time.unscaledDeltaTime; // unscaledDeltaTime continua rodando mesmo com Time.timeScale = 0
            if (fadeCanvas != null) fadeCanvas.alpha = Mathf.Clamp01(t / fadeSpeed);
            yield return null;
        }

        // 3. Lê o texto
        yield return new WaitForSecondsRealtime(timeToReadText);

        // 4. Retoma os áudios e carrega o Menu
        if (musicSource != null) musicSource.UnPause();
        if (sfxSource != null) sfxSource.UnPause();
        
        Time.timeScale = 1f;

        // Torna o rato visível e desbloqueado para o Menu Principal
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(0);
    }
}