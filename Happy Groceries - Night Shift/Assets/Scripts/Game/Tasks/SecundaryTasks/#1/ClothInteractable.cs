using UnityEngine;

public class ClothInteractable : MonoBehaviour
{
    [Header("Settings")]
    public float timeToPickUp = 2f; // Tempo para pegar o pano
    public GameObject interactionIcon;

    [Header("Progress Bar Settings")]
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    // Variável estática para parar o Player (similar ao isPickingUpBox)
    public static bool isPickingUpCloth = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;

    private void Start()
    {
        // Garante que a UI comece desativada
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(false);

        // Prepara o material da barra de progresso
        if (progressBarRenderer != null)
        {
            progressMaterial = progressBarRenderer.material;
            SetProgress(0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Só mostra o ícone se for o Player e ele AINDA NÃO tiver o pano
        if (collision.CompareTag("Player") && !TaskManager.instance.hasCloth)
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
        // Verifica se está no alcance e se ainda não tem o pano
        if (inRange && !TaskManager.instance.hasCloth)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    StartInteraction();
                }

                // Incrementa o tempo
                holdTimer += Time.deltaTime;
                float currentPercentage = (holdTimer / timeToPickUp) * 100f;
                SetProgress(currentPercentage);

                // Concluiu a ação
                if (holdTimer >= timeToPickUp)
                {
                    FinishInteraction();
                }
            }
            else
            {
                // Se soltar a tecla E no meio do processo
                if (isInteracting)
                {
                    ResetInteraction();
                }
            }
        }
    }

    private void StartInteraction()
    {
        isInteracting = true;
        isPickingUpCloth = true; // Para o movimento do player

        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);
    }

    private void FinishInteraction()
    {
        isInteracting = false;
        isPickingUpCloth = false; // Libera o player

        // Salva no TaskManager que pegamos o pano
        TaskManager.instance.hasCloth = true;

        Debug.Log("Pano coletado!");

        // Esconde barra e desativa o objeto da cena
        if (progressBarObj != null) progressBarObj.SetActive(false);
        gameObject.SetActive(false);
    }

    private void ResetInteraction()
    {
        if (isInteracting)
        {
            isInteracting = false;
            isPickingUpCloth = false; // Libera o player se cancelar
            holdTimer = 0f;
            SetProgress(0f);

            if (progressBarObj != null) progressBarObj.SetActive(false);

            // Mostra o ícone de novo se ainda estiver perto
            if (inRange && !TaskManager.instance.hasCloth)
            {
                if (interactionIcon != null) interactionIcon.SetActive(true);
            }
        }
    }

    private void SetProgress(float value)
    {
        if (progressMaterial != null)
        {
            progressMaterial.SetFloat(percentageProperty, value);
        }
    }
}