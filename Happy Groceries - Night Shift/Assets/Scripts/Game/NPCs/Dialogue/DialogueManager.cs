// DialogueManager.cs - Updated on 2026-02-11 20:47:20

using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour {
    [SerializeField] private NPCDialogue dialogueData;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    
    private int currentLineIndex = 0;

    void Start() {
        InitializeDialogue();
    }

    private void InitializeDialogue() {
        if (dialogueData == null) {
            dialogueData = ScriptableObject.CreateInstance<NPCDialogue>();
            dialogueData.npcName = "NPC";
            dialogueData.dialogueLines = new string[] {
                "Olá jovem, espero que esteja a ter uma boa noite.",
                "Era só isto, obrigado."
            };
            dialogueData.autoProgressLines = new bool[] { true, false };
            dialogueData.autoProgressDelay = 2.0f;
            dialogueData.TypingSpeed = 0.05f;
        }
    }

    public void StartDialogue() {
        if (dialogueData == null) return;
        
        currentLineIndex = 0;
        if (nameText != null) {
            nameText.SetText(dialogueData.npcName);
        }
        DisplayLine();
    }

    private void DisplayLine() {
        if (currentLineIndex >= dialogueData.dialogueLines.Length) {
            EndDialogue();
            return;
        }

        string line = dialogueData.dialogueLines[currentLineIndex];
        StopAllCoroutines();
        StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(string line) {
        if (dialogueText != null) {
            dialogueText.text = "";
            foreach (char c in line) {
                dialogueText.text += c;
                yield return new WaitForSeconds(dialogueData.TypingSpeed);
            }
        }

        // Check if this line should auto-progress
        if (currentLineIndex < dialogueData.autoProgressLines.Length && 
            dialogueData.autoProgressLines[currentLineIndex]) {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void NextLine() {
        StopAllCoroutines();
        currentLineIndex++;
        DisplayLine();
    }

    void EndDialogue() {
        Debug.Log("End of dialogue");
    }
}
