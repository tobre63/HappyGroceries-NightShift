using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NightTimer : MonoBehaviour
{
    public static NightTimer instance;

    [Header("UI Settings (Relógio)")]
    [SerializeField] private TMP_Text timeText;

    [Header("Night Duration")]
    [SerializeField] private float nightDurationInSeconds = 60f;

    [Header("Time Control")]
    [Range(23f, 29f)]
    public float currentTime = 23f;

    [Header("Transição de Game Over (5:00 AM)")]
    public CanvasGroup gameOverFadeCanvas;
    public TextMeshProUGUI gameOverTextUI;
    public string gameOverMessage = "Bad Ending.\nThanks for playing.";
    public float fadeSpeed = 2f;
    public float textReadingTime = 4f;

    [Header("Áudio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Aviso de Fim de Tempo")]
    public AudioSource tickTockSource; // Coloque o AudioSource com o som de tic-tac aqui
    private bool hasStartedTickTock = false;
    private const float TICK_TOCK_TIME = 28f + (50f / 60f); // Representa 4:50 AM (28.833f)

    private float timeMultiplier;
    private const float END_TIME = 29f;
    private bool isGameOver = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        timeMultiplier = 6f / nightDurationInSeconds;

        if (gameOverFadeCanvas != null)
        {
            gameOverFadeCanvas.alpha = 0f;
            gameOverFadeCanvas.blocksRaycasts = false;
        }

        if (gameOverTextUI != null) gameOverTextUI.text = "";
    }

    void Update()
    {
        if (isGameOver || Time.timeScale == 0f) return;

        if (currentTime < END_TIME)
        {
            currentTime += Time.deltaTime * timeMultiplier;
            
            // Verifica se chegou às 4:50 AM para tocar o Tic-Tac
            if (currentTime >= TICK_TOCK_TIME && !hasStartedTickTock)
            {
                hasStartedTickTock = true;
                if (tickTockSource != null)
                {
                    tickTockSource.Play();
                }
            }
        }
        else
        {
            currentTime = END_TIME;

            if (!isGameOver)
            {
                TriggerBadEnding();
            }
        }

        UpdateClockUI();
    }

    void UpdateClockUI()
    {
        if (timeText == null) return;

        float displayHour = currentTime % 24;
        int hours = Mathf.FloorToInt(displayHour);
        int minutes = Mathf.FloorToInt((displayHour - hours) * 60);

        timeText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }

    private void TriggerBadEnding()
    {
        isGameOver = true;
        StartCoroutine(BadEndingSequence());
    }

    private IEnumerator BadEndingSequence()
    {
        // 1. Congela o jogo e pausa/para os AudioSources específicos
        Time.timeScale = 0f;

        if (musicSource != null) musicSource.Pause();
        if (sfxSource != null) sfxSource.Pause();
        if (tickTockSource != null) tickTockSource.Stop(); // Paramos o tic-tac quando dá 5:00 AM

        if (gameOverTextUI != null) gameOverTextUI.text = gameOverMessage;

        // FADE IN PARA PRETO
        float t = 0f;
        while (t < fadeSpeed)
        {
            t += Time.unscaledDeltaTime; 
            if (gameOverFadeCanvas != null)
            {
                gameOverFadeCanvas.alpha = Mathf.Clamp01(t / fadeSpeed);
                gameOverFadeCanvas.blocksRaycasts = true;
            }
            yield return null;
        }

        // DEIXA LER A MENSAGEM
        yield return new WaitForSecondsRealtime(textReadingTime);

        // Limpeza antes de sair (despausa o som)
        if (musicSource != null) musicSource.UnPause();
        if (sfxSource != null) sfxSource.UnPause();
        
        Time.timeScale = 1f;

        // Torna o rato visível e desbloqueado para o Menu Principal
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(0);
    }
}