// DialogueManager.cs - Updated on 2026-02-11 21:10:00

using UnityEngine;
using System.Collections;

public class DialogueManager : MonoBehaviour {
    private NPCDialogue dialogueData;
    private int currentLineIndex = 0;
    private bool isDisplayingDialogue = false;
    private Coroutine autoProgressCoroutine;

    void Start() {
        // Create and configure the dialogue data programmatically
        // Note: This runtime creation is intentional per requirements, though typically
        // ScriptableObjects should be created as assets in the Unity Editor
        dialogueData = ScriptableObject.CreateInstance<NPCDialogue>();
        dialogueData.npcName = "NPC";
        dialogueData.dialogueLines = new string[] {
            "Olá jovem, espero que esteja a ter uma boa noite.",
            "Era só isto, obrigado."
        };
        dialogueData.autoProgressLines = new bool[] {
            true,   // Line 0: Auto-progress
            false   // Line 1: Wait for player interaction
        };
        dialogueData.autoProgressDelay = 1.5f;
        dialogueData.TypingSpeed = 0.05f;
        
        // Start the dialogue automatically
        StartDialogue(dialogueData);
    }

    public void StartDialogue(NPCDialogue dialogueData) {
        if (dialogueData == null) {
            Debug.LogError("DialogueData is null!");
            return;
        }

        this.dialogueData = dialogueData;
        currentLineIndex = 0;
        isDisplayingDialogue = true;

        Debug.Log($"Starting dialogue with {dialogueData.npcName}");
        
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine() {
        if (currentLineIndex >= dialogueData.dialogueLines.Length) {
            EndDialogue();
            return;
        }

        string currentLine = dialogueData.dialogueLines[currentLineIndex];
        Debug.Log($"{dialogueData.npcName}: {currentLine}");

        // Check if this line should auto-progress
        bool shouldAutoProgress = currentLineIndex < dialogueData.autoProgressLines.Length 
            && dialogueData.autoProgressLines[currentLineIndex];

        if (shouldAutoProgress) {
            // Stop any existing auto-progress coroutine
            if (autoProgressCoroutine != null) {
                StopCoroutine(autoProgressCoroutine);
            }
            // Start auto-progress for this line
            autoProgressCoroutine = StartCoroutine(AutoProgressToNextLine());
        } else {
            Debug.Log("(Waiting for player interaction...)");
        }
    }

    private IEnumerator AutoProgressToNextLine() {
        yield return new WaitForSeconds(dialogueData.autoProgressDelay);
        ProgressToNextLine();
    }

    public void ProgressToNextLine() {
        if (!isDisplayingDialogue) {
            return;
        }

        // Stop any auto-progress coroutine when player manually advances
        if (autoProgressCoroutine != null) {
            StopCoroutine(autoProgressCoroutine);
            autoProgressCoroutine = null;
        }

        currentLineIndex++;
        DisplayCurrentLine();
    }

    void EndDialogue() {
        isDisplayingDialogue = false;
        
        // Stop any running auto-progress coroutine
        if (autoProgressCoroutine != null) {
            StopCoroutine(autoProgressCoroutine);
            autoProgressCoroutine = null;
        }

        Debug.Log("End of dialogue");
    }

    void Update() {
        // Allow player to manually advance dialogue with a key press (e.g., Space or Enter)
        if (isDisplayingDialogue && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))) {
            ProgressToNextLine();
        }
    }
}