using UnityEngine;

public class TrashEventController : MonoBehaviour
{
    public static TrashEventController instance;

    [Header("Settings")]
    public float questStartHour = 25f; // 01:00 da manhã
    public int totalTrashCans = 2;     // QUANTAS lixeiras existem na loja?

    [Header("Event State")]
    public bool isQuestActive = false;
    public bool isTaskCompleted = false;

    private int trashCollectedCount = 0; // Quantas já pegamos

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isTaskCompleted || isQuestActive) return;

        if (NightTimer.instance != null)
        {
            if (NightTimer.instance.currentTime >= questStartHour)
            {
                StartTrashQuest();
            }
        }
    }

    public void StartTrashQuest()
    {
        isQuestActive = true;
        trashCollectedCount = 0; // Reinicia a contagem
        Debug.Log("Evento do Lixo Iniciado!");

        if (ObjectiveFeedback.instance != null)
        {
            // Mostra algo como "Take out the trash (0/2)"
            UpdateObjectiveText();
        }
    }

    public void OnTrashPickedUp()
    {
        if (!isQuestActive) return;

        trashCollectedCount++; // Aumenta contagem

        // Se ainda não pegou todas...
        if (trashCollectedCount < totalTrashCans)
        {
            UpdateObjectiveText();
        }
        else
        {
            // Se JÁ pegou todas, muda para o objetivo de ir lá fora
            if (ObjectiveFeedback.instance != null)
            {
                ObjectiveFeedback.instance.RemoveSpecificObjective("Take out the trash"); // Remove genérico
                // Nota: Se o texto anterior tinha contagem, removemos ele também na atualização
                ObjectiveFeedback.instance.SetObjective("Throw it in the dumpster outside.", true);
            }
        }
    }

    public void OnTrashDisposed()
    {
        if (!isQuestActive) return;

        // Só permite descartar se TIVER pego tudo (opcional, mas seguro)
        if (trashCollectedCount < totalTrashCans) return;

        if (ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.RemoveSpecificObjective("Throw it in the dumpster outside.");
        }

        isQuestActive = false;
        isTaskCompleted = true;

        // Reseta o estado do jogador no TaskManager para ele ficar "livre"
        if (TaskManager.instance != null)
            TaskManager.instance.hasTrash = false;

        Debug.Log("Evento do Lixo Finalizado!");
    }

    private void UpdateObjectiveText()
    {
        if (ObjectiveFeedback.instance != null)
        {
            // Remove o anterior para não duplicar
            ObjectiveFeedback.instance.RemoveSpecificObjective("Take out the trash");

            // Adiciona com contagem atualizada
            string text = $"Take out the trash ({trashCollectedCount}/{totalTrashCans})";
            ObjectiveFeedback.instance.SetObjective(text, true);
        }
    }

    // Helper para as lixeiras saberem se podem ser recolhidas
    public bool AreAllTrashCansCollected()
    {
        return trashCollectedCount >= totalTrashCans;
    }
}