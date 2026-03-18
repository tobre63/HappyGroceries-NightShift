using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameEndManager : MonoBehaviour
{
    [Header("Sistema de Relógio")]
    public float realSecondsPerGameMinute = 1f; // 1 segundo real = 1 minuto no jogo
    public int startHour = 0; // Começa à Meia-Noite
    public int endHour = 5;   // Acaba às 5:00 AM
    public TextMeshProUGUI clockText; // Para mostrares a hora no canto do ecrã

    [Header("Transição de Game Over (Tempo Esgotado)")]
    public CanvasGroup gameOverFadeCanvas;
    public TextMeshProUGUI gameOverText;
    public string gameOverMessage = "5:00 AM\nYou were caught inside...";
    public float fadeDuration = 2f;
    public float textReadingTime = 3f;

    private float timer = 0f;
    private int currentMinute = 0;
    private int currentHour = 0;
    private bool isGameOver = false;

    void Start()
    {
        currentHour = startHour;
        currentMinute = 0;
        UpdateClockUI();

        if (gameOverFadeCanvas != null)
        {
            gameOverFadeCanvas.alpha = 0f;
            gameOverFadeCanvas.blocksRaycasts = false;
        }
    }

    void Update()
    {
        // Se o jogo já acabou, ou se estiver em pausa, não conta o tempo
        if (isGameOver || Time.timeScale == 0f) return;

        timer += Time.deltaTime;

        if (timer >= realSecondsPerGameMinute)
        {
            timer = 0f;
            currentMinute++;

            if (currentMinute >= 60)
            {
                currentMinute = 0;
                currentHour++;

                // BATEU AS 5 DA MANHÃ!
                if (currentHour >= endHour)
                {
                    TriggerGameOver();
                }
            }

            UpdateClockUI();
        }
    }

    private void UpdateClockUI()
    {
        if (clockText != null)
        {
            // Formata o texto para ficar "00:00", "01:15", "05:00"
            clockText.text = currentHour.ToString("00") + ":" + currentMinute.ToString("00");
        }
    }

    private void TriggerGameOver()
    {
        isGameOver = true;
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        Time.timeScale = 0f; // Para o jogo todo

        if (gameOverText != null) gameOverText.text = gameOverMessage;

        // Faz o Fade In para Preto
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (gameOverFadeCanvas != null) gameOverFadeCanvas.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        // Deixa o jogador ler "Game Over"
        yield return new WaitForSecondsRealtime(textReadingTime);

        // Limpa o tempo e carrega a Cena 0 (Menu)
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}