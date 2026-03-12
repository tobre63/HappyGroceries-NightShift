using UnityEngine;

public class TrashInteractable : MonoBehaviour
{
    public enum InteractionType
    {
        Pickup, // Lixeira de dentro
        Dispose // Lixeira de fora (Dumpster)
    }

    // Variável global para dizer ao PlayerController que estamos a bloquear o movimento
    public static bool isInteractingWithTrash = false;

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

        // NOVO: Se o evento do assassino já disparou, congelamos tudo e saímos. A barra e o jogador ficam como estão.
        if (TrashEventController.instance != null && TrashEventController.instance.isKillerEventActive)
            return;

        if (!CanInteract())
        {
            if (interactionIcon != null) interactionIcon.SetActive(false);
            if (isInteracting) ResetInteraction();
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (!isInteracting) StartInteraction();

            holdTimer += Time.deltaTime;
            UpdateProgressVisual((holdTimer / timeToInteract) * 100f);

            // EVENTO DO ASSASSINO
            if (interactionType == InteractionType.Dispose &&
                TrashEventController.instance.trashDisposedCount == 1 &&
                holdTimer >= timeToInteract * 0.5f)
            {
                // Dispara o assassino. REMOVEMOS o "ResetInteraction()" para a UI não desaparecer.
                TrashEventController.instance.SpawnKillerAndChase(playerRb.transform);
                return;
            }

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
                // Só pode pegar se estiver cheia e se o jogador NÃO tiver lixo na mão
                bool isThisCanFull = (spriteRenderer != null && spriteRenderer.sprite == fullSprite);
                return isThisCanFull && !TaskManager.instance.hasTrash;

            case InteractionType.Dispose:
                // Só pode jogar no dumpster se TIVER lixo na mão
                return TaskManager.instance.hasTrash;
        }
        return false;
    }

    private void StartInteraction()
    {
        isInteracting = true;
        isInteractingWithTrash = true; // Bloqueia o player

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
    }

    private void CompleteTask()
    {
        if (interactionType == InteractionType.Pickup)
        {
            TaskManager.instance.hasTrash = true;

            if (spriteRenderer != null && emptySprite != null)
                spriteRenderer.sprite = emptySprite;

            TrashEventController.instance.OnTrashPickedUp();
        }
        else if (interactionType == InteractionType.Dispose)
        {
            TaskManager.instance.hasTrash = false;
            TrashEventController.instance.OnTrashDisposed();
        }

        ResetInteraction();
        CheckInteractionConditions();
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        isInteractingWithTrash = false; // Desbloqueia o player
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