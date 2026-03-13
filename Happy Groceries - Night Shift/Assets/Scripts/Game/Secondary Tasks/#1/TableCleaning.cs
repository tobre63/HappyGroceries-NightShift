using UnityEngine;
using TMPro;

public class TableCleaningInteractable : MonoBehaviour
{
    [Header("Cleaning Settings")]
    public float timeToCleanPerItem = 1.0f;
    public GameObject interactionIcon;

    [Header("Dirt Items")]
    public GameObject[] dirtItems;

    [Header("UI Settings")]
    public GameObject playerFloatingTextObj;
    public TMP_Text progressText;

    public static bool isCleaningTable = false;
    public static bool isTableClean = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float cleanTimer = 0f;
    private int itemsCleaned = 0;
    private bool isCompletelyClean = false;

    private Rigidbody2D playerRb;

    private void Start()
    {
        isTableClean = false;

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (playerFloatingTextObj != null) playerFloatingTextObj.SetActive(false);

        foreach (GameObject dirt in dirtItems)
        {
            dirt.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Registamos SEMPRE que o jogador está na área, com ou sem pano
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
        if (isCompletelyClean || !inRange) return;

        // Mantém o ícone atualizado (ex: apanha o pano estando já dentro da área da mesa)
        CheckInteractionIcon();

        // Se não tem pano, não vale a pena fazer o resto do Update
        if (!TaskManager.instance.hasCloth) return;

        if (Input.GetKey(KeyCode.E))
        {
            if (!isInteracting)
            {
                StartCleaning();
            }

            cleanTimer += Time.deltaTime;

            if (cleanTimer >= timeToCleanPerItem)
            {
                CleanNextItem();
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

    private void StartCleaning()
    {
        isInteracting = true;
        isCleaningTable = true;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (playerFloatingTextObj != null) playerFloatingTextObj.SetActive(true);

        UpdateProgressText();
    }

    private void CleanNextItem()
    {
        if (itemsCleaned < dirtItems.Length)
        {
            dirtItems[itemsCleaned].SetActive(false);
            itemsCleaned++;
            cleanTimer = 0f;

            UpdateProgressText();

            if (itemsCleaned >= dirtItems.Length)
            {
                CompleteTask();
            }
        }
    }

    private void CompleteTask()
    {
        isCompletelyClean = true;
        isTableClean = true;

        isInteracting = false;
        isCleaningTable = false;

        if (playerFloatingTextObj != null) playerFloatingTextObj.SetActive(false);
        if (interactionIcon != null) interactionIcon.SetActive(false);

        // AVISA O TASK CONTROLLER
        if (ClothTaskController.instance != null)
        {
            ClothTaskController.instance.isTableCleaned = true;
            ClothTaskController.instance.CheckProgress();
        }
    }

    private void ResetInteraction()
    {
        if (isInteracting)
        {
            isInteracting = false;
            isCleaningTable = false;
            cleanTimer = 0f;

            if (playerFloatingTextObj != null) playerFloatingTextObj.SetActive(false);

            CheckInteractionIcon();
        }
    }

    // NOVO: Função dedicada para gerir o ícone, igual à que usaste no ClothInteractable
    private void CheckInteractionIcon()
    {
        if (inRange && !isCompletelyClean && TaskManager.instance.hasCloth && !isInteracting)
        {
            if (interactionIcon != null && !interactionIcon.activeSelf) 
                interactionIcon.SetActive(true);
        }
        else
        {
            if (interactionIcon != null && interactionIcon.activeSelf) 
                interactionIcon.SetActive(false);
        }
    }

    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = itemsCleaned + "/" + dirtItems.Length;
        }
    }
}