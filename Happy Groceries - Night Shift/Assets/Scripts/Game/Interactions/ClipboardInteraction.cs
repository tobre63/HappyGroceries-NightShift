using UnityEngine;

public class ClipboardInteraction : MonoBehaviour
{
    // --- SETTINGS ---

    [Header("UI References")]
    public GameObject eButtonPrompt;
    public GameObject clipboardMenu;

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public bool closeMenuOnExit = true;

    // --- VARIÁVEIS INTERNAS ---

    private bool isPlayerInRange;

    void Start()
    {
        // Garante que a UI começa toda escondida para evitar bugs visuais ao iniciar a cena
        if (eButtonPrompt != null) eButtonPrompt.SetActive(false);
        if (clipboardMenu != null) clipboardMenu.SetActive(false);
    }

    void Update()
    {
        // Se o jogador estiver na área de alcance e pressionar a tecla de interação (ex: 'E')
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            ToggleMenu();
        }

        // Permite fechar o menu diretamente pressionando a tecla ESC
        if (clipboardMenu != null && clipboardMenu.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    private void ToggleMenu()
    {
        // Alterna o estado do menu dependendo se este já se encontra aberto ou fechado
        if (clipboardMenu != null)
        {
            bool isCurrentlyActive = clipboardMenu.activeSelf;

            if (isCurrentlyActive)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    private void OpenMenu()
    {
        if (clipboardMenu != null)
        {
            // Mostra o menu do clipboard no ecrã
            clipboardMenu.SetActive(true);

            // Esconde a indicação visual da tecla de interação para não poluir o ecrã durante a leitura
            if (eButtonPrompt != null)
            {
                eButtonPrompt.SetActive(false);
            }
        }
    }

    private void CloseMenu()
    {
        if (clipboardMenu != null)
        {
            // Esconde o menu do clipboard
            clipboardMenu.SetActive(false);

            // Volta a exibir a indicação visual da tecla caso o jogador ainda se encontre perto da mesa/objeto
            if (eButtonPrompt != null && isPlayerInRange)
            {
                eButtonPrompt.SetActive(true);
            }
        }
    }

    // --- FÍSICA E COLISÕES (2D) ---

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Valida se a colisão foi feita pelo Jogador usando a Tag e o Nome exato do GameObject
        // Esta dupla verificação ajuda a prevenir bugs com colisores invisíveis ou "filhos" do jogador (ex: áreas de ataque ou de deteção do jogador)
        if (collision.CompareTag("Player") && collision.gameObject.name == "Player")
        {
            isPlayerInRange = true;

            // Mostra o botão de interação apenas se o menu principal ainda não estiver aberto
            if (eButtonPrompt != null && (clipboardMenu == null || !clipboardMenu.activeSelf))
            {
                eButtonPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Valida novamente a saída do Jogador com a Tag e o Nome exato
        if (collision.CompareTag("Player") && collision.gameObject.name == "Player")
        {
            isPlayerInRange = false;

            // Esconde imediatamente a indicação visual da tecla de interação
            if (eButtonPrompt != null)
            {
                eButtonPrompt.SetActive(false);
            }

            // Se a opção estiver ativa, fecha o menu de forma automática quando o jogador se afasta
            if (closeMenuOnExit && clipboardMenu != null)
            {
                clipboardMenu.SetActive(false);
            }
        }
    }
}