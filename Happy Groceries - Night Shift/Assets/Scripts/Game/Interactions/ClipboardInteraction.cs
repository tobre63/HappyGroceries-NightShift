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
        if (eButtonPrompt != null) eButtonPrompt.SetActive(false);
        if (clipboardMenu != null) clipboardMenu.SetActive(false);
    }

    void Update()
    {
        // Verifica se a missão secundária está ativa
        bool isQuestActive = (ClothTaskController.instance != null && ClothTaskController.instance.isQuestActive) || (MopTaskController.instance != null && MopTaskController.instance.isQuestActive);

        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            // Bloqueia ABRIR a clipboard se a quest estiver ativa, mas permite FECHAR se já estiver aberta
            bool isCurrentlyActive = (clipboardMenu != null && clipboardMenu.activeSelf);

            if (!isQuestActive || isCurrentlyActive)
            {
                ToggleMenu();
            }
        }

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

            if (eButtonPrompt != null)
            {
                eButtonPrompt.SetActive(false);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Mudei para PUBLIC para o ClothTaskController conseguir fechar o menu
    public void CloseMenu()
    {
        if (clipboardMenu != null)
        {
            clipboardMenu.SetActive(false);

            if (eButtonPrompt != null && isPlayerInRange)
            {
                eButtonPrompt.SetActive(true);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // --- FÍSICA E COLISÕES (2D) ---

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject.name == "Player")
        {
            isPlayerInRange = true;

            if (eButtonPrompt != null && (clipboardMenu == null || !clipboardMenu.activeSelf))
            {
                // Só mostra o botão E se não houver missão ativa
                bool isQuestActive = (ClothTaskController.instance != null && ClothTaskController.instance.isQuestActive);
                if (!isQuestActive)
                {
                    eButtonPrompt.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject.name == "Player")
        {
            isPlayerInRange = false;

            if (eButtonPrompt != null)
            {
                eButtonPrompt.SetActive(false);
            }

            if (closeMenuOnExit && clipboardMenu != null)
            {
                clipboardMenu.SetActive(false);
            }
        }
    }
}