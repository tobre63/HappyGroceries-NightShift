using UnityEngine;

public class ClipboardInteraction : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Arraste aqui o Canvas (ou Painel) que contém o botão 'E'")]
    public GameObject eButtonPrompt;

    [Tooltip("Arraste aqui o menu/imagem do clipboard que deve abrir ao clicar")]
    public GameObject clipboardMenu;

    [Header("Interaction Settings")]
    [Tooltip("Tecla para interagir")]
    public KeyCode interactionKey = KeyCode.E;

    [Tooltip("Fecha o menu automaticamente quando o player sai?")]
    public bool closeMenuOnExit = true;

    [Header("Fade Settings")]
    [Tooltip("Usar efeito de fade in/out?")]
    public bool useFade = true;

    [Tooltip("Velocidade do fade (maior = mais rápido)")]
    public float fadeSpeed = 5f;

    private bool isPlayerInRange;
    private CanvasGroup promptCanvasGroup;
    private float targetAlpha = 0f;

    void Start()
    {
        // Começa com tudo escondido
        if (clipboardMenu != null)
        {
            clipboardMenu.SetActive(false);
        }

        // Configura o CanvasGroup para o fade
        if (eButtonPrompt != null)
        {
            // Tenta obter o CanvasGroup existente
            promptCanvasGroup = eButtonPrompt.GetComponent<CanvasGroup>();

            // Se não existir, adiciona um
            if (promptCanvasGroup == null && useFade)
            {
                promptCanvasGroup = eButtonPrompt.AddComponent<CanvasGroup>();
            }

            // Começa invisível
            if (promptCanvasGroup != null)
            {
                promptCanvasGroup.alpha = 0f;
            }

            // Ativa o objeto (o fade controla a visibilidade)
            if (useFade)
            {
                eButtonPrompt.SetActive(true);
            }
            else
            {
                eButtonPrompt.SetActive(false);
            }
        }
    }

    void Update()
    {
        // Atualiza o fade
        if (useFade && promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = Mathf.Lerp(promptCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }

        // Interação com tecla
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            ToggleMenu();
        }

        // Fechar com ESC
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

            // Esconde o prompt quando o menu abre
            HidePrompt();

            // OPCIONAL: Pausar o jogo
            // if (GameManager.Instance != null) GameManager.Instance.PauseGame();
        }
    }

    private void CloseMenu()
    {
        if (clipboardMenu != null)
        {
            clipboardMenu.SetActive(false);

            // Volta a mostrar o prompt se o player ainda estiver perto
            if (isPlayerInRange)
            {
                ShowPrompt();
            }

            // OPCIONAL: Despausar
            // if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
        }
    }

    private void ShowPrompt()
    {
        if (eButtonPrompt == null) return;

        if (useFade)
        {
            targetAlpha = 1f;
        }
        else
        {
            eButtonPrompt.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (eButtonPrompt == null) return;

        if (useFade)
        {
            targetAlpha = 0f;
        }
        else
        {
            eButtonPrompt.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Só mostra o prompt se o menu não estiver aberto
            if (clipboardMenu == null || !clipboardMenu.activeSelf)
            {
                ShowPrompt();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Esconde o prompt
            HidePrompt();

            // Fecha o menu se a opção estiver ativa
            if (closeMenuOnExit && clipboardMenu != null)
            {
                clipboardMenu.SetActive(false);
            }
        }
    }
}