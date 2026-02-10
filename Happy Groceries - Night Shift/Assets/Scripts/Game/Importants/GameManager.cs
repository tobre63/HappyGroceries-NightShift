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

    public bool isPaused { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
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
    }

    public void PauseWithoutMenu()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        SceneManager.LoadScene("Menu");
    }

    public void Back()
    {
        isPaused = true;

        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }
}