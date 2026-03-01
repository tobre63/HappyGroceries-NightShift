using UnityEngine;

public class BottleInteractable : MonoBehaviour
{
    public float timeToPickUp = 1f;
    public GameObject interactionIcon;
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    // VARIÁVEL PARA PARAR O MOVIMENTO DO JOGADOR
    public static bool isPickingUpBottle = false;

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

        if (collision.CompareTag("Player") && !CleaningEventController.instance.isBottlePickedUp)
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
        if (!CleaningEventController.instance.isQuestActive || !inRange || CleaningEventController.instance.isBottlePickedUp) return;

        if (Input.GetKey(KeyCode.E))
        {
            if (!isInteracting)
            {
                isInteracting = true;
                isPickingUpBottle = true; // BLOQUEIA O JOGADOR

                // CORREÇÃO AQUI: Esconde o ícone do 'E' enquanto enche a barra
                if (interactionIcon != null) interactionIcon.SetActive(false);
                if (progressBarObj != null) progressBarObj.SetActive(true);
            }

            holdTimer += Time.deltaTime;
            if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, (holdTimer / timeToPickUp) * 100f);

            if (holdTimer >= timeToPickUp)
            {
                CleaningEventController.instance.isBottlePickedUp = true;
                CleaningEventController.instance.CheckProgress();

                ResetInteraction(); // Garante que tudo limpa e o jogador é libertado ANTES de desativar o objeto
                gameObject.SetActive(false);
            }
        }
        else if (isInteracting) ResetInteraction();
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        isPickingUpBottle = false; // LIBERTA O JOGADOR
        holdTimer = 0f;

        if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, 0f);
        if (progressBarObj != null) progressBarObj.SetActive(false);

        if (inRange && !CleaningEventController.instance.isBottlePickedUp)
        {
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
    }
}