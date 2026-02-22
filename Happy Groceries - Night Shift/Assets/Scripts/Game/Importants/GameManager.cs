using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Configs")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject settingsButton;

    [Header("UI Sounds")]
    public AudioSource uiAudioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Background Music")]
    public AudioSource bgmAudioSource;
    public float bgmFadeDuration = 0.1f;
    private float originalBgmVolume = 1f;
    private Coroutine bgmFadeCoroutine;

    [Header("Tasks / To-Do List")]
    public GameObject taskObject;
    public KeyCode toggleKey = KeyCode.Tab; // Tecla para ver as tarefas
    public bool holdToShow = true;          // Se true, tens de segurar a tecla. Se false, é só clicar.
    private bool isTasksToggled = false;

    public bool isPaused { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        if (bgmAudioSource != null)
        {
            originalBgmVolume = bgmAudioSource.volume;
        }

        // --- O TRUQUE MÁGICO DO ÁUDIO ---
        // Ligamos o SettingsMenu à força mal o jogo arranca.
        // Isto faz com que o script "SettingsManager" acorde e carregue os teus Saves de volume!
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(true);
        }
    }

    void Start()
    {
        // ...E agora que o volume já carregou, voltamos a esconder o menu para o jogador jogar normalmente.
        CloseSettings();
        Resume();
        ForceCloseTasks(); // Garante que as tasks começam fechadas
    }

    void Update()
    {
        // 1. Lógica do Menu de Pausa
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleEscPress();
        }

        // 2. Lógica das Tasks (Só funciona se o jogo NÃO estiver em pausa)
        if (!isPaused)
        {
            HandleTasksInput();
        }
    }

    // ==========================================
    //           GESTÃO DAS TASKS
    // ==========================================

    private void HandleTasksInput()
    {
        if (taskObject == null) return;

        if (holdToShow)
        {
            // Modo HOLD: Mostra apenas enquanto a tecla está pressionada
            bool isKeyPressed = Input.GetKey(toggleKey);

            if (taskObject.activeSelf != isKeyPressed)
            {
                taskObject.SetActive(isKeyPressed);
            }
        }
        else
        {
            // Modo TOGGLE: Clica uma vez para mostrar, clica de novo para esconder
            if (Input.GetKeyDown(toggleKey))
            {
                isTasksToggled = !isTasksToggled;
                taskObject.SetActive(isTasksToggled);
            }
        }
    }

    public void ShowTasks(bool show)
    {
        if (taskObject != null)
        {
            taskObject.SetActive(show);
            if (!holdToShow) isTasksToggled = show;
        }
    }

    public void ForceCloseTasks()
    {
        if (taskObject != null)
        {
            taskObject.SetActive(false);
            isTasksToggled = false;
        }
    }

    // ==========================================
    //           GESTÃO DE PAUSA E MENUS
    // ==========================================

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

        ForceCloseTasks(); // Esconde as tasks quando abres o menu

        if (pauseMenu != null) pauseMenu.SetActive(true);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

        ForceCloseTasks();

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
            if (rt != null) rt.sizeDelta = new Vector2(rt.sizeDelta.x, 160f);
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
        if (bgmAudioSource != null) bgmAudioSource.volume = originalBgmVolume;
        SceneManager.LoadScene("Menu");
    }

    public void Back()
    {
        isPaused = true;
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }

    // ==========================================
    //                 ÁUDIO
    // ==========================================

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

    private IEnumerator FadeBGM(float targetVolume, bool pauseAtEnd)
    {
        float startVolume = bgmAudioSource.volume;
        float timeElapsed = 0f;

        if (!pauseAtEnd && !bgmAudioSource.isPlaying) bgmAudioSource.UnPause();

        while (timeElapsed < bgmFadeDuration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            bgmAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / bgmFadeDuration);
            yield return null;
        }

        bgmAudioSource.volume = targetVolume;
        if (pauseAtEnd) bgmAudioSource.Pause();
    }
}