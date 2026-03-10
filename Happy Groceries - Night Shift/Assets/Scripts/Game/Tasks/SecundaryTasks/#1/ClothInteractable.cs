using UnityEngine;

public class ClothInteractable : MonoBehaviour
{
    [Header("Settings")]
    public float timeToInteract = 2f;
    public GameObject interactionIcon;

    [Header("Progress Bar Settings")]
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    public static bool isInteractingWithCloth = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;
    private SpriteRenderer spriteRenderer; // Referência para esconder o visual

    private void Start()
    {
        // Pega o SpriteRenderer automaticamente do objeto atual
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(false);

        if (progressBarRenderer != null)
        {
            progressMaterial = progressBarRenderer.material;
            SetProgress(0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = true;
            CheckInteractionIcon();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
            if (interactionIcon != null) interactionIcon.SetActive(false);
            ResetInteraction();
        }
    }

    private void Update()
    {
        if (!inRange) return;

        // LÓGICA DO MOP: Verifica se pode pegar OU devolver
        bool canPickUp = !TaskManager.instance.hasCloth;

        // Só pode devolver se tiver o pano E a mesa estiver limpa
        bool canPutAway = TaskManager.instance.hasCloth && TableCleaningInteractable.isTableClean;

        if (canPickUp || canPutAway)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    StartInteraction();
                }

                holdTimer += Time.deltaTime;
                float currentPercentage = (holdTimer / timeToInteract) * 100f;
                SetProgress(currentPercentage);

                if (holdTimer >= timeToInteract)
                {
                    FinishInteraction(canPickUp, canPutAway);
                }
            }
            else
            {
                if (isInteracting)
                {
                    ResetInteraction();
                }
            }
        }
        else
        {
            // Se não pode fazer nada, garante que o ícone suma
            if (interactionIcon != null && interactionIcon.activeSelf)
                interactionIcon.SetActive(false);
        }
    }

    private void StartInteraction()
    {
        isInteracting = true;
        isInteractingWithCloth = true; // Para o player

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);
    }

    private void FinishInteraction(bool pickingUp, bool puttingAway)
    {
        isInteracting = false;
        isInteractingWithCloth = false; // Libera o player

        if (pickingUp)
        {
            // PEGAR O PANO
            TaskManager.instance.hasCloth = true;
            SetVisuals(false); // Esconde o pano, mas mantém o objeto ativo
            Debug.Log("Pano pego!");
        }
        else if (puttingAway)
        {
            // DEVOLVER O PANO
            TaskManager.instance.hasCloth = false;
            SetVisuals(true); // Mostra o pano de volta

            // Trava o objeto para não pegar de novo imediatamente (igual ao Mop)
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Debug.Log("Pano devolvido! Tarefa concluída.");
        }

        if (progressBarObj != null) progressBarObj.SetActive(false);

        // Reseta o timer para evitar loops
        holdTimer = 0f;
    }

    private void ResetInteraction()
    {
        if (isInteracting)
        {
            isInteracting = false;
            isInteractingWithCloth = false;
            holdTimer = 0f;
            SetProgress(0f);

            if (progressBarObj != null) progressBarObj.SetActive(false);

            // Verifica se deve mostrar o ícone novamente
            CheckInteractionIcon();
        }
    }

    private void CheckInteractionIcon()
    {
        if (!inRange) return;

        bool canPickUp = !TaskManager.instance.hasCloth;
        bool canPutAway = TaskManager.instance.hasCloth && TableCleaningInteractable.isTableClean;

        if (canPickUp || canPutAway)
        {
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
        else
        {
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }

    private void SetVisuals(bool active)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = active;
        }
    }

    private void SetProgress(float value)
    {
        if (progressMaterial != null)
        {
            progressMaterial.SetFloat(percentageProperty, value);
        }
    }
}