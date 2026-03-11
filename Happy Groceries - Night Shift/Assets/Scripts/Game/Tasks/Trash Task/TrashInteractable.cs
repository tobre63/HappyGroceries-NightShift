using UnityEngine;

public class TrashInteractable : MonoBehaviour
{
    public enum InteractionType
    {
        Pickup, // Lixeira de dentro
        Dispose // Lixeira de fora
    }

    [Header("Configuration")]
    public InteractionType interactionType;
    public float timeToInteract = 2f;

    [Header("Visuals (Inside Trash Only)")]
    public Sprite fullSprite;
    public Sprite emptySprite;

    [Header("UI References")]
    public GameObject interactionIcon;
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;

    private bool isPlayerInRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Rigidbody2D playerRb;
    private Material progressMaterial;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (interactionType == InteractionType.Pickup && fullSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = fullSprite;
        }

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(false);
        if (progressBarRenderer != null) progressMaterial = progressBarRenderer.material;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerRb = collision.GetComponent<Rigidbody2D>();
            CheckInteractionConditions();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerRb = null;
            ResetInteraction();
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        if (!CanInteract())
        {
            if (interactionIcon != null) interactionIcon.SetActive(false);
            ResetInteraction();
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (!isInteracting) StartInteraction();

            holdTimer += Time.deltaTime;
            UpdateProgressVisual((holdTimer / timeToInteract) * 100f);

            if (holdTimer >= timeToInteract) CompleteTask();
        }
        else
        {
            if (isInteracting) ResetInteraction();
        }
    }

    private bool CanInteract()
    {
        if (TrashEventController.instance == null || !TrashEventController.instance.isQuestActive)
            return false;

        switch (interactionType)
        {
            case InteractionType.Pickup:
                // MUDANÇA IMPORTANTE:
                // Agora podemos pegar lixo mesmo que TaskManager.hasTrash seja true.
                // A única restrição é se ESTA lixeira visualmente ainda está cheia.
                bool isThisCanFull = (spriteRenderer != null && spriteRenderer.sprite == fullSprite);
                return isThisCanFull;

            case InteractionType.Dispose:
                // Só pode jogar fora se tiver lixo NA MÃO 
                // E se já tiver pego TODAS as lixeiras internas
                bool hasTrash = TaskManager.instance.hasTrash;
                bool allCollected = TrashEventController.instance.AreAllTrashCansCollected();
                return hasTrash && allCollected;
        }
        return false;
    }

    private void StartInteraction()
    {
        isInteracting = true;
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
    }

    private void CompleteTask()
    {
        if (interactionType == InteractionType.Pickup)
        {
            // Marca que o jogador tem lixo (se pegar 1 ou 10 sacos, continua "tendo lixo")
            TaskManager.instance.hasTrash = true;

            // Esvazia visualmente ESTA lixeira
            if (spriteRenderer != null && emptySprite != null)
                spriteRenderer.sprite = emptySprite;

            TrashEventController.instance.OnTrashPickedUp();
        }
        else if (interactionType == InteractionType.Dispose)
        {
            // O jogador se livra do lixo
            TaskManager.instance.hasTrash = false;
            TrashEventController.instance.OnTrashDisposed();
        }

        ResetInteraction();
        CheckInteractionConditions();
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        holdTimer = 0f;
        UpdateProgressVisual(0f);
        if (progressBarObj != null) progressBarObj.SetActive(false);
        CheckInteractionConditions();
    }

    private void CheckInteractionConditions()
    {
        if (isPlayerInRange && CanInteract())
        {
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
        else
        {
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }

    private void UpdateProgressVisual(float value)
    {
        if (progressMaterial != null && progressMaterial.HasProperty("_Percentage"))
            progressMaterial.SetFloat("_Percentage", value);
    }
}