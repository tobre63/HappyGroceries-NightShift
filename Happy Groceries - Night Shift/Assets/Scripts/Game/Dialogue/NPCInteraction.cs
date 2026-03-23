using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DialogueChoice
{
    public string textoDoBotao;
    public int proximoNode = -1;
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string frase;
    public AudioClip vozDaFrase; // Arraste o MP3 espec�fico da frase aqui
}

[System.Serializable]
public class DialogueNode
{
    public DialogueLine[] frasesDoNPC;
    public DialogueChoice[] escolhas;
}

public class NPCInteraction : MonoBehaviour
{
    public static bool isPlayerTalking = false;

    [Header("UI Toggles")]
    public GameObject interactionIcon;
    public GameObject talkGUI;

    [Header("Text Components")]
    public TMP_Text clientNameText;
    public TMP_Text clientTalkText;

    [Header("Player Buttons")]
    public Button[] playerButtons;
    public TMP_Text[] playerButtonsTexts;

    [Header("Dialogue Tree Settings")]
    public string npcName = "Cliente Misterioso";
    public DialogueNode[] dialogueNodes;

    [Header("Audio & Animation Settings")]
    public float typingSpeed = 0.04f;
    public AudioClip typingSound;
    public AudioClip defaultVoiceClip; // Voz padr�o se a frase n�o tiver �udio

    [Header("Machine Animation")]
    public bool usesMachine = false; // Ativa isto no Inspector apenas nos NPCs que compram
    public Animator machineAnimator;

    private Animator anim;
    private AudioSource audioSource;
    private NPCController npcController;

    private bool playerInRange = false;
    private bool isTyping = false;
    private int dialogueState = 0;
    private int currentNodeIndex = 0;
    private int currentLineIndex = 0;
    public bool autoStartDialogue = false;

    void Start()
    {
        npcController = GetComponent<NPCController>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        CloseAllUI();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return;

        if (npcController != null && (!npcController.isActiveInWorld || npcController.isFading))
        {
            if (dialogueState != 0) CloseAllUI();
            return;
        }

        bool isReadyToTalk = (npcController == null) || npcController.isWaitingForInteraction;

        if (playerInRange && isReadyToTalk)
        {
            if (dialogueState == 0 && interactionIcon != null && !interactionIcon.activeSelf && !autoStartDialogue)
                interactionIcon.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (dialogueState == 0) StartDialogue();
                else if (dialogueState == 1)
                {
                    if (isTyping)
                    {
                        StopAllCoroutines();
                        if (audioSource.isPlaying) audioSource.Stop();
                        FinishTyping(dialogueNodes[currentNodeIndex]);
                    }
                    else NextLine();
                }
            }
        }
        else
        {
            if (dialogueState == 0 && interactionIcon != null && interactionIcon.activeSelf)
                interactionIcon.SetActive(false);
        }
    }

    public void StartDialogue()
    {
        if (dialogueNodes == null || dialogueNodes.Length == 0) return;

        isPlayerTalking = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        dialogueState = 1;
        currentNodeIndex = 0;
        currentLineIndex = 0;

        if (talkGUI != null) talkGUI.SetActive(true);
        if (clientNameText != null) clientNameText.text = npcName;

        EsconderBotoes();
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (interactionIcon != null) interactionIcon.SetActive(false);

        if (usesMachine && currentNodeIndex == 0 && currentLineIndex == 0)
        {
            if (machineAnimator != null)
            {
                machineAnimator.SetBool("isOpen", true);
                Invoke("CloseMachine", 5f); // Usa Invoke em vez de Coroutine
            }
        }

        DialogueNode currentNode = dialogueNodes[currentNodeIndex];
        DialogueLine currentLine = currentNode.frasesDoNPC[currentLineIndex];

        // --- SISTEMA DE VOZ ---
        if (audioSource != null)
        {
            audioSource.Stop();
            AudioClip clipToPlay = currentLine.vozDaFrase != null ? currentLine.vozDaFrase : defaultVoiceClip;

            if (clipToPlay != null)
            {
                audioSource.clip = clipToPlay;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        StartCoroutine(TypeLine(currentLine.frase, currentNode));
    }

    private IEnumerator TypeLine(string line, DialogueNode currentNode)
    {
        isTyping = true;
        clientTalkText.text = "";

        if (anim != null) anim.SetBool("isTalking", true);

        foreach (char c in line.ToCharArray())
        {
            clientTalkText.text += c;

            if (typingSound != null && audioSource != null && char.IsLetterOrDigit(c))
            {
                audioSource.PlayOneShot(typingSound, 0.2f);
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        FinishTyping(currentNode);
    }

    private void FinishTyping(DialogueNode currentNode)
    {
        isTyping = false;
        if (clientTalkText != null)
            clientTalkText.text = currentNode.frasesDoNPC[currentLineIndex].frase;

        if (anim != null) anim.SetBool("isTalking", false);

        if (currentLineIndex == currentNode.frasesDoNPC.Length - 1)
        {
            if (currentNode.escolhas != null && currentNode.escolhas.Length > 0)
                MostrarBotoesDeEscolha(currentNode);
            else if (interactionIcon != null)
                interactionIcon.SetActive(true);
        }
        else if (interactionIcon != null)
        {
            interactionIcon.SetActive(true);
        }
    }

    private void NextLine()
    {
        DialogueNode currentNode = dialogueNodes[currentNodeIndex];

        if (currentLineIndex < currentNode.frasesDoNPC.Length - 1)
        {
            currentLineIndex++;
            ShowCurrentLine();
        }
        else EndDialogue();
    }

    private void MostrarBotoesDeEscolha(DialogueNode node)
    {
        dialogueState = 2;
        if (interactionIcon != null) interactionIcon.SetActive(false);
        EsconderBotoes();

        int numberOfChoices = Mathf.Min(node.escolhas.Length, playerButtons.Length);

        for (int i = 0; i < numberOfChoices; i++)
        {
            playerButtons[i].gameObject.SetActive(true);
            if (playerButtonsTexts[i] != null)
                playerButtonsTexts[i].text = node.escolhas[i].textoDoBotao;

            playerButtons[i].onClick.RemoveAllListeners();
            int index = i;
            playerButtons[i].onClick.AddListener(() => EscolheuOpcao(index));
        }
    }

    public void EscolheuOpcao(int indexDoBotao)
    {
        if (dialogueState != 2) return;
        DialogueNode currentNode = dialogueNodes[currentNodeIndex];
        if (indexDoBotao >= currentNode.escolhas.Length) return;

        int proximoNode = currentNode.escolhas[indexDoBotao].proximoNode;

        if (proximoNode == -1 || proximoNode >= dialogueNodes.Length) EndDialogue();
        else
        {
            currentNodeIndex = proximoNode;
            currentLineIndex = 0;
            dialogueState = 1;
            EsconderBotoes();
            ShowCurrentLine();
        }
    }

    private void EsconderBotoes()
    {
        foreach (Button btn in playerButtons) if (btn != null) btn.gameObject.SetActive(false);
    }

    private void EndDialogue()
    {
        if (audioSource.isPlaying) audioSource.Stop();
        CloseAllUI();

        CocaColaSpillEvent spillEvent = GetComponent<CocaColaSpillEvent>();
        if (spillEvent != null && !spillEvent.hasSpilled)
        {
            spillEvent.StartSpillSequence();
            return;
        }
        else if (spillEvent != null && spillEvent.hasSpilled)
        {
            spillEvent.EndSpillSequence();
        }

        ChildBathroomEvent childEvent = GetComponent<ChildBathroomEvent>();
        if (childEvent != null) childEvent.OnDialogueFinished();

        if (npcController != null) npcController.ResumeMovement();
    }

    public void OnPlayerEnter()
    {
        playerInRange = true;
        if (autoStartDialogue && dialogueState == 0)
        {
            ChildBathroomEvent childEvent = GetComponent<ChildBathroomEvent>();
            if (childEvent != null) childEvent.StopAudio();
            StartDialogue();
        }
    }

    public void OnPlayerExit()
    {
        playerInRange = false;
        if (dialogueState != 0) CloseAllUI();
    }

    private void CloseAllUI()
    {
        dialogueState = 0;
        isTyping = false;
        StopAllCoroutines();

        // NOVO: Cancela o temporizador se ainda estiver a contar e força a máquina a fechar
        CancelInvoke("CloseMachine");
        CloseMachine();

        if (anim != null) anim.SetBool("isTalking", false);
        isPlayerTalking = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (talkGUI != null) talkGUI.SetActive(false);
        EsconderBotoes();
    }

    private void CloseMachine()
    {
        if (machineAnimator != null)
        {
            machineAnimator.SetBool("isOpen", false);
        }
    }
}