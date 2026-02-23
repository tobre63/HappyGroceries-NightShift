using UnityEngine;

public class ShelfInteractable : MonoBehaviour
{
    [Header("Shelf Settings")]
    public int acceptedBoxID;
    public float placeInterval = 0.5f;
    public GameObject interactionIcon;

    [Header("Shelf Items")]
    // Array para guardares os itens/sprites que já são filhos desta prateleira
    public GameObject[] shelfItems;

    private bool inRange = false;
    private bool isInteracting = false;
    private float placeTimer = 0f;
    private int itemsPlaced = 0;
    private bool isCompleted = false;

    private void Start()
    {
        interactionIcon.SetActive(false);

        // Por segurança, garantimos que todos os itens começam desativados
        foreach (GameObject item in shelfItems)
        {
            item.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCompleted) return;

        if (collision.CompareTag("Player"))
        {
            inRange = true;

            if (TaskManager.instance.currentBoxHeldID == acceptedBoxID)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
            interactionIcon.SetActive(false);
            ResetCurrentItemInteraction();
        }
    }

    private void Update()
    {
        if (isCompleted || !inRange) return;

        if (TaskManager.instance.currentBoxHeldID == acceptedBoxID)
        {
            if (!interactionIcon.activeSelf) interactionIcon.SetActive(true);

            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    isInteracting = true;
                    TaskManager.instance.onInteractionStart.Invoke();
                }

                placeTimer += Time.deltaTime;

                if (placeTimer >= placeInterval)
                {
                    // Ativa o GameObject atual na hierarquia, correspondente ao progresso
                    if (itemsPlaced < shelfItems.Length)
                    {
                        shelfItems[itemsPlaced].SetActive(true);
                    }

                    itemsPlaced++;
                    placeTimer = 0f;

                    // Verifica se já ativou todos os itens do array
                    if (itemsPlaced >= shelfItems.Length)
                    {
                        isCompleted = true;
                        isInteracting = false;

                        TaskManager.instance.currentBoxHeldID = 0;
                        TaskManager.instance.onInteractionStop.Invoke();

                        interactionIcon.SetActive(false);
                    }
                }
            }
            else
            {
                if (isInteracting)
                {
                    ResetCurrentItemInteraction();
                }
            }
        }
        else
        {
            if (interactionIcon.activeSelf) interactionIcon.SetActive(false);
        }
    }

    private void ResetCurrentItemInteraction()
    {
        if (isInteracting)
        {
            isInteracting = false;
            placeTimer = 0f;
            TaskManager.instance.onInteractionStop.Invoke();
        }
    }
}