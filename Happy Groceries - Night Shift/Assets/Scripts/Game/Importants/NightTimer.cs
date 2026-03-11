using UnityEngine;
using TMPro;

public class NightTimer : MonoBehaviour
{
    public static NightTimer instance; // Singleton para acesso global

    [Header("UI Settings")]
    [SerializeField] private TMP_Text timeText;

    [Header("Night Duration")]
    [SerializeField] private float nightDurationInSeconds = 60f;

    [Header("Time Control")]
    [Range(23f, 29f)]
    public float currentTime = 23f; // 23=23:00, 24=00:00, 30=06:00

    private float timeMultiplier;
    private const float END_TIME = 29f;

    private void Awake()
    {
        // Cria a instância global
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        timeMultiplier = 6f / nightDurationInSeconds;
    }

    void Update()
    {
        if (currentTime < END_TIME)
        {
            currentTime += Time.deltaTime * timeMultiplier;
        }
        else
        {
            currentTime = END_TIME;
            // Fim da noite
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
}