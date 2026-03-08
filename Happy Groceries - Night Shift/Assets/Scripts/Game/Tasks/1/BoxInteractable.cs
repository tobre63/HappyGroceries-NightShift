using UnityEngine;

public class BoxInteractable : MonoBehaviour
{
    [Header("Box Settings")]
    public int boxID;
    public float timeToPickUp = 5f;
    public GameObject interactionIcon;

    [Header("Progress Bar Settings")]
    public GameObject progressBarObj;
    public Renderer progressBarRenderer;
    public string percentageProperty = "_Percentage";

    // Vari�vel est�tica para o PlayerController saber se o jogador est� a interagir
    public static bool isPickingUpBox = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;

    private void Start()
    {
        interactionIcon.SetActive(false);

        if (progressBarObj != null)
        {
            progressBarObj.SetActive(false);
        }

        if (progressBarRenderer != null)
        {
            progressMaterial = progressBarRenderer.material;
            SetProgress(0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && TaskManager.instance.currentBoxHeldID == 0)
        {
            inRange = true;
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
            interactionIcon.SetActive(false);
            ResetInteraction();
        }
    }

    private void Update()
    {
        if (inRange && TaskManager.instance.currentBoxHeldID == 0)
        {
            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    isInteracting = true;
                    isPickingUpBox = true; // Avisa o PlayerController para parar o movimento

                    interactionIcon.SetActive(false);
                    if (progressBarObj != null) progressBarObj.SetActive(true);
                }

                holdTimer += Time.deltaTime;

                float currentPercentage = (holdTimer / timeToPickUp) * 100f;
                SetProgress(currentPercentage);

                if (holdTimer >= timeToPickUp)
                {
                    isInteracting = false;
                    isPickingUpBox = false; // Liberta o jogador
                    TaskManager.instance.currentBoxHeldID = boxID;

                    if (ObjectiveFeedback.instance != null)
                    {
                        ObjectiveFeedback.instance.ChangeMainObjective("Carry the box to the shelf.");
                    }

                    if (progressBarObj != null) progressBarObj.SetActive(false);
                    gameObject.SetActive(false);
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
    }

    private void ResetInteraction()
    {
        if (isInteracting)
        {
            isInteracting = false;
            isPickingUpBox = false; // Liberta o jogador caso ele cancele a a��o a meio
            holdTimer = 0f;
            SetProgress(0f);

            if (progressBarObj != null) progressBarObj.SetActive(false);
            if (inRange && TaskManager.instance.currentBoxHeldID == 0)
            {
                interactionIcon.SetActive(true);
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