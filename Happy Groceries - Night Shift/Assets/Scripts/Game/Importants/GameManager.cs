using System.Collections; // Necessário para as Corrotinas (Fades)
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configurações de UI")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject settingsButton;

    [Header("Sons da UI")]
    public AudioSource uiAudioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Música de Fundo (BGM)")]
    public AudioSource bgmAudioSource;
    public float bgmFadeDuration = 0.1f; // O tempo super rápido do fade
    private float originalBgmVolume = 1f;
    private Coroutine bgmFadeCoroutine;

    public bool isPaused { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        // Guarda o volume original da música mal o jogo comece
        if (bgmAudioSource != null)
        {
            originalBgmVolume = bgmAudioSource.volume;
        }
    }

    void Start()
    {
        Resume();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleEscPress();
        }
    }

    private void HandleEscPress()
    {
        if (settingsMenu != null && settingsMenu.activeSelf)
        {
            CloseSettings();
        }
        else if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenu != null) pauseMenu.SetActive(true);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Inicia o Fade Out para a música
        if (bgmAudioSource != null)
        {
            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeBGM(0f, true));
        }
    }

    public void PauseWithoutMenu()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Inicia o Fade Out para a música
        if (bgmAudioSource != null)
        {
            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeBGM(0f, true));
        }
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Inicia o Fade In para a música voltar ao volume original
        if (bgmAudioSource != null)
        {
            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeBGM(originalBgmVolume, false));
        }
    }

    public void OpenSettings()
    {
        if (settingsButton != null)
        {
            RectTransform rt = settingsButton.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 160f);
            }
        }

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }

    public void Home()
    {
        Time.timeScale = 1f;

        // Garante que o volume volta ao normal se sairmos para o menu principal
        if (bgmAudioSource != null) bgmAudioSource.volume = originalBgmVolume;

        SceneManager.LoadScene("Menu");
    }

    public void Back()
    {
        isPaused = true;

        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }

    public void PlayHoverSound()
    {
        if (uiAudioSource != null && hoverSound != null)
        {
            uiAudioSource.ignoreListenerPause = true;
            uiAudioSource.PlayOneShot(hoverSound);
        }
    }

    public void PlayClickSound()
    {
        if (uiAudioSource != null && clickSound != null)
        {
            uiAudioSource.ignoreListenerPause = true;
            uiAudioSource.PlayOneShot(clickSound);
        }
    }

    // --- CORROTINA MAGICA DO FADE (Ignora a Pausa do Jogo) ---
    private IEnumerator FadeBGM(float targetVolume, bool pauseAtEnd)
    {
        float startVolume = bgmAudioSource.volume;
        float timeElapsed = 0f;

        // Se for para ligar a música, fazemos o UnPause imediatamente antes de subir o volume
        if (!pauseAtEnd && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.UnPause();
        }

        while (timeElapsed < bgmFadeDuration)
        {
            // unscaledDeltaTime é obrigatório aqui porque o Time.timeScale = 0 congelou o tempo normal!
            timeElapsed += Time.unscaledDeltaTime;
            bgmAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / bgmFadeDuration);
            yield return null;
        }

        bgmAudioSource.volume = targetVolume;

        // Se for para desligar a música, só fazemos o Pause DEPOIS do volume chegar a zero
        if (pauseAtEnd)
        {
            bgmAudioSource.Pause();
        }
    }
}