using UnityEngine;

public class ExitDoorInteractable : MonoBehaviour
{
    [Header("Configurações da Porta")]
    public float timeToUnlock = 3f;
    public GameObject interactionIcon;

    [Header("Progress Bar Settings")]
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    [Header("Objetos da Porta a Desbloquear")]
    public AutomatedDoorSide[] doorScripts;
    public GameObject doorCollisionBlocker;

    public static bool isUnlockingDoor = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;
    private bool isDoorUnlocked = false;

    private void Start()
    {
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(false);

        if (progressBarRenderer != null)
        {
            progressMaterial = progressBarRenderer.material;
            SetProgress(0f);
        }

        LockDoor();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && KeyInteractable.hasKey && !isDoorUnlocked)
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
        if (inRange && KeyInteractable.hasKey && !isDoorUnlocked)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    isInteracting = true;
                    isUnlockingDoor = true;
                    if (interactionIcon != null) interactionIcon.SetActive(false);
                    if (progressBarObj != null) progressBarObj.SetActive(true);
                }

                holdTimer += Time.deltaTime;
                SetProgress((holdTimer / timeToUnlock) * 100f);

                if (holdTimer >= timeToUnlock)
                {
                    UnlockDoor();
                }
            }
            else if (isInteracting)
            {
                ResetInteraction();
            }
        }
    }

    private void UnlockDoor()
    {
        isInteracting = false;
        isUnlockingDoor = false;
        isDoorUnlocked = true;

        if (progressBarObj != null) progressBarObj.SetActive(false);
        if (interactionIcon != null) interactionIcon.SetActive(false);

        foreach (var script in doorScripts)
        {
            if (script != null) script.isLockedByAKey = false;
        }

        if (doorCollisionBlocker != null) doorCollisionBlocker.SetActive(false);

        gameObject.SetActive(false);
    }

    private void LockDoor()
    {
        isDoorUnlocked = false;
        gameObject.SetActive(true);

        foreach (var script in doorScripts)
        {
            if (script != null) script.isLockedByAKey = true;
        }

        if (doorCollisionBlocker != null) doorCollisionBlocker.SetActive(true);
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        isUnlockingDoor = false;
        holdTimer = 0f;
        SetProgress(0f);

        if (progressBarObj != null) progressBarObj.SetActive(false);
        if (inRange && KeyInteractable.hasKey && !isDoorUnlocked && interactionIcon != null)
            interactionIcon.SetActive(true);
    }

    // Chamado pelo PlayerCatchTrigger quando o jogador morre
    public void ResetDoorState()
    {
        LockDoor();
        inRange = false; // Finge que não está perto
        if (interactionIcon != null) interactionIcon.SetActive(false); // Força ícone a apagar
        ResetInteraction();
    }

    private void SetProgress(float value)
    {
        if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, value);
    }
}