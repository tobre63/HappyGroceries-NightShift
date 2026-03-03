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
public class DialogueNode
{
    [TextArea(2, 4)]
    public string[] frasesDoNPC;
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

        // --- VERIFICAÇÃO 1: O NPC está ativo e não está a desaparecer? ---
        if (npcController != null && (!npcController.isActiveInWorld || npcController.isFading))
        {
            if (dialogueState != 0) CloseAllUI();
            return;
        }

        // --- VERIFICAÇÃO 2: Está pronto a falar? ---
        // Se NÃO tiver NPCController (como a Criança), assume que está sempre pronto a falar!
        bool isReadyToTalk = (npcController == null) || npcController.isWaitingForInteraction;

        // --- LÓGICA DE INTERAÇÃO ---
        if (playerInRange && isReadyToTalk)
        {
            if (dialogueState == 0 && interactionIcon != null && !interactionIcon.activeSelf && !autoStartDialogue)
                interactionIcon.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (dialogueState == 0)
                {
                    StartDialogue();
                }
                else if (dialogueState == 1)
                {
                    if (isTyping)
                    {
                        StopAllCoroutines();
                        FinishTyping(dialogueNodes[currentNodeIndex]);
                    }
                    else
                    {
                        NextLine();
                    }
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

        DialogueNode currentNode = dialogueNodes[currentNodeIndex];
        string lineToType = currentNode.frasesDoNPC[currentLineIndex];

        StartCoroutine(TypeLine(lineToType, currentNode));
    }

    private IEnumerator TypeLine(string line, DialogueNode currentNode)
    {
        isTyping = true;
        clientTalkText.text = "";

        if (anim != null) anim.SetBool("isTalking", true);

        foreach (char c in line.ToCharArray())
        {
            clientTalkText.text += c;

            // NOVO: Só toca som se for letra ou número (ignora espaços, vírgulas, barras, etc)
            if (typingSound != null && audioSource != null && char.IsLetterOrDigit(c))
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f); // Variação de pitch mais notória
                audioSource.PlayOneShot(typingSound, 0.5f);
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        FinishTyping(currentNode);
    }

    private void FinishTyping(DialogueNode currentNode)
    {
        isTyping = false;

        if (clientTalkText != null)
            clientTalkText.text = currentNode.frasesDoNPC[currentLineIndex];

        if (anim != null) anim.SetBool("isTalking", false);

        if (currentLineIndex == currentNode.frasesDoNPC.Length - 1)
        {
            if (currentNode.escolhas != null && currentNode.escolhas.Length > 0)
            {
                MostrarBotoesDeEscolha(currentNode);
            }
            else if (interactionIcon != null)
            {
                interactionIcon.SetActive(true);
            }
        }
        else
        {
            if (interactionIcon != null) interactionIcon.SetActive(true);
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
        else
        {
            EndDialogue();
        }
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

            // NOVA LÓGICA: Limpa os donos antigos do botão e adiciona o NPC atual!
            playerButtons[i].onClick.RemoveAllListeners();

            int index = i; // Extremamente importante guardar o valor de 'i' nesta variável para o listener funcionar
            playerButtons[i].onClick.AddListener(() => EscolheuOpcao(index));
        }
    }

    public void EscolheuOpcao(int indexDoBotao)
    {
        // ESCUDO 1: Se ESTE NPC não estiver à espera de uma escolha, ignora o clique!
        if (dialogueState != 2) return;

        DialogueNode currentNode = dialogueNodes[currentNodeIndex];

        // ESCUDO 2: Proteção contra o erro IndexOutOfRange (caso a opção não exista neste Node)
        if (indexDoBotao >= currentNode.escolhas.Length) return;

        int proximoNode = currentNode.escolhas[indexDoBotao].proximoNode;

        if (proximoNode == -1 || proximoNode >= dialogueNodes.Length)
        {
            EndDialogue();
        }
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
        foreach (Button btn in playerButtons)
        {
            if (btn != null) btn.gameObject.SetActive(false);
        }
    }

    private void EndDialogue()
    {
        CloseAllUI();

        // --- VERIFICAÇÃO DE EVENTOS ESPECIAIS (Ex: Coca-Cola) ---
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

        // --- NOVO: VERIFICAÇÃO DO EVENTO DA CRIANÇA NA CASA DE BANHO ---
        ChildBathroomEvent childEvent = GetComponent<ChildBathroomEvent>();
        if (childEvent != null)
        {
            childEvent.OnDialogueFinished();
        }
        // --------------------------------------------------------

        if (npcController != null) npcController.ResumeMovement();
    }

    public void OnPlayerEnter()
    {
        playerInRange = true;

        // Se este NPC estiver configurado para falar automaticamente:
        if (autoStartDialogue && dialogueState == 0)
        {
            // Corta o som da casa de banho instantaneamente se for a Criança!
            ChildBathroomEvent childEvent = GetComponent<ChildBathroomEvent>();
            if (childEvent != null)
            {
                childEvent.StopAudio();
            }

            // Arranca a conversa sem esperar pelo 'E'
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

        if (anim != null) anim.SetBool("isTalking", false);

        isPlayerTalking = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (talkGUI != null) talkGUI.SetActive(false);
        EsconderBotoes();
    }
}