using UnityEngine;

public class ClipboardInteraction : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Arraste aqui o Canvas (ou Painel) que contém o botão 'E', que deve flutuar sobre a mesa")]
    public GameObject eButtonPrompt;

    [Tooltip("Arraste aqui o menu/imagem do clipboard que deve abrir ao clicar")]
    public GameObject clipboardMenu;

    [Header("Interaction Settings")]
    [Tooltip("Tecla para interagir")]
    public KeyCode interactionKey = KeyCode.E;

    [Tooltip("Fecha o menu automaticamente quando o player sai?")]
    public bool closeMenuOnExit = true;

    private bool isPlayerInRange;

    void Start()
    {
        // Começa com tudo escondido para evitar bugs visuais ao iniciar o jogo
        if (eButtonPrompt != null) eButtonPrompt.SetActive(false);
        if (clipboardMenu != null) clipboardMenu.SetActive(false);
    }

    void Update()
    {
        // Se o player estiver no alcance e apertar a tecla de interação
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            ToggleMenu();
        }

        // OPCIONAL: Fechar o menu com ESC
        if (clipboardMenu != null && clipboardMenu.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    private void ToggleMenu()
    {
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
            clipboardMenu.SetActive(true);

            // Esconde o prompt do botão "E" para não poluir a tela
            if (eButtonPrompt != null)
            {
                eButtonPrompt.SetActive(false);
            }

            // OPCIONAL: Pausar o jogo quando abre o clipboard
            // Time.timeScale = 0f;
            // Ou chamar o GameManager
            // if (GameManager.Instance != null) GameManager.Instance.PauseGame();
        }
    }

    private void CloseMenu()
    {
        if (clipboardMenu != null)
        {
            clipboardMenu.SetActive(false);

            // Volta a mostrar o prompt se o player ainda estiver perto
            if (eButtonPrompt != null && isPlayerInRange)
            {
                eButtonPrompt.SetActive(true);
            }

            // OPCIONAL: Despausar o jogo
            // Time.timeScale = 1f;
            // if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
        }
    }

    // --- FÍSICA 2D ---

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o objeto que entrou no Trigger tem a Tag "Player"
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Só mostra o prompt se o menu não estiver aberto
            if (eButtonPrompt != null && (clipboardMenu == null || !clipboardMenu.activeSelf))
            {
                eButtonPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Quando o Player sai de perto
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Esconde o prompt
            if (eButtonPrompt != null)
            {
                eButtonPrompt.SetActive(false);
            }

            // Fecha o menu se a opção estiver ativa
            if (closeMenuOnExit && clipboardMenu != null)
            {
                clipboardMenu.SetActive(false);
            }
        }
    }
}