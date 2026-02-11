// DialogueManager.cs - Updated on 2026-02-11 20:47:20

using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour {
    [Header("UI Elements")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialogueBox;

    private NPCDialogue dialogueData;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;

    void Start() {
        InitializeDialogue();
        
        if (dialogueData != null) {
            StartDialogue();
        }
    }

    private void InitializeDialogue() {
        // Create and configure NPCDialogue data programmatically
        dialogueData = ScriptableObject.CreateInstance<NPCDialogue>();
        dialogueData.npcName = "NPC";
        dialogueData.dialogueLines = new string[] {
            "Olá jovem, espero que esteja a ter uma boa noite.",
            "Era só isto, obrigado."
        };
        dialogueData.autoProgressLines = new bool[] {
            true,   // Line 1: Auto-advance enabled
            false   // Line 2: Auto-advance disabled, waits for input
        };
        dialogueData.autoProgressDelay = 1.5f;
        dialogueData.TypingSpeed = 0.05f;
    }

    private void StartDialogue() {
        if (dialogueData == null) {
            Debug.LogWarning("DialogueData is null! Cannot start dialogue.");
            return;
        }

        currentLineIndex = 0;
        
        if (dialogueBox != null) {
            dialogueBox.SetActive(true);
        }

        if (nameText != null) {
            nameText.SetText(dialogueData.npcName);
        }

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine() {
        if (dialogueData == null || currentLineIndex >= dialogueData.dialogueLines.Length) {
            EndDialogue();
            return;
        }

        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
        }

        string line = dialogueData.dialogueLines[currentLineIndex];
        typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(string line) {
        if (dialogueText != null) {
            dialogueText.text = "";
            
            foreach (char c in line) {
                dialogueText.text += c;
                yield return new WaitForSeconds(dialogueData.TypingSpeed);
            }
        }

        // Check if current line should auto-advance
        if (currentLineIndex < dialogueData.autoProgressLines.Length && 
            dialogueData.autoProgressLines[currentLineIndex]) {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            AdvanceDialogue();
        }
    }

    public void AdvanceDialogue() {
        currentLineIndex++;
        DisplayCurrentLine();
    }

    private void EndDialogue() {
        if (dialogueBox != null) {
            dialogueBox.SetActive(false);
        }
        Debug.Log("End of dialogue");
    }

    void Update() {
        // Allow player to manually advance dialogue on input
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            if (dialogueData != null && currentLineIndex < dialogueData.dialogueLines.Length) {
                // Check if current line is NOT set to auto-advance
                if (currentLineIndex < dialogueData.autoProgressLines.Length && 
                    !dialogueData.autoProgressLines[currentLineIndex]) {
                    AdvanceDialogue();
                }
            }
        }
    }
}
