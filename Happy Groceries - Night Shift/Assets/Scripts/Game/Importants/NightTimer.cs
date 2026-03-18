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
        // 1. Congela o jogo e CORTA TODO O ÁUDIO
        Time.timeScale = 0f;
        AudioListener.pause = true; // Calo absoluto de todos os SFX e BGM na cena!

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

        // Limpeza antes de sair (despausa o som para o Menu ter som)
        AudioListener.pause = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(0);
    }
}