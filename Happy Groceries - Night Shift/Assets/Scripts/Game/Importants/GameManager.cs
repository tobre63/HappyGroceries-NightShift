using UnityEngine;

using UnityEngine.SceneManagement;

using UnityEngine.InputSystem;



public class GameManager : MonoBehaviour

{

    // Permite que outros scripts acessem o GameManager via GameManager.Instance

    public static GameManager Instance { get; private set; }



    [Header("Configurações de UI")]

    public GameObject pauseMenu;

    public GameObject settingsMenu;

    public GameObject settingsButton;


    // 'public get' permite ler de fora. 'private set' impede que outros scripts alterem.

    public bool isPaused { get; private set; } = false;



    void Awake()

    {

        // Garante que só exista um GameManager na cena

        if (Instance == null) { Instance = this; }

        else { Destroy(gameObject); }

    }



    void Start()

    {

        // Garante estado inicial limpo

        Resume();

    }



    void Update()

    {

        // O New Input System detecta o pressionamento uma vez por frame com .wasPressedThisFrame

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)

        {

            HandleEscPress();

        }

    }



    private void HandleEscPress()

    {

        // Prioridade 1: Se o menu de configurações estiver aberto, apenas volta para o pause

        if (settingsMenu != null && settingsMenu.activeSelf)

        {

            CloseSettings();

        }

        // Prioridade 2: Se estiver pausado (mas no menu principal de pausa), despausa

        else if (isPaused)

        {

            Resume();

        }

        // Prioridade 3: Se o jogo estiver rodando, pausa

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
        // Garante a altura do botão SEMPRE no clique
        if (settingsButton != null)
        {
            RectTransform rt = settingsButton.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 160f);
            }
        }

        // Abre o menu de settings
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

