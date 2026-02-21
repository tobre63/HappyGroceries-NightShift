using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Estruturas de dados para a criação da árvore de diálogo no Inspector
[System.Serializable]
public class DialogueChoice
{
    public string textoDoBotao;
    public int proximoNode = -1; // -1 indica o fim da conversa
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(2, 4)]
    public string[] frasesDoNPC; // Linhas de diálogo do NPC antes de dar escolhas
    public DialogueChoice[] escolhas; // Opções de resposta do jogador
}

public class NPCInteraction : MonoBehaviour
{
    // Variável global para impedir que o jogador faça outras ações enquanto fala
    public static bool isPlayerTalking = false;

    // --- SETTINGS ---

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

    // --- COMPONENTES E ESTADOS INTERNOS ---

    private Animator anim;
    private AudioSource audioSource;
    private NPCController npcController;

    private bool playerInRange = false;
    private bool isTyping = false; // Controla se o texto está a ser digitado letra a letra

    // Máquina de estados do diálogo:
    // 0 = Inativo | 1 = A ler frases | 2 = A aguardar clique num botão
    private int dialogueState = 0;

    private int currentNodeIndex = 0;
    private int currentLineIndex = 0;

    void Start()
    {
        // Inicialização de componentes
        npcController = GetComponent<NPCController>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Associações de cliques aos botões (se houver pelo menos 3 na lista)
        if (playerButtons.Length >= 3)
        {
            playerButtons[0].onClick.AddListener(() => EscolheuOpcao(0));
            playerButtons[1].onClick.AddListener(() => EscolheuOpcao(1));
            playerButtons[2].onClick.AddListener(() => EscolheuOpcao(2));
        }

        CloseAllUI();
    }

    void Update()
    {
        // Se o jogo estiver em pausa, o NPC ignora tudo o resto
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return;

        if (npcController == null || !npcController.isActiveInWorld || npcController.isFading)
        {
            CloseAllUI();
            return;
        }

        // Se o NPC não existir, estiver invisível ou a fazer fade, aborta qualquer lógica de UI
        if (npcController == null || !npcController.isActiveInWorld || npcController.isFading)
        {
            CloseAllUI();
            return;
        }

        // Verifica se o NPC chegou ao ponto em que está disponível para falar
        bool isReadyToTalk = npcController.isWaitingForInteraction;

        // Se o jogador estiver na área e o NPC estiver pronto:
        if (playerInRange && isReadyToTalk)
        {
            // Ativa o ícone de interação se o diálogo não tiver começado
            if (dialogueState == 0 && interactionIcon != null && !interactionIcon.activeSelf)
                interactionIcon.SetActive(true);

            // Deteta o input do jogador
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
                        // "Skip" do efeito de digitação: mostra a frase toda de imediato
                        StopAllCoroutines();
                        FinishTyping(dialogueNodes[currentNodeIndex]);
                    }
                    else
                    {
                        // Avança para a próxima frase
                        NextLine();
                    }
                }
            }
        }
        else
        {
            // Desativa o ícone de interação se as condições não forem cumpridas
            if (dialogueState == 0 && interactionIcon != null && interactionIcon.activeSelf)
                interactionIcon.SetActive(false);
        }
    }

    private void StartDialogue()
    {
        // Prevenção de erros caso não existam nós configurados
        if (dialogueNodes == null || dialogueNodes.Length == 0) return;

        isPlayerTalking = true;

        // Desbloqueia e mostra o rato para o jogador conseguir escolher as opções
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        dialogueState = 1;
        currentNodeIndex = 0;
        currentLineIndex = 0;

        // Ativa a UI principal
        if (talkGUI != null) talkGUI.SetActive(true);
        if (clientNameText != null) clientNameText.text = npcName;

        EsconderBotoes();
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        // Esconde o ícone de interação enquanto a frase está a ser lida
        if (interactionIcon != null) interactionIcon.SetActive(false);

        DialogueNode currentNode = dialogueNodes[currentNodeIndex];
        string lineToType = currentNode.frasesDoNPC[currentLineIndex];

        StartCoroutine(TypeLine(lineToType, currentNode));
    }

    // Corrotina para o efeito "máquina de escrever"
    private IEnumerator TypeLine(string line, DialogueNode currentNode)
    {
        isTyping = true;
        clientTalkText.text = "";

        if (anim != null) anim.SetBool("isTalking", true);

        // Imprime o texto letra a letra
        foreach (char c in line.ToCharArray())
        {
            clientTalkText.text += c;

            // Reproduz som de digitação, ignorando os espaços vazios
            if (typingSound != null && audioSource != null && c != ' ')
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(typingSound, 0.5f);
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        FinishTyping(currentNode);
    }

    // Chamada no final de cada frase, natural ou forçada via "skip"
    private void FinishTyping(DialogueNode currentNode)
    {
        isTyping = false;

        // Garante que o texto fica integralmente visível
        if (clientTalkText != null)
            clientTalkText.text = currentNode.frasesDoNPC[currentLineIndex];

        if (anim != null) anim.SetBool("isTalking", false);

        // Se for a última linha deste nó (node)
        if (currentLineIndex == currentNode.frasesDoNPC.Length - 1)
        {
            // Verifica se tem escolhas associadas e exibe-as
            if (currentNode.escolhas != null && currentNode.escolhas.Length > 0)
            {
                MostrarBotoesDeEscolha(currentNode);
            }
            else if (interactionIcon != null)
            {
                // Mostra o ícone para indicar ao jogador que pode encerrar o diálogo
                interactionIcon.SetActive(true);
            }
        }
        else
        {
            // Mostra o ícone para indicar ao jogador que pode avançar de frase
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
    }

    private void NextLine()
    {
        DialogueNode currentNode = dialogueNodes[currentNodeIndex];

        // Se existirem mais frases, avança o índice
        if (currentLineIndex < currentNode.frasesDoNPC.Length - 1)
        {
            currentLineIndex++;
            ShowCurrentLine();
        }
        else
        {
            // Se as frases acabarem, fecha o diálogo
            EndDialogue();
        }
    }

    private void MostrarBotoesDeEscolha(DialogueNode node)
    {
        dialogueState = 2; // Estado de espera pelas escolhas do jogador
        if (interactionIcon != null) interactionIcon.SetActive(false);

        EsconderBotoes();

        int numberOfChoices = Mathf.Min(node.escolhas.Length, playerButtons.Length);

        // Liga os botões necessários baseados no número de escolhas definidos no node
        for (int i = 0; i < numberOfChoices; i++)
        {
            playerButtons[i].gameObject.SetActive(true);
            if (playerButtonsTexts[i] != null)
                playerButtonsTexts[i].text = node.escolhas[i].textoDoBotao;
        }
    }

    public void EscolheuOpcao(int indexDoBotao)
    {
        DialogueNode currentNode = dialogueNodes[currentNodeIndex];
        int proximoNode = currentNode.escolhas[indexDoBotao].proximoNode;

        // Se a escolha levar ao fim da árvore (-1) ou a um nó inexistente
        if (proximoNode == -1 || proximoNode >= dialogueNodes.Length)
        {
            EndDialogue();
        }
        else
        {
            // Atualiza para o novo nó selecionado
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
        // Liberta o NPC para continuar a sua rotina
        if (npcController != null) npcController.ResumeMovement();
    }

    // --- FUNÇÕES PÚBLICAS DE COLISÃO ---
    // Agora são chamadas externamente através do script DialogueTriggerZone, que se encontra num objeto filho

    public void OnPlayerEnter()
    {
        playerInRange = true;
    }

    public void OnPlayerExit()
    {
        playerInRange = false;
        // Se o diálogo estiver aberto e o jogador sair do raio, fecha à força a interação
        if (dialogueState != 0) CloseAllUI();
    }

    private void CloseAllUI()
    {
        // Reset geral das variáveis
        dialogueState = 0;
        isTyping = false;
        StopAllCoroutines();

        if (anim != null) anim.SetBool("isTalking", false);

        isPlayerTalking = false;

        // Bloqueia e esconde o rato de volta (Gameplay normal)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Desliga componentes visuais
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (talkGUI != null) talkGUI.SetActive(false);
        EsconderBotoes();
    }
}