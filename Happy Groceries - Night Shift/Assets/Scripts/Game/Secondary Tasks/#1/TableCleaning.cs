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

    // Vari�veis de Controle para o script do Jogador ler
    public static bool isCleaningTable = false;
    public static bool isTableClean = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float cleanTimer = 0f;
    private int itemsCleaned = 0;
    private bool isCompletelyClean = false;

    // Refer�ncia para parar fisicamente o jogador
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
        if (isCompletelyClean) return;

        if (collision.CompareTag("Player") && TaskManager.instance.hasCloth)
        {
            inRange = true;
            playerRb = collision.GetComponent<Rigidbody2D>(); // Guarda o Rigidbody do jogador

            if (interactionIcon != null) interactionIcon.SetActive(true);
            UpdateProgressText();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
            playerRb = null; // Limpa a refer�ncia

            if (interactionIcon != null) interactionIcon.SetActive(false);
            ResetInteraction();
        }
    }

    private void Update()
    {
        if (isCompletelyClean || !inRange || !TaskManager.instance.hasCloth) return;

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
        isCleaningTable = true; // Bloqueia o input no player

        // Para o jogador fisicamente no momento em que aperta o E
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
        isCleaningTable = false; // Liberta o player

        if (playerFloatingTextObj != null) playerFloatingTextObj.SetActive(false);
        if (interactionIcon != null) interactionIcon.SetActive(false);
    }

    private void ResetInteraction()
    {
        if (isInteracting)
        {
            isInteracting = false;
            isCleaningTable = false; // Liberta o player se soltar o E
            cleanTimer = 0f;

            if (playerFloatingTextObj != null) playerFloatingTextObj.SetActive(false);

            if (inRange && !isCompletelyClean)
            {
                if (interactionIcon != null) interactionIcon.SetActive(true);
            }
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