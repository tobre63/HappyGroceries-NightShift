using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Screens & Fade")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private CanvasGroup fadePanel;

    [Header("Elements to Hide on Transition")]
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private GameObject backgroundPanel;
    
    // NOVO: Referência para o botão de Skip para podermos mostrá-lo/escondê-lo
    [SerializeField] private GameObject skipButton; 

    [Header("Story Text Settings")]
    [SerializeField] private TextMeshProUGUI loadingText;
    [TextArea(8, 15)]
    [SerializeField] private string fullText;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float waitAfterText = 2.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private CanvasGroup loadingTextGroup;
    private bool isTransitioning = false;
    
    // NOVO: Flag para saber se o jogador clicou no skip
    private bool isSkippingText = false; 

    private void Awake()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadingText != null) loadingText.text = "";
        
        // NOVO: Garante que o botão de skip começa escondido
        if (skipButton != null) skipButton.SetActive(false);

        if (loadingText != null)
        {
            loadingTextGroup = loadingText.GetComponent<CanvasGroup>();
            if (loadingTextGroup == null)
                loadingTextGroup = loadingText.gameObject.AddComponent<CanvasGroup>();

            loadingTextGroup.alpha = 1f;
            loadingTextGroup.blocksRaycasts = false;
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 1;
            fadePanel.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        StartCoroutine(DoFade(1, 0, 1.5f));
    }

    public void StartGame(string sceneName)
    {
        if (isTransitioning || string.IsNullOrEmpty(sceneName)) return;

        isTransitioning = true;
        PlaySound(clickSound);

        StartCoroutine(SequenceWithDiary(sceneName));
    }

    // NOVO: Método que o teu botão "Skip" vai chamar
    public void SkipTextAnimation()
    {
        // Só permite fazer skip se estivermos na transição e ainda não tivermos feito skip
        if (isTransitioning && !isSkippingText)
        {
            isSkippingText = true;
            PlaySound(clickSound);
        }
    }

    private IEnumerator SequenceWithDiary(string sceneName)
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeMusic(musicSource, 0f, 1.5f));
        }

        yield return StartCoroutine(DoFade(0, 1, 1.5f));

        if (buttonsPanel != null) buttonsPanel.SetActive(false);
        if (backgroundPanel != null) backgroundPanel.SetActive(false);

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "";
            
            // NOVO: Reset da flag e mostra o botão de skip
            isSkippingText = false;
            if (skipButton != null) skipButton.SetActive(true);

            foreach (char letter in fullText.ToCharArray())
            {
                // NOVO: Verifica se o jogador clicou no Skip
                if (isSkippingText)
                {
                    loadingText.text = fullText; // Mostra o texto todo
                    break; // Sai do ciclo 'foreach' imediatamente
                }

                loadingText.text += letter;
                if (typeSound != null && audioSource != null) audioSource.PlayOneShot(typeSound);
                yield return new WaitForSeconds(typingSpeed);
            }
            
            // NOVO: Esconde o botão de skip quando o texto terminar (seja natural ou por skip)
            if (skipButton != null) skipButton.SetActive(false);
        }

        // 5. Tempo de leitura (espera os 2 segundos, mesmo se tiver feito skip)
        yield return new WaitForSeconds(waitAfterText);

        if (loadingTextGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(loadingTextGroup, 1f, 0f, 1.0f));
            loadingText.gameObject.SetActive(false);
        }

        SceneManager.LoadScene(sceneName);
    }

    // --- Helper Coroutines (Utilitários) ---

    private IEnumerator DoFade(float start, float end, float duration)
    {
        if (fadePanel == null) yield break;
        fadePanel.gameObject.SetActive(true);

        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }
        fadePanel.alpha = end;

        if (end <= 0) fadePanel.gameObject.SetActive(false);
    }

    private IEnumerator FadeMusic(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null) yield break;
        float timer = 0f;
        cg.alpha = start;
        cg.gameObject.SetActive(true);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }

        cg.alpha = end;
        if (end <= 0f) cg.gameObject.SetActive(false);
    }

    // --- UI & Audio Methods ---

    public void OpenSettings()
    {
        if (isTransitioning) return;
        PlaySound(clickSound);
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (isTransitioning) return;
        PlaySound(clickSound);
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        if (isTransitioning) return;
        PlaySound(clickSound);
        Application.Quit();
    }

    public void OnButtonHover()
    {
        if (isTransitioning) return;
        PlaySound(hoverSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}