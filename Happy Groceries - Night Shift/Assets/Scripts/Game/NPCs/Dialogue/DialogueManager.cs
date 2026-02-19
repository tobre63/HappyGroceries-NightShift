using UnityEngine;
using TMPro; // Necessário para acessar os textos da UI

[RequireComponent(typeof(Collider2D))]
public class DialogueManager : MonoBehaviour, IInteractable
{
    [Header("Objeto Pai da UI")]
    [Tooltip("Arraste o objeto 'Talk' inteiro aqui")]
    public GameObject talkParent;

    [Header("Textos do NPC")]
    [Tooltip("Arraste o objeto 'ClientName' (deve ter um componente TextMeshProUGUI)")]
    public TextMeshProUGUI clientNameText;

    [Tooltip("Arraste o objeto 'ClientTalk' (deve ter um componente TextMeshProUGUI)")]
    public TextMeshProUGUI clientTalkText;

    [Header("Opções do Jogador")]
    [Tooltip("Arraste o objeto 'PlayerHostile'")]
    public GameObject playerHostile;
    [Tooltip("Arraste o objeto 'PlayerKind'")]
    public GameObject playerKind;
    [Tooltip("Arraste o objeto 'PlayerAfraid'")]
    public GameObject playerAfraid;

    [Header("Dados do Diálogo")]
    [Tooltip("Arraste aqui o seu ScriptableObject NPCDialogue com as falas deste NPC")]
    public NPCDialogue dialogueData;

    void Start()
    {
        // Garante que o painel de diálogo comece oculto na tela
        if (talkParent != null)
        {
            talkParent.SetActive(false);
        }
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        Debug.Log("NPC Recebeu o comando de Interact!");

        if (talkParent != null)
        {
            // Alterna o estado (se estiver fechado, abre. Se estiver aberto, fecha)
            bool isOpening = !talkParent.activeSelf;
            talkParent.SetActive(isOpening);

            if (isOpening)
            {
                SetupDialogueUI();
            }
        }
        else
        {
            Debug.LogError("ERRO: O objeto 'Talk' não foi atribuído no Inspector do NPC!");
        }
    }

    private void SetupDialogueUI()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("AVISO: Nenhum ScriptableObject (NPCDialogue) atribuído neste NPC!");
            return;
        }

        // 1. Define o Nome do NPC na tela
        if (clientNameText != null)
        {
            clientNameText.text = dialogueData.npcName;
        }

        // 2. Define a primeira fala do NPC
        if (clientTalkText != null && dialogueData.dialogueLines.Length > 0)
        {
            // Pega a primeira linha de diálogo do seu ScriptableObject
            clientTalkText.text = dialogueData.dialogueLines[0];
        }

        // 3. Garante que as três opções de resposta do jogador apareçam
        if (playerHostile != null) playerHostile.SetActive(true);
        if (playerKind != null) playerKind.SetActive(true);
        if (playerAfraid != null) playerAfraid.SetActive(true);
    }
}