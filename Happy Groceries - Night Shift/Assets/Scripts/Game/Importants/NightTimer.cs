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
    public float currentTime = 23f; // 23=23:00, 24=00:00, 29=05:00

    [Header("Transição de Game Over (5:00 AM)")]
    [Tooltip("Arrasta o painel preto que vai tapar o ecrã")]
    public CanvasGroup gameOverFadeCanvas;
    [Tooltip("O texto que vai dizer 'Bad Ending'")]
    public TextMeshProUGUI gameOverTextUI;
    public string gameOverMessage = "Bad Ending. - Thanks for playing.";
    public float fadeSpeed = 2f;
    public float textReadingTime = 4f;

    private float timeMultiplier;
    private const float END_TIME = 29f; // 05:00 AM
    private bool isGameOver = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        timeMultiplier = 6f / nightDurationInSeconds;

        // Garante que o painel de Game Over começa invisível
        if (gameOverFadeCanvas != null)
        {
            gameOverFadeCanvas.alpha = 0f;
            gameOverFadeCanvas.blocksRaycasts = false;
        }

        if (gameOverTextUI != null)
        {
            gameOverTextUI.text = "";
        }
    }

    void Update()
    {
        // Se o jogo estiver em pausa ou já tiver acabado, o tempo não avança
        if (isGameOver || Time.timeScale == 0f) return;

        if (currentTime < END_TIME)
        {
            currentTime += Time.deltaTime * timeMultiplier;
        }
        else
        {
            currentTime = END_TIME;

            // SE BATEU AS 5H DA MANHÃ, DESPOLETA O FINAL MAU!
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
        // Congela o mundo (o jogador e o assassino param)
        Time.timeScale = 0f;

        if (gameOverTextUI != null)
        {
            gameOverTextUI.text = gameOverMessage;
        }

        // FADE IN PARA PRETO
        float t = 0f;
        while (t < fadeSpeed)
        {
            t += Time.unscaledDeltaTime;
            if (gameOverFadeCanvas != null)
            {
                gameOverFadeCanvas.alpha = Mathf.Clamp01(t / fadeSpeed);
                gameOverFadeCanvas.blocksRaycasts = true; // Impede o jogador de clicar noutras coisas
            }
            yield return null;
        }

        // DEIXA LER A MENSAGEM
        yield return new WaitForSecondsRealtime(textReadingTime);

        // VAI PARA O MENU PRINCIPAL (Cena 0)
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}