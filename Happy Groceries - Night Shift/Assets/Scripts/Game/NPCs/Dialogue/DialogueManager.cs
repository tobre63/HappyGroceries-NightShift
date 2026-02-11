// DialogueManager.cs - Updated on 2026-02-11 20:47:20

using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DialogueManager : MonoBehaviour {
    private Queue<string> sentences;
    public NPCDialogue dialogueData;
    public TextMeshProUGUI nameText;

    void Start() {
        sentences = new Queue<string>();
        InitializeDialogue();
    }

    private void InitializeDialogue() {
        // Create a new instance if dialogueData is null
        if (dialogueData == null) {
            dialogueData = ScriptableObject.CreateInstance<NPCDialogue>();
        }

        // Set npcName
        dialogueData.npcName = "NPC";

        // Set dialogueLines with the two specific strings
        dialogueData.dialogueLines = new string[] {
            "Olá jovem, espero que esteja a ter uma boa noite.",
            "Era só isto, obrigado."
        };

        // Set autoProgressLines - first line auto-advances, second waits for input
        dialogueData.autoProgressLines = new bool[] { true, false };

        // Set default delays
        dialogueData.autoProgressDelay = 2f;
        dialogueData.TypingSpeed = 0.05f;
    }

    public void StartDialogue(Dialogue dialogue) {
        // Fix the property access bug: use npcName instead of name
        if (nameText != null) {
            nameText.SetText(dialogueData.npcName);
        }
        
        sentences.Clear();

        foreach (string sentence in dialogue.sentences) {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence() {
        if (sentences.Count == 0) {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        Debug.Log(sentence); // Display sentence on the screen.
    }

    void EndDialogue() {
        Debug.Log("End of dialogue");
    }
}