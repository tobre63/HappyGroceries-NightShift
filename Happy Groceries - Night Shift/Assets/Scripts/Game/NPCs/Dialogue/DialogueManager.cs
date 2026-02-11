using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Data")]
    public NPCDialogue dialogueData;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;

    [Header("Interaction")]
    public KeyCode interactionKey = KeyCode.E;
    public string playerTag = "Player";

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private bool isPlayerInRange;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Set up the specific dialogue sequence
        if (dialogueData == null)
        {
            dialogueData = ScriptableObject.CreateInstance<NPCDialogue>();
        }

        dialogueData.npcName = "NPC";
        dialogueData.dialogueLines = new string[]
        {
            "Olá jovem, espero que esteja a ter uma boa noite.",
            "Era só isto, obrigado."
        };
        dialogueData.autoProgressLines = new bool[]
        {
            true,  // Line 0: auto-progress
            false  // Line 1: wait for player interaction
        };
        dialogueData.autoProgressDelay = 1.5f;
        dialogueData.TypingSpeed = 0.05f;
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            if (isDialogueActive)
                NextLine();
            else
                StartDialogue();
        }
    }

    void StartDialogue()
    {
        if (dialogueData == null) return;

        isDialogueActive = true;
        dialogueIndex = 0;

        if (nameText != null)
            nameText.SetText(dialogueData.npcName);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (GameManager.Instance != null)
            GameManager.Instance.PauseWithoutMenu();

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (dialogueIndex + 1 < dialogueData.dialogueLines.Length)
        {
            dialogueIndex++;
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.TypingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;

        if (dialogueText != null)
            dialogueText.SetText("");

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.Resume();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            EndDialogue();
        }
    }
}