using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Classes de dados para estruturar a árvore de diálogo no Inspector
[System.Serializable]
public class DialogueChoice
{
    public string textoDoBotao;
    public int proximoNode = -1; // -1 significa que a conversa termina após esta escolha
}

[System.Serializable]
public class DialogueNode
{
    [TextArea(2, 4)]
    public string[] frasesDoNPC; // Array com as várias frases que o NPC diz antes das opções
    public DialogueChoice[] escolhas; // Opções que o jogador tem no final destas frases
}

public class NPCInteraction : MonoBehaviour
{
    // Variável estática global (pode ser lida por outros scripts para saber se o jogador está em diálogo)
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
    public float typingSpeed = 0.04f;   // Velocidade do efeito "máquina de escrever"
    public AudioClip typingSound;       // O som de 'blip' tocado a cada letra

    // --- COMPONENTES E CONTROLOS INTERNOS ---

    private Animator anim;
    private AudioSource audioSource;
    private NPCController npcController;

    private bool playerInRange = false;
    private bool isTyping = false; // Controla se o texto ainda está a aparecer letra a letra

    // Máquina de estados simples do diálogo: 
    // 0 = Fechado/Inativo | 1 = A ler frases (texto a correr ou à espera de avançar) | 2 = À espera que o jogador clique num botão
    private int dialogueState = 0;

    private int currentNodeIndex = 0;
    private int currentLineIndex = 0;

    void Start()
    {
        // Vai buscar os componentes ligados ao NPC
        npcController = GetComponent<NPCController>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Configura automaticamente os cliques dos botões se houver pelo menos 3 associados no Inspector
        if (playerButtons.Length >= 3)
        {
            playerButtons[0].onClick.AddListener(() => EscolheuOpcao(0));
            playerButtons[1].onClick.AddListener(() => EscolheuOpcao(1));
            playerButtons[2].onClick.AddListener(() => EscolheuOpcao(2));
        }

        // Garante que toda a UI começa desligada
        CloseAllUI();
    }

    void Update()
    {
        // Se o NPC não existir, não estiver ativo no mundo ou estiver a fazer fade, garante que a UI fecha
        if (npcController == null || !npcController.isActiveInWorld || npcController.isFading)
        {
            CloseAllUI();
            return;
        }

        // Verifica se o NPC chegou ao waypoint onde deve esperar pela interação do jogador
        bool isReadyToTalk = npcController.isWaitingForInteraction;

        // Se o jogador estiver na área e o NPC estiver pronto para falar
        if (playerInRange && isReadyToTalk)
        {
            // Mostra o ícone de interação (ex: a tecla 'E') se o diálogo não tiver começado
            if (dialogueState == 0 && interactionIcon != null && !interactionIcon.activeSelf)
                interactionIcon.SetActive(true);

            // Deteta o input do jogador para interagir
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (dialogueState == 0)
                {
                    // Inicia o diálogo caso esteja fechado
                    StartDialogue();
                }
                else if (dialogueState == 1)
                {
                    // Se o diálogo já está a decorrer...
                    if (isTyping)
                    {
                        // Se o texto está a ser escrito, interrompe a corrotina e mostra a frase inteira de uma vez (Skip)
                        StopAllCoroutines();
                        FinishTyping(dialogueNodes[currentNodeIndex]);
                    }
                    else
                    {
                        // Se a frase já estava toda no ecrã, avança para a próxima
                        NextLine();
                    }
                }
            }
        }
        else
        {
            // Esconde o ícone de interação se o jogador sair da área ou o NPC ainda não estiver pronto
            if (dialogueState == 0 && interactionIcon != null && interactionIcon.activeSelf)
                interactionIcon.SetActive(false);
        }
    }

    private void StartDialogue()
    {
        // Previne erros se não houver nós de diálogo configurados
        if (dialogueNodes == null || dialogueNodes.Length == 0) return;

        isPlayerTalking = true;

        // Liberta o cursor para que o jogador consiga clicar nas opções
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        dialogueState = 1;
        currentNodeIndex = 0;
        currentLineIndex = 0;

        // Ativa a interface principal de conversa e define o nome do NPC
        if (talkGUI != null) talkGUI.SetActive(true);
        if (clientNameText != null) clientNameText.text = npcName;

        EsconderBotoes();
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        // Esconde o ícone 'E' enquanto o texto está a ser escrito
        if (interactionIcon != null) interactionIcon.SetActive(false);

        DialogueNode currentNode = dialogueNodes[currentNodeIndex];
        string lineToType = currentNode.frasesDoNPC[currentLineIndex];

        // Inicia a corrotina para fazer o texto aparecer letra a letra
        StartCoroutine(TypeLine(lineToType, currentNode));
    }

    // Corrotina que gera o efeito de "máquina de escrever"
    private IEnumerator TypeLine(string line, DialogueNode currentNode)
    {
        isTyping = true;
        clientTalkText.text = "";

        // Ativa a animação de falar do NPC
        if (anim != null) anim.SetBool("isTalking", true);

        // Percorre cada caractere da frase
        foreach (char c in line.ToCharArray())
        {
            clientTalkText.text += c;

            // Toca som a cada letra (ignorando espaços em branco para um som mais natural)
            if (typingSound != null && audioSource != null && c != ' ')
            {
                // Pequena variação (pitch) para o som não ser repetitivo e mecânico
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(typingSound, 0.5f);
            }

            // Espera o tempo definido antes de mostrar a próxima letra
            yield return new WaitForSeconds(typingSpeed);
        }

        // Quando o ciclo terminar, finaliza o processo
        FinishTyping(currentNode);
    }

    // Chamada automaticamente quando o texto acaba de ser escrito, ou forçada quando o jogador faz "skip"
    private void FinishTyping(DialogueNode currentNode)
    {
        isTyping = false;

        // Garante que o texto fica 100% visível (útil para quando há skip)
        if (clientTalkText != null)
            clientTalkText.text = currentNode.frasesDoNPC[currentLineIndex];

        // Desliga a animação de falar do NPC
        if (anim != null) anim.SetBool("isTalking", false);

        // Verifica se chegámos à última frase do Node atual
        if (currentLineIndex == currentNode.frasesDoNPC.Length - 1)
        {
            // Se existirem escolhas, mostra os botões
            if (currentNode.escolhas != null && currentNode.escolhas.Length > 0)
            {
                MostrarBotoesDeEscolha(currentNode);
            }
            else
            {
                // Se não houver escolhas, mostra o ícone de interação para finalizar a conversa
                if (interactionIcon != null) interactionIcon.SetActive(true);
            }
        }
        else
        {
            // Se ainda houver mais frases neste Node, mostra o 'E' para o jogador avançar
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
    }

    private void NextLine()
    {
        DialogueNode currentNode = dialogueNodes[currentNodeIndex];

        // Se ainda não estivermos na última frase deste Node, avança no índice e mostra-a
        if (currentLineIndex < currentNode.frasesDoNPC.Length - 1)
        {
            currentLineIndex++;
            ShowCurrentLine();
        }
        else
        {
            // Se as frases acabaram e não haviam opções, fecha a conversa
            EndDialogue();
        }
    }

    private void MostrarBotoesDeEscolha(DialogueNode node)
    {
        dialogueState = 2; // Passa ao estado de "esperar que clique num botão"
        if (interactionIcon != null) interactionIcon.SetActive(false); // Esconde o 'E'

        EsconderBotoes();

        // Determina quantos botões vão ser ligados (limite máximo igual ao número de botões disponíveis na UI)
        int numberOfChoices = Mathf.Min(node.escolhas.Length, playerButtons.Length);

        // Ativa os botões necessários e atribui os respetivos textos
        for (int i = 0; i < numberOfChoices; i++)
        {
            playerButtons[i].gameObject.SetActive(true);
            if (playerButtonsTexts[i] != null)
            {
                playerButtonsTexts[i].text = node.escolhas[i].textoDoBotao;
            }
        }
    }

    // Função que é executada pelos botões de escolha na UI
    public void EscolheuOpcao(int indexDoBotao)
    {
        DialogueNode currentNode = dialogueNodes[currentNodeIndex];
        int proximoNode = currentNode.escolhas[indexDoBotao].proximoNode;

        // Se o próximo Node for -1, ou se for inválido, o diálogo termina
        if (proximoNode == -1 || proximoNode >= dialogueNodes.Length)
        {
            EndDialogue();
        }
        else
        {
            // Caso contrário, salta para o Node indicado e começa a ler a primeira frase dele
            currentNodeIndex = proximoNode;
            currentLineIndex = 0;
            dialogueState = 1; // Volta ao estado "a ler frases"

            EsconderBotoes();
            ShowCurrentLine();
        }
    }

    private void EsconderBotoes()
    {
        // Desativa todos os botões de escolha
        foreach (Button btn in playerButtons)
        {
            if (btn != null) btn.gameObject.SetActive(false);
        }
    }

    private void EndDialogue()
    {
        // Encerra a UI
        CloseAllUI();

        // Avisa o NPCController que o diálogo acabou e ele já pode retomar o seu caminho
        if (npcController != null) npcController.ResumeMovement();
    }

    // Deteta se o jogador entra na área de alcance para falar
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) playerInRange = true;
    }

    // Deteta se o jogador sai da área de alcance (cancela/encerra a conversa se estiver a decorrer)
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (dialogueState != 0) CloseAllUI();
        }
    }

    private void CloseAllUI()
    {
        // Faz reset a todas as variáveis de estado do diálogo
        dialogueState = 0;
        isTyping = false;
        StopAllCoroutines(); // Para a corrotina de TypeLine caso estivesse a decorrer

        if (anim != null) anim.SetBool("isTalking", false);

        isPlayerTalking = false;

        // Esconde e tranca novamente o cursor do rato
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Desliga os elementos visuais
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (talkGUI != null) talkGUI.SetActive(false);
        EsconderBotoes();
    }
}