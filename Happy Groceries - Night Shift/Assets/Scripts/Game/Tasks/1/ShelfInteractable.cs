using UnityEngine;
using TMPro;

public class ShelfInteractable : MonoBehaviour
{
    [Header("Shelf Settings")]
    public int acceptedBoxID;
    public float placeInterval = 0.5f;
    public GameObject interactionIcon;

    [Header("Shelf Items")]
    public GameObject[] shelfItems;

    [Header("Floating Text Settings")]
    public GameObject floatingTextObj;
    public TMP_Text progressText;

    // Número total de prateleiras na cena — define no Inspector
    [Header("Completion Settings")]
    public static int totalShelves = 2;          // Podes mudar aqui ou tornar público no Inspector
    private static int completedShelves = 0;     // Contador estático partilhado entre todas as prateleiras

    public static bool isPlacingBox = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float placeTimer = 0f;
    private int itemsPlaced = 0;
    private bool isCompleted = false;

    private void Start()
    {
        interactionIcon.SetActive(false);
        if (floatingTextObj != null) floatingTextObj.SetActive(false);

        foreach (GameObject item in shelfItems)
            item.SetActive(false);

        UpdateProgressText();
    }

    // Garante que o contador reinicia quando a cena é carregada
    private void OnEnable()
    {
        completedShelves = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCompleted) return;

        if (collision.CompareTag("Player"))
        {
            inRange = true;
            if (TaskManager.instance.currentBoxHeldID == acceptedBoxID)
                interactionIcon.SetActive(true);
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
            if (!interactionIcon.activeSelf && !isInteracting)
                interactionIcon.SetActive(true);

            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    isInteracting = true;
                    isPlacingBox = true;
                    interactionIcon.SetActive(false);
                    if (floatingTextObj != null) floatingTextObj.SetActive(true);
                    UpdateProgressText();
                }

                placeTimer += Time.deltaTime;

                if (placeTimer >= placeInterval)
                {
                    if (itemsPlaced < shelfItems.Length)
                    {
                        shelfItems[itemsPlaced].SetActive(true);
                        itemsPlaced++;
                        UpdateProgressText();
                    }

                    placeTimer = 0f;

                    if (itemsPlaced >= shelfItems.Length)
                    {
                        isCompleted = true;
                        isInteracting = false;
                        isPlacingBox = false;

                        TaskManager.instance.currentBoxHeldID = 0;

                        if (floatingTextObj != null) floatingTextObj.SetActive(false);
                        interactionIcon.SetActive(false);

                        completedShelves++;

                        // Se todas as prateleiras estiverem completas, esconde o objetivo
                        if (completedShelves >= totalShelves)
                        {
                            ObjectiveFeedback.instance.HideObjective();
                        }
                        else
                        {
                            ObjectiveFeedback.instance.SetObjective("Pick up another box.");
                        }
                    }
                }
            }
            else
            {
                if (isInteracting) ResetCurrentItemInteraction();
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
            isPlacingBox = false;
            placeTimer = 0f;
            if (floatingTextObj != null) floatingTextObj.SetActive(false);
            if (inRange && TaskManager.instance.currentBoxHeldID == acceptedBoxID)
                interactionIcon.SetActive(true);
        }
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
            progressText.text = itemsPlaced + "/" + shelfItems.Length;
    }
}