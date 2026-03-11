using UnityEngine;

public class CleanFloor : MonoBehaviour
{
    [Header("Configurações")]
    public float timeToClean = 2.0f;

    [Header("Referências Visuais")]
    public GameObject interactionIcon;
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    private bool isPlayerInside = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;

    private void Start()
    {
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(false);

        if (progressBarRenderer != null)
            progressMaterial = progressBarRenderer.material;
    }

    private void Update()
    {
        if (!isPlayerInside) return;

        // Verifica se tem o Mop equipado através do controller principal
        bool hasMop = CleaningEventController.instance.isMopEquipped;

        // Verifica se a missão secundária está ativa
        bool isSecQuestActive = MopTaskController.instance != null && MopTaskController.instance.isQuestActive;

        // Se não tiver a mop OU se a missão secundária ainda não começou, não faz nada
        if (!hasMop || !isSecQuestActive)
        {
            if (isInteracting) ResetInteraction();
            if (interactionIcon != null) interactionIcon.SetActive(false);
            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (!isInteracting) StartInteraction();

            holdTimer += Time.deltaTime;
            UpdateProgressBar();

            if (holdTimer >= timeToClean)
            {
                CleanDirt();
            }
        }
        else
        {
            if (isInteracting) ResetInteraction();

            if (interactionIcon != null && !interactionIcon.activeSelf)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    private void StartInteraction()
    {
        isInteracting = true;
        holdTimer = 0f;

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);

        // Bloqueia o jogador
        if (CleaningEventController.instance != null)
            CleaningEventController.instance.isCleaningDirt = true;
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        holdTimer = 0f;

        if (progressBarObj != null) progressBarObj.SetActive(false);
        if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, 0f);

        // Liberta o jogador
        if (CleaningEventController.instance != null)
            CleaningEventController.instance.isCleaningDirt = false;
    }

    private void CleanDirt()
    {
        // Liberta o jogador 
        if (CleaningEventController.instance != null)
            CleaningEventController.instance.isCleaningDirt = false;

        // AVISA O TASK CONTROLLER DA MISSÃO SECUNDÁRIA!
        if (MopTaskController.instance != null && MopTaskController.instance.isQuestActive)
        {
            MopTaskController.instance.dirtCleanedCount++;
            MopTaskController.instance.CheckProgress();
        }

        // Destrói a sujidade
        Destroy(gameObject);
    }

    private void UpdateProgressBar()
    {
        if (progressMaterial != null)
        {
            float percentage = (holdTimer / timeToClean) * 100f;
            progressMaterial.SetFloat(percentageProperty, percentage);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            ResetInteraction();
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }
}