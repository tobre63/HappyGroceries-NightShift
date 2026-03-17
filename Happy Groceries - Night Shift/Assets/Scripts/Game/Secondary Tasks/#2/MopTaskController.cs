using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MopTaskController : MonoBehaviour
{
    public static MopTaskController instance;

    [Header("Event State")]
    public bool isQuestActive = false;
    public bool isMopPickedUp = false;
    public int dirtCleanedCount = 0;

    [Header("Settings & References")]
    public int totalDirtToClean = 10;
    public Button taskButton;
    public TMP_Text taskText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Chamado pelo Botão da Clipboard
    public void StartMopTask()
    {
        isQuestActive = true;
        dirtCleanedCount = 0;

        // NOVO: Verifica se o jogador já tem a mop na mão (devido ao evento principal)
        if (CleaningEventController.instance != null && CleaningEventController.instance.isMopEquipped)
        {
            isMopPickedUp = true;
        }

        // Atualiza a UI Secundária de acordo com o estado
        if (SecondaryObjectiveFeedback.instance != null)
        {
            if (isMopPickedUp)
            {
                // Já tem a mop, pede logo para limpar
                SecondaryObjectiveFeedback.instance.SetObjective($"Clean the floor ({dirtCleanedCount}/{totalDirtToClean}).");
            }
            else
            {
                // Não tem a mop, pede para apanhar
                SecondaryObjectiveFeedback.instance.SetObjective("Pick up the mop.");
            }
        }

        // Fecha a clipboard
        ClipboardInteraction clipboard = FindFirstObjectByType<ClipboardInteraction>();
        if (clipboard != null) clipboard.CloseMenu();
    }

    public void CheckProgress()
    {
        if (!isQuestActive) return;

        // Atualiza a UI consoante o estado
        if (isMopPickedUp && dirtCleanedCount < totalDirtToClean)
        {
            if (SecondaryObjectiveFeedback.instance != null)
            {
                SecondaryObjectiveFeedback.instance.SetObjective($"Clean the floor ({dirtCleanedCount}/{totalDirtToClean}).");
            }
        }
        else if (isMopPickedUp && dirtCleanedCount >= totalDirtToClean)
        {
            if (SecondaryObjectiveFeedback.instance != null)
            {
                SecondaryObjectiveFeedback.instance.SetObjective("Put the mop back.");
            }
        }
        else if (!isMopPickedUp && dirtCleanedCount >= totalDirtToClean)
        {
            CompleteTask();
        }
    }

    private void CompleteTask()
    {
        isQuestActive = false;

        // Esconde o objetivo secundário
        if (SecondaryObjectiveFeedback.instance != null)
        {
            SecondaryObjectiveFeedback.instance.HideObjective();
        }

        // Risca o texto na clipboard e desativa o botão
        if (taskButton != null) taskButton.interactable = false;
        if (taskText != null) taskText.fontStyle = FontStyles.Strikethrough;
    }
}