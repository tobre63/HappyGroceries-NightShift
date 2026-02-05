using UnityEngine;

public class NPCScheduleManager : MonoBehaviour
{
    [Header("NPC Settings")]
    public GameObject npcObject; // Arraste o NPC aqui
    public float activationHour = 22f;

    [Header("Timer Reference")]
    [SerializeField] private NightTimer nightTimer;

    void Start()
    {
        if (nightTimer == null)
            nightTimer = FindFirstObjectByType<NightTimer>();

        // Desativa o NPC no início se ainda não for hora
        if (nightTimer != null && nightTimer.currentTime < activationHour)
        {
            if (npcObject != null)
                npcObject.SetActive(false);
        }
    }

    void Update()
    {
        if (nightTimer == null || npcObject == null) return;

        bool shouldBeActive = nightTimer.currentTime >= activationHour;

        // Ativa/desativa baseado na hora
        if (npcObject.activeSelf != shouldBeActive)
        {
            npcObject.SetActive(shouldBeActive);
        }
    }
}