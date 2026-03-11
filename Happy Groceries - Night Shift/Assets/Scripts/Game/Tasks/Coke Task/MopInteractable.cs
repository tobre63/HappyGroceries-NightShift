using UnityEngine;

public class MopInteractable : MonoBehaviour
{
    [Header("Configurações")]
    public float timeToInteract = 1.5f;

    [Header("Referências Visuais")]
    public GameObject mopVisuals;
    public GameObject interactionIcon;
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    public static bool isInteractingWithMop = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;

    // Para rastrear a transição da missão principal
    private bool wasQuestActive = false;

    private void Start()
    {
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(false);
        if (progressBarRenderer != null) progressMaterial = progressBarRenderer.material;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = true;
            CheckIconVisibility();
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
        CheckQuestActivation();

        if (!inRange) return;

        // Agora usamos as verificações para ambas as ações
        bool canPickUp = CanPickUpMop();
        bool canPutAway = CanPutMopAway();

        if (!isInteracting)
        {
            CheckIconVisibility();
        }

        if (canPickUp || canPutAway)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting) StartInteraction();

                holdTimer += Time.deltaTime;
                UpdateProgressBar();

                if (holdTimer >= timeToInteract)
                {
                    FinishInteraction(canPickUp, canPutAway);
                }
            }
            else if (isInteracting)
            {
                ResetInteraction(); // Cancelou antes de preencher a barra
            }
        }
    }

    private void CheckQuestActivation()
    {
        if (CleaningEventController.instance.isQuestActive && !wasQuestActive)
        {
            wasQuestActive = true;

            if (CleaningEventController.instance.isMopEquipped)
            {
                if (ObjectiveFeedback.instance != null)
                {
                    ObjectiveFeedback.instance.RemoveSpecificObjective("Pick up the mop.");
                    if (!CleaningEventController.instance.IsTaskReadyToFinish())
                    {
                        ObjectiveFeedback.instance.SetObjective("Clean the scene.", true);
                    }
                }
            }
        }
        else if (!CleaningEventController.instance.isQuestActive && wasQuestActive)
        {
            wasQuestActive = false;
        }
    }

    // NOVO: Verifica se o jogador tem motivos para pegar na esfregona
    private bool CanPickUpMop()
    {
        // Se já tem a esfregona na mão, não pode pegar
        if (CleaningEventController.instance.isMopEquipped) return false;

        // Verifica se há lixo secundário (CleanFloor) na cena
        bool hasSecondaryTasks = FindObjectsByType<CleanFloor>(FindObjectsSortMode.None).Length > 0;

        // Verifica se a missão principal está ativa e ainda NÃO foi terminada
        bool mainQuestNeedsMop = CleaningEventController.instance.isQuestActive && !CleaningEventController.instance.IsTaskReadyToFinish();

        // Só permite pegar se houver pelo menos uma tarefa por fazer
        return hasSecondaryTasks || mainQuestNeedsMop;
    }

    private bool CanPutMopAway()
    {
        if (!CleaningEventController.instance.isMopEquipped) return false;

        if (CleaningEventController.instance.isQuestActive)
        {
            // Quest Principal: Só guarda se a tarefa da Coca-Cola estiver limpa
            return CleaningEventController.instance.IsTaskReadyToFinish();
        }
        else
        {
            // Quest Secundária: Só guarda se não houver mais chãos sujos (CleanFloor) na cena
            return FindObjectsByType<CleanFloor>(FindObjectsSortMode.None).Length == 0;
        }
    }

    private void CheckIconVisibility()
    {
        if (interactionIcon == null) return;

        bool canPickUp = CanPickUpMop();
        bool canPutAway = CanPutMopAway();

        interactionIcon.SetActive(canPickUp || canPutAway);
    }

    private void StartInteraction()
    {
        isInteracting = true;
        isInteractingWithMop = true; // Bloqueia movimento

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);
    }

    private void FinishInteraction(bool isPickingUp, bool isPuttingAway)
    {
        isInteracting = false;
        isInteractingWithMop = false;

        if (isPickingUp)
        {
            CleaningEventController.instance.isMopEquipped = true;
            SetMopAlpha(0f);

            if (CleaningEventController.instance.isQuestActive && ObjectiveFeedback.instance != null)
            {
                ObjectiveFeedback.instance.RemoveSpecificObjective("Pick up the mop.");
                if (!CleaningEventController.instance.IsTaskReadyToFinish())
                {
                    ObjectiveFeedback.instance.SetObjective("Clean the scene.", true);
                }
            }
        }
        else if (isPuttingAway)
        {
            CleaningEventController.instance.isMopEquipped = false;
            SetMopAlpha(1f);

            if (CleaningEventController.instance.isQuestActive && ObjectiveFeedback.instance != null)
            {
                ObjectiveFeedback.instance.ForceClearAll();
            }
        }

        ResetInteraction();
        CheckIconVisibility(); // Atualiza o ícone (agora vai desaparecer se as tasks acabarem)
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        isInteractingWithMop = false;
        holdTimer = 0f;

        if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, 0f);
        if (progressBarObj != null) progressBarObj.SetActive(false);

        if (inRange) CheckIconVisibility();
    }

    private void UpdateProgressBar()
    {
        if (progressMaterial != null)
            progressMaterial.SetFloat(percentageProperty, (holdTimer / timeToInteract) * 100f);
    }

    private void SetMopAlpha(float alpha)
    {
        if (mopVisuals != null)
        {
            SpriteRenderer sr = mopVisuals.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }
}