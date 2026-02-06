using UnityEngine;

public class ClipboardInteraction : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Arraste aqui o Canvas (ou Painel) que contém o botão 'E', que deve flutuar sobre a mesa")]
    public GameObject eButtonPrompt;

    [Tooltip("Arraste aqui o menu/imagem que deve abrir ao clicar")]
    public GameObject settingsMenu;

    private bool isPlayerInRange;

    void Start()
    {
        // Começa com tudo escondido
        if (eButtonPrompt != null) eButtonPrompt.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (settingsMenu != null)
        {
            bool isActive = settingsMenu.activeSelf;
            settingsMenu.SetActive(!isActive);

            // Opcional: Se o menu abriu, esconde o botão "E". Se fechou, mostra de novo.
            if (eButtonPrompt != null)
            {
                eButtonPrompt.SetActive(isActive);
            }
        }
    }

    // --- FÍSICA 2D ---

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se foi o Player que encostou
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (eButtonPrompt != null) eButtonPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Verifica se foi o Player que saiu
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (eButtonPrompt != null) eButtonPrompt.SetActive(false);
            if (settingsMenu != null) settingsMenu.SetActive(false);
        }
    }
}