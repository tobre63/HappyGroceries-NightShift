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
    private SpriteRenderer spriteRenderer;

    private Rigidbody2D playerRb;

    private void Start()
    {
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
            playerRb = collision.GetComponent<Rigidbody2D>();
            CheckInteractionIcon();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
            playerRb = null;
            if (interactionIcon != null) interactionIcon.SetActive(false);
            ResetInteraction();
        }
    }

    private void Update()
    {
        if (!inRange) return;

        bool canPickUp = !TaskManager.instance.hasCloth;
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
            if (interactionIcon != null && interactionIcon.activeSelf)
                interactionIcon.SetActive(false);
        }
    }

    private void StartInteraction()
    {
        isInteracting = true;
        isInteractingWithCloth = true;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);
    }

    private void FinishInteraction(bool pickingUp, bool puttingAway)
    {
        isInteracting = false;
        isInteractingWithCloth = false;

        if (pickingUp)
        {
            TaskManager.instance.hasCloth = true;
            SetVisuals(false);

            // AVISA O TASK CONTROLLER
            if (ClothTaskController.instance != null)
            {
                ClothTaskController.instance.isClothPickedUp = true;
                ClothTaskController.instance.CheckProgress();
            }
        }
        else if (puttingAway)
        {
            TaskManager.instance.hasCloth = false;
            SetVisuals(true);

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // AVISA O TASK CONTROLLER
            if (ClothTaskController.instance != null)
            {
                ClothTaskController.instance.isClothPickedUp = false;
                ClothTaskController.instance.CheckProgress();
            }
        }

        if (progressBarObj != null) progressBarObj.SetActive(false);
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