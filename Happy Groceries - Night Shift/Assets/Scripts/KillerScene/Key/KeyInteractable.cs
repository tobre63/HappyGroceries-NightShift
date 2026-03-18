using UnityEngine;

public class KeyInteractable : MonoBehaviour
{
    [Header("Configurações da Chave")]
    public float timeToPickUp = 5f;
    public GameObject interactionIcon;

    [Header("Progress Bar Settings")]
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    public static bool hasKey = false;
    public static bool isInteractingWithKey = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;

    private void Start()
    {
        hasKey = false;
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
        if (collision.CompareTag("Player") && !hasKey)
        {
            inRange = true;
            if (interactionIcon != null && !isInteracting) interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
            if (interactionIcon != null) interactionIcon.SetActive(false); // CORREÇÃO BUG: Força ícone a apagar
            ResetInteraction();
        }
    }

    private void Update()
    {
        if (inRange && !hasKey)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    isInteracting = true;
                    isInteractingWithKey = true;
                    if (interactionIcon != null) interactionIcon.SetActive(false);
                    if (progressBarObj != null) progressBarObj.SetActive(true);
                }

                holdTimer += Time.deltaTime;
                SetProgress((holdTimer / timeToPickUp) * 100f);

                if (holdTimer >= timeToPickUp)
                {
                    FinishPickUp();
                }
            }
            else if (isInteracting)
            {
                ResetInteraction();
            }
        }
    }

    private void FinishPickUp()
    {
        isInteracting = false;
        isInteractingWithKey = false;
        hasKey = true;

        if (progressBarObj != null) progressBarObj.SetActive(false);
        if (interactionIcon != null) interactionIcon.SetActive(false);
        gameObject.SetActive(false);
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        isInteractingWithKey = false;
        holdTimer = 0f;
        SetProgress(0f);

        if (progressBarObj != null) progressBarObj.SetActive(false);
        // Só mostra o ícone se o jogador ainda estiver na zona e não tiver a chave
        if (inRange && !hasKey && interactionIcon != null) interactionIcon.SetActive(true);
    }

    // Chamado pelo PlayerCatchTrigger quando o jogador morre
    public void ResetKey()
    {
        hasKey = false;
        gameObject.SetActive(true);

        inRange = false; // Finge que o jogador não está perto quando recomeça
        if (interactionIcon != null) interactionIcon.SetActive(false); // Força a desaparecer o ícone no Game Over

        ResetInteraction();
    }

    private void SetProgress(float value)
    {
        if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, value);
    }
}