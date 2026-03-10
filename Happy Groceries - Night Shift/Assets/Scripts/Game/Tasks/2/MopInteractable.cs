using UnityEngine;

public class MopInteractable : MonoBehaviour
{
    [Header("Configurações")]
    public float timeToInteract = 1.5f;

    [Header("Referências Visuais")]
    public GameObject mopVisuals; // Sprite do Mop no chão/parede
    public GameObject interactionIcon;
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    // VARIÁVEL GLOBAL PARA PARAR O MOVIMENTO DO JOGADOR
    public static bool isInteractingWithMop = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;

    private void Start()
    {
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(false);
        if (progressBarRenderer != null) progressMaterial = progressBarRenderer.material;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // REMOVIDO: if (!CleaningEventController.instance.isQuestActive) return;

        if (collision.CompareTag("Player"))
        {
            inRange = true;
            UpdateIconVisibility();
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
        // REMOVIDO: if (!CleaningEventController.instance.isQuestActive) ...
        if (!inRange) return;

        // LÓGICA ATUALIZADA:
        // Pode pegar se não tiver equipado.
        bool canPickUp = !CleaningEventController.instance.isMopEquipped;

        // Pode guardar se tiver equipado (removemos a restrição de IsTaskReadyToFinish aqui para não travar o jogador)
        bool canPutAway = CleaningEventController.instance.isMopEquipped;

        // Atualiza ícone dinamicamente caso o estado mude enquanto o jogador está parado dentro do trigger
        if (!isInteracting) UpdateIconVisibility();

        if (canPickUp || canPutAway)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    StartInteraction();
                }

                holdTimer += Time.deltaTime;
                UpdateProgressBar();

                if (holdTimer >= timeToInteract)
                {
                    FinishInteraction(canPickUp, canPutAway);
                }
            }
            else if (isInteracting)
            {
                ResetInteraction(); // Soltou a tecla antes da hora
            }
        }
    }

    private void StartInteraction()
    {
        isInteracting = true;
        isInteractingWithMop = true; // Bloqueia Jogador

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);
    }

    private void FinishInteraction(bool isPickingUp, bool isPuttingAway)
    {
        isInteracting = false;
        isInteractingWithMop = false; // Libera Jogador

        if (isPickingUp)
        {
            // --- PEGAR O MOP ---
            CleaningEventController.instance.isMopEquipped = true;
            SetMopAlpha(0f); // Esconde o mop do cenário

            // Atualiza objetivos se o sistema existir
            if (ObjectiveFeedback.instance != null)
            {
                ObjectiveFeedback.instance.RemoveSpecificObjective("Pick up the mop.");
                // Adiciona o objetivo de limpar apenas se ainda não tiver limpado tudo
                if (!CleaningEventController.instance.IsTaskReadyToFinish())
                {
                    ObjectiveFeedback.instance.SetObjective("Clean the scene.", true);
                }
            }
        }
        else if (isPuttingAway)
        {
            // --- GUARDAR O MOP ---
            CleaningEventController.instance.isMopEquipped = false;
            SetMopAlpha(1f); // Mostra o mop no cenário novamente

            // Só finaliza a Quest "oficialmente" se as sujeiras estiverem limpas
            if (CleaningEventController.instance.IsTaskReadyToFinish())
            {
                // Desliga o colisor apenas se completou tudo (fim da task)
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                if (ObjectiveFeedback.instance != null)
                {
                    ObjectiveFeedback.instance.ForceClearAll();
                }
            }
            // Se não terminou de limpar, o jogador apenas guardou o mop, mas pode pegar de novo depois.
        }

        ResetInteraction();
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        isInteractingWithMop = false;
        holdTimer = 0f;

        if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, 0f);
        if (progressBarObj != null) progressBarObj.SetActive(false);

        if (inRange) UpdateIconVisibility();
    }

    private void UpdateIconVisibility()
    {
        if (interactionIcon == null) return;

        // Lógica simples: Se estou no alcance e não estou interagindo, mostre o ícone.
        // O jogador sempre pode interagir (pegar ou guardar)
        interactionIcon.SetActive(true);
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