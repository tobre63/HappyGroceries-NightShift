using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]

public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1.5f;
    public float TypingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1f;
}
