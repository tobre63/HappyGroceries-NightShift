using UnityEngine;

public class DirtZoneInteractable : MonoBehaviour
{
    public float timeToCleanLevel = 2f;
    public GameObject[] dirtStages;
    public GameObject interactionIcon;

    // VARIÁVEL PARA PARAR O MOVIMENTO DO JOGADOR
    public static bool isCleaningDirt = false;

    private int currentStageIndex = 0;
    private bool isFullyCleaned = false;
    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;

    private void Start()
    {
        if (interactionIcon != null) interactionIcon.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CleaningEventController.instance.isQuestActive) return;

        if (collision.CompareTag("Player") && !isFullyCleaned && CleaningEventController.instance.isMopEquipped)
        {
            inRange = true;
            if (interactionIcon != null) interactionIcon.SetActive(true);
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
        if (!CleaningEventController.instance.isQuestActive || !inRange || isFullyCleaned || !CleaningEventController.instance.isMopEquipped) return;

        if (Input.GetKey(KeyCode.E))
        {
            if (!isInteracting)
            {
                isInteracting = true;
                isCleaningDirt = true; // BLOQUEIA O JOGADOR

                if (interactionIcon != null) interactionIcon.SetActive(true);
            }

            holdTimer += Time.deltaTime;

            if (holdTimer >= timeToCleanLevel)
            {
                CleanCurrentStage();
            }
        }
        else if (isInteracting)
        {
            ResetInteraction();
        }
    }

    private void CleanCurrentStage()
    {
        if (dirtStages[currentStageIndex] != null)
        {
            dirtStages[currentStageIndex].SetActive(false);
        }

        currentStageIndex++;

        // CORREÇÃO AQUI: Em vez de fazer um ResetInteraction() completo, apenas zeramos o tempo!
        // Assim, a variável 'isCleaningDirt' continua 'true' e o jogador não dá aquele passo falso.
        holdTimer = 0f;

        if (currentStageIndex >= dirtStages.Length)
        {
            isFullyCleaned = true;
            inRange = false;
            if (interactionIcon != null) interactionIcon.SetActive(false);

            CleaningEventController.instance.dirtZonesCleaned++;
            CleaningEventController.instance.CheckProgress();

            // A nódoa sumiu de vez, agora sim libertamos o jogador!
            ResetInteraction();
        }
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        isCleaningDirt = false; // LIBERTA O JOGADOR
        holdTimer = 0f;

        if (inRange && !isFullyCleaned && interactionIcon != null)
        {
            interactionIcon.SetActive(true);
        }
    }
}