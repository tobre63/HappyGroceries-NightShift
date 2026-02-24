using UnityEngine;

public class BoxInteractable : MonoBehaviour
{
    [Header("Box Settings")]
    public int boxID; // Define 1 para a primeira caixa, 2 para a segunda
    public float timeToPickUp = 5f; // Os 5 segundos necessários para apanhar
    public GameObject interactionIcon; // O ícone que surge por cima da caixa

    [Header("Progress Bar Settings")]
    public GameObject progressBarObj; // O GameObject do radial progress bar
    public Renderer progressBarRenderer; // O Renderer que contém o material com o teu shader
    public string percentageProperty = "_Percentage"; // O nome interno da propriedade no shader

    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;

    private void Start()
    {
        interactionIcon.SetActive(false);

        // Garante que a barra começa invisível
        if (progressBarObj != null)
        {
            progressBarObj.SetActive(false);
        }

        // Criamos uma instância do material para não alterar o material original em outras caixas
        if (progressBarRenderer != null)
        {
            progressMaterial = progressBarRenderer.material;
            SetProgress(0f); // Garante que a barra começa a 0
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se é o jogador e se este NÃO tem nenhuma caixa nas mãos (ID == 0)
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

            // Segurança: se o jogador for empurrado para fora enquanto carrega no E
            ResetInteraction();
        }
    }

    private void Update()
    {
        // Se o jogador está na zona e tem as mãos livres
        if (inRange && TaskManager.instance.currentBoxHeldID == 0)
        {
            if (Input.GetKey(KeyCode.E))
            {
                // Dispara o evento de parar o movimento no primeiro frame
                if (!isInteracting)
                {
                    isInteracting = true;
                    TaskManager.instance.onInteractionStart.Invoke();

                    // Oculta o ícone normal e mostra a barra de progresso
                    interactionIcon.SetActive(false);
                    if (progressBarObj != null) progressBarObj.SetActive(true);
                }

                holdTimer += Time.deltaTime;

                // Calcula a percentagem de 0 a 100 com base no tempo atual
                float currentPercentage = (holdTimer / timeToPickUp) * 100f;
                SetProgress(currentPercentage);

                // Concluiu os 5 segundos
                if (holdTimer >= timeToPickUp)
                {
                    isInteracting = false;
                    TaskManager.instance.currentBoxHeldID = boxID; // Guarda a caixa
                    TaskManager.instance.onInteractionStop.Invoke(); // Liberta o movimento

                    // Oculta a barra de progresso e desativa a caixa
                    if (progressBarObj != null) progressBarObj.SetActive(false);
                    gameObject.SetActive(false); // Esconde a caixa (apanhada)
                }
            }
            else
            {
                // Se largar a tecla antes do tempo, cancela a ação e reinicia o tempo
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
            holdTimer = 0f;
            SetProgress(0f); // Volta a pôr o shader a 0%

            // Oculta a barra e volta a mostrar o ícone de interação, caso ainda esteja perto da caixa
            if (progressBarObj != null) progressBarObj.SetActive(false);
            if (inRange && TaskManager.instance.currentBoxHeldID == 0)
            {
                interactionIcon.SetActive(true);
            }

            TaskManager.instance.onInteractionStop.Invoke(); // Liberta o movimento
        }
    }

    // Função auxiliar para mudar o valor do shader
    private void SetProgress(float value)
    {
        if (progressMaterial != null)
        {
            progressMaterial.SetFloat(percentageProperty, value);
        }
    }
}