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

    [Header("Buttons References")]
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject skipButton;

    [Header("Story Text Settings")]
    [SerializeField] private TextMeshProUGUI loadingText;
    [TextArea(8, 15)]
    [SerializeField] private string fullText;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float punctuationPause = 0.15f;
    [SerializeField] private float waitAfterText = 2.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private CanvasGroup loadingTextGroup;
    private bool isTransitioning = false;
    private bool isSkippingText = false;

    // Chaves para o PlayerPrefs
    private const string SAVE_SCENE_KEY = "SavedScene";

    private void Awake()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadingText != null) loadingText.text = "";

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

        // Verifica o save logo ao iniciar o Menu
        CheckSaveGame();
    }

    private void CheckSaveGame()
    {
        if (continueButton != null)
        {
            bool hasSaveGame = PlayerPrefs.HasKey(SAVE_SCENE_KEY);
            continueButton.interactable = hasSaveGame;

            // Opcional: Se quiser que o botão mude de cor ou transparência visualmente
            // var canvasGroup = continueButton.GetComponent<CanvasGroup>();
            // if (canvasGroup) canvasGroup.alpha = hasSaveGame ? 1f : 0.5f;
        }
    }


    public void StartGame(string sceneName)
    {
        if (isTransitioning || string.IsNullOrEmpty(sceneName)) return;

        // --- RESET TOTAL PARA NOVO JOGO ---
        // 1. Define que existe um save (para o botão Continue aparecer depois)
        PlayerPrefs.SetString("SavedScene", sceneName);

        // 2. APAGA dados antigos de posição e tempo
        PlayerPrefs.DeleteKey("PlayerX");
        PlayerPrefs.DeleteKey("PlayerY");
        PlayerPrefs.DeleteKey("PlayerZ");
        PlayerPrefs.DeleteKey("PlayerRotY");
        PlayerPrefs.DeleteKey("SavedTime"); // <--- Importante apagar o tempo antigo!

        PlayerPrefs.Save();
        // ----------------------------------

        isTransitioning = true;
        PlaySound(clickSound); // Toca som se houver

        StartCoroutine(SequenceWithDiary(sceneName));
    }

    public void ContinueGame()
    {
        if (isTransitioning) return;

        if (!PlayerPrefs.HasKey(SAVE_SCENE_KEY)) return;

        string savedSceneName = PlayerPrefs.GetString(SAVE_SCENE_KEY);

        isTransitioning = true;
        PlaySound(clickSound);

        StartCoroutine(SequenceContinueGame(savedSceneName));
    }

    public void SkipTextAnimation()
    {
        if (isTransitioning && !isSkippingText)
        {
            isSkippingText = true;
            PlaySound(clickSound);
        }
    }

    private IEnumerator SequenceContinueGame(string sceneName)
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeMusic(musicSource, 0f, 1.0f));
        }

        if (buttonsPanel != null) buttonsPanel.SetActive(false);

        yield return StartCoroutine(DoFade(0, 1, 1.0f));

        SceneManager.LoadScene(sceneName);
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

            isSkippingText = false;
            if (skipButton != null) skipButton.SetActive(true);

            foreach (char letter in fullText.ToCharArray())
            {
                if (isSkippingText)
                {
                    loadingText.text = fullText;
                    break;
                }

                loadingText.text += letter;

                if (char.IsLetterOrDigit(letter))
                {
                    if (typeSound != null && audioSource != null)
                    {
                        audioSource.pitch = Random.Range(0.9f, 1.1f);
                        audioSource.PlayOneShot(typeSound);
                    }
                }

                yield return new WaitForSeconds(typingSpeed);

                if (letter == '.' || letter == '!' || letter == '?')
                {
                    yield return new WaitForSeconds(punctuationPause);
                }
            }

            if (skipButton != null) skipButton.SetActive(false);
        }

        yield return new WaitForSeconds(waitAfterText);

        if (loadingTextGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(loadingTextGroup, 1f, 0f, 1.0f));
            loadingText.gameObject.SetActive(false);
        }

        SceneManager.LoadScene(sceneName);
    }

    // --- Helper Coroutines ---

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
        if (clip != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(clip);
        }
    }
}