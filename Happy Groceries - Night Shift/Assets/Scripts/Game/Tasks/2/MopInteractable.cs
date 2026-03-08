using UnityEngine;

public class MopInteractable : MonoBehaviour
{
    public float timeToInteract = 1.5f;
    public GameObject mopVisuals; // Arrasta a imagem da esfregona para aqui
    public GameObject interactionIcon;
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    // VARIÁVEL PARA PARAR O MOVIMENTO DO JOGADOR
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
        if (!CleaningEventController.instance.isQuestActive) return;

        if (collision.CompareTag("Player"))
        {
            inRange = true;
            if (!CleaningEventController.instance.isMopEquipped || CleaningEventController.instance.IsTaskReadyToFinish())
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
        if (!CleaningEventController.instance.isQuestActive || !inRange) return;

        bool canPickUp = !CleaningEventController.instance.isMopEquipped;
        bool canPutAway = CleaningEventController.instance.isMopEquipped && CleaningEventController.instance.IsTaskReadyToFinish();

        if (canPickUp || canPutAway)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    isInteracting = true;
                    isInteractingWithMop = true; // BLOQUEIA O JOGADOR

                    // CORREÇÃO AQUI: Esconde o ícone do 'E' enquanto enche a barra
                    if (interactionIcon != null) interactionIcon.SetActive(false);
                    if (progressBarObj != null) progressBarObj.SetActive(true);
                }

                holdTimer += Time.deltaTime;
                if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, (holdTimer / timeToInteract) * 100f);

                if (holdTimer >= timeToInteract)
                {
                    isInteracting = false;
                    isInteractingWithMop = false; // LIBERTA O JOGADOR

                    if (canPickUp)
                    {
                        CleaningEventController.instance.isMopEquipped = true;
                        SetMopAlpha(0f); // Fica invisível (Alpha 0)

                        if (ObjectiveFeedback.instance != null)
                        {
                            // Remove o objetivo antigo e mete o novo
                            ObjectiveFeedback.instance.RemoveSpecificObjective("Pick up the mop.");
                            ObjectiveFeedback.instance.SetObjective("Clean the scene.", true);
                        }
                    }
                    else if (canPutAway)
                    {
                        CleaningEventController.instance.isMopEquipped = false;
                        SetMopAlpha(1f); // Volta a ficar visível (Alpha 1)

                        // IMPEDE QUE A MOP SEJA APANHADA OUTRA VEZ: Desliga o colisor
                        Collider2D col = GetComponent<Collider2D>();
                        if (col != null) col.enabled = false;

                        if (ObjectiveFeedback.instance != null)
                        {
                            // NOVO: Aplica a força nuclear e apaga TUDO do ecrã!
                            ObjectiveFeedback.instance.ForceClearAll();
                        }
                    }
                    ResetInteraction();
                }
            }
            else if (isInteracting) ResetInteraction();
        }
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        isInteractingWithMop = false; // Garante que liberta o jogador se ele largar o 'E' a meio
        holdTimer = 0f;
        if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, 0f);
        if (progressBarObj != null) progressBarObj.SetActive(false);

        if (inRange && (!CleaningEventController.instance.isMopEquipped || CleaningEventController.instance.IsTaskReadyToFinish()))
            if (interactionIcon != null) interactionIcon.SetActive(true);
    }

    // Função que altera apenas a transparência do SpriteRenderer
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