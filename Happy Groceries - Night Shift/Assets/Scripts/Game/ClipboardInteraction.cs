using UnityEngine;

public class ClipboardInteraction : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Arraste aqui o objeto/imagem da tecla 'E' que deve aparecer")]
    public GameObject eButtonPrompt;

    [Tooltip("Arraste aqui o menu de Settings (ou Tasks) que deve abrir")]
    public GameObject settingsMenu;

    private bool isPlayerInRange;

    void Start()
    {
        // Garante que o prompt e o menu começam desativados
        if (eButtonPrompt != null) eButtonPrompt.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(false);
    }

    void Update()
    {
        // Se o jogador estiver na área e pressionar 'E'
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (settingsMenu != null)
        {
            // Inverte o estado atual do menu (se aberto fecha, se fechado abre)
            bool isActive = settingsMenu.activeSelf;
            settingsMenu.SetActive(!isActive);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se foi o Player que entrou na area
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (eButtonPrompt != null) eButtonPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Verifica se foi o Player que saiu da area
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (eButtonPrompt != null) eButtonPrompt.SetActive(false);

            // Opcional: Fechar o menu se o jogador se afastar demais
            if (settingsMenu != null) settingsMenu.SetActive(false);
        }
    }
}