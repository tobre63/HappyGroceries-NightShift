using UnityEngine;
using TMPro;

public class NightTimer : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TMP_Text timeText;  // Texto onde o tempo vai ser mostrado

    [Header("Night Duration")]
    [SerializeField] private float nightDurationInSeconds = 60f; // Quanto tempo dura a noite na vida real

    [Header("Time Control")]
    [Range(21f, 30f)] // Slider para facilitar testes
    public float currentTime = 21f; // Hora atual (21 = 21:00, 24 = 00:00, 25 = 01:00, 30 = 06:00)

    private float timeMultiplier;
    private const float END_TIME = 30f; // 30 representa 06:00 da manha (24 + 6)

    void Start()
    {
        // Calcula a velocidade do tempo com base na duracao escolhida
        // Noite vai das 21h as 30h (9 horas de jogo)
        timeMultiplier = 9f / nightDurationInSeconds;
    }

    void Update()
    {
        // Avanca o tempo se ainda nao chegou as 06:00
        if (currentTime < END_TIME)
        {
            currentTime += Time.deltaTime * timeMultiplier;
        }
        else
        {
            // Para exatamente as 06:00
            currentTime = END_TIME;
            // Aqui podes adicionar codigo para "Victory" ou "End of Night"
        }

        UpdateClockUI();
    }

    void UpdateClockUI()
    {
        // Faz o "loop" do relogio usando modulo 24
        float displayHour = currentTime % 24;

        int hours = Mathf.FloorToInt(displayHour);
        int minutes = Mathf.FloorToInt((displayHour - hours) * 60);

        // Formata como 00:00
        timeText.text = string.Format("{0:00}:{1:00}", hours, minutes);
    }
}