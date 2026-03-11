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

    private bool wasQuestActive = false;
    private Collider2D mopCollider;

    private void Start()
    {
        mopCollider = GetComponent<Collider2D>();

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
        // LÓGICA DO COLLIDER: Só está ativo se alguma das missões (Principal ou Secundária) estiver ativa
        bool mainActive = CleaningEventController.instance != null && CleaningEventController.instance.isQuestActive;
        bool secActive = MopTaskController.instance != null && MopTaskController.instance.isQuestActive;

        if (mopCollider != null)
        {
            mopCollider.enabled = (mainActive || secActive);
        }

        CheckQuestActivation();

        if (!inRange || !mopCollider.enabled) return;

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
                ResetInteraction();
            }
        }
    }

    private void CheckQuestActivation()
    {
        // Mantém a tua lógica original da Main Quest intocável
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

    private bool CanPickUpMop()
    {
        if (CleaningEventController.instance.isMopEquipped) return false;

        // A Mop pode ser apanhada se: A main quest precisar dela, OU a sec quest precisar dela
        bool mainNeedsMop = CleaningEventController.instance.isQuestActive && !CleaningEventController.instance.IsTaskReadyToFinish();
        bool secNeedsMop = MopTaskController.instance != null && MopTaskController.instance.isQuestActive && MopTaskController.instance.dirtCleanedCount < MopTaskController.instance.totalDirtToClean;

        return mainNeedsMop || secNeedsMop;
    }

    private bool CanPutMopAway()
    {
        if (!CleaningEventController.instance.isMopEquipped) return false;

        // Se a Mop ainda for precisa para alguma missão, não deixa pousar
        bool mainNeedsIt = CleaningEventController.instance.isQuestActive && !CleaningEventController.instance.IsTaskReadyToFinish();
        bool secNeedsIt = MopTaskController.instance != null && MopTaskController.instance.isQuestActive && MopTaskController.instance.dirtCleanedCount < MopTaskController.instance.totalDirtToClean;

        if (mainNeedsIt || secNeedsIt) return false;

        // Se chegou aqui, já ninguém precisa de limpar. Verifica se alguma missão já terminou a limpeza
        bool mainDone = CleaningEventController.instance.isQuestActive && CleaningEventController.instance.IsTaskReadyToFinish();
        bool secDone = MopTaskController.instance != null && MopTaskController.instance.isQuestActive && MopTaskController.instance.dirtCleanedCount >= MopTaskController.instance.totalDirtToClean;

        return mainDone || secDone;
    }

    private void CheckIconVisibility()
    {
        if (interactionIcon == null) return;
        interactionIcon.SetActive(CanPickUpMop() || CanPutMopAway());
    }

    private void StartInteraction()
    {
        isInteracting = true;
        isInteractingWithMop = true;

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);
    }

    private void FinishInteraction(bool isPickingUp, bool isPuttingAway)
    {
        isInteracting = false;
        isInteractingWithMop = false;

        if (isPickingUp)
        {
            // O estado global do jogador estar com a Mop é controlado pela Main Quest, usamos essa variável
            CleaningEventController.instance.isMopEquipped = true;
            SetMopAlpha(0f);

            // Avisa a Missão Principal
            if (CleaningEventController.instance.isQuestActive && ObjectiveFeedback.instance != null)
            {
                ObjectiveFeedback.instance.RemoveSpecificObjective("Pick up the mop.");
                if (!CleaningEventController.instance.IsTaskReadyToFinish())
                {
                    ObjectiveFeedback.instance.SetObjective("Clean the scene.", true);
                }
            }

            // Avisa a Missão Secundária
            if (MopTaskController.instance != null && MopTaskController.instance.isQuestActive)
            {
                MopTaskController.instance.isMopPickedUp = true;
                MopTaskController.instance.CheckProgress();
            }
        }
        else if (isPuttingAway)
        {
            CleaningEventController.instance.isMopEquipped = false;
            SetMopAlpha(1f);

            // Avisa a Missão Principal
            if (CleaningEventController.instance.isQuestActive && ObjectiveFeedback.instance != null)
            {
                ObjectiveFeedback.instance.ForceClearAll();
            }

            // Avisa a Missão Secundária
            if (MopTaskController.instance != null && MopTaskController.instance.isQuestActive)
            {
                MopTaskController.instance.isMopPickedUp = false;
                MopTaskController.instance.CheckProgress();
            }
        }

        ResetInteraction();
        CheckIconVisibility();
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