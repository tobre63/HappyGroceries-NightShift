using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClothTaskController : MonoBehaviour
{
    public static ClothTaskController instance;

    [Header("Event State")]
    public bool isQuestActive = false;
    public bool isClothPickedUp = false;
    public bool isTableCleaned = false;

    [Header("References")]
    public BoxCollider2D clothCollider;
    public Button taskButton;
    public TMP_Text taskText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (clothCollider != null) clothCollider.enabled = false;
    }

    public void StartClothTask()
    {
        isQuestActive = true;

        if (clothCollider != null) clothCollider.enabled = true;

        // USA AGORA O NOVO SISTEMA SECUNDÁRIO
        if (SecondaryObjectiveFeedback.instance != null)
        {
            SecondaryObjectiveFeedback.instance.SetObjective("Pick up the cloth.");
        }

        ClipboardInteraction clipboard = FindFirstObjectByType<ClipboardInteraction>();
        if (clipboard != null) clipboard.CloseMenu();
    }

    public void CheckProgress()
    {
        if (!isQuestActive) return;

        if (isClothPickedUp && !isTableCleaned)
        {
            if (SecondaryObjectiveFeedback.instance != null)
            {
                SecondaryObjectiveFeedback.instance.SetObjective("Clear the counter.");
            }
        }
        else if (isTableCleaned && isClothPickedUp)
        {
            if (SecondaryObjectiveFeedback.instance != null)
            {
                SecondaryObjectiveFeedback.instance.SetObjective("Put the cloth back.");
            }
        }
        else if (isTableCleaned && !isClothPickedUp)
        {
            CompleteTask();
        }
    }

    private void CompleteTask()
    {
        isQuestActive = false;

        // ESCONDE O OBJETIVO SECUNDÁRIO
        if (SecondaryObjectiveFeedback.instance != null)
        {
            SecondaryObjectiveFeedback.instance.HideObjective();
        }

        if (clothCollider != null) clothCollider.enabled = false;

        if (taskButton != null) taskButton.interactable = false;

        if (taskText != null)
        {
            taskText.fontStyle = FontStyles.Strikethrough;
        }
    }
}