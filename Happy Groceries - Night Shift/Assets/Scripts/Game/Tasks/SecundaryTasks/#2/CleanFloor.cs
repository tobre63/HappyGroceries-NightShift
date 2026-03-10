using UnityEngine;

public class CleanFloor : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Tempo (em segundos) segurando a tecla para limpar.")]
    public float timeToClean = 2.0f;

    [Header("Referências Visuais")]
    [Tooltip("Ícone de interação (tecla E).")]
    public GameObject interactionIcon;

    [Tooltip("Objeto que contém a barra de progresso.")]
    public GameObject progressBarObj;

    [Tooltip("Renderer da barra para controlar o preenchimento (Shader).")]
    public Renderer progressBarRenderer;

    [Tooltip("Nome da propriedade no Shader (ex: _Percentage).")]
    public string percentageProperty = "_Percentage";

    // Estado interno
    private bool isPlayerInside = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;
    private Material progressMaterial;

    private void Start()
    {
        // Inicializa visuais desligados
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(false);

        // Pega o material para alterar o valor do shader
        if (progressBarRenderer != null)
            progressMaterial = progressBarRenderer.material;
    }

    private void Update()
    {
        // Se o player não estiver dentro, não faz nada
        if (!isPlayerInside) return;

        // Verifica se tem o Mop
        bool hasMop = CleaningEventController.instance.isMopEquipped;

        // Se não tiver Mop, reseta tudo e sai
        if (!hasMop)
        {
            if (isInteracting) ResetInteraction();
            if (interactionIcon != null) interactionIcon.SetActive(false);
            return;
        }

        // Lógica de Input (Segurar E)
        if (Input.GetKey(KeyCode.E))
        {
            if (!isInteracting)
            {
                StartInteraction();
            }

            // Lógica de Progresso
            holdTimer += Time.deltaTime;
            UpdateProgressBar();

            // Verifica conclusão
            if (holdTimer >= timeToClean)
            {
                CleanDirt();
            }
        }
        else
        {
            // Se não está segurando a tecla
            if (isInteracting)
            {
                ResetInteraction(); // Cancelou no meio
            }

            // Garante que o ícone esteja visível se estivermos parados na sujeira com o Mop
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

        // Troca visual: Esconde Ícone -> Mostra Barra
        if (interactionIcon != null) interactionIcon.SetActive(false);
        if (progressBarObj != null) progressBarObj.SetActive(true);

        // Bloqueia movimento do jogador
        if (CleaningEventController.instance != null)
            CleaningEventController.instance.isCleaningDirt = true;
    }

    private void ResetInteraction()
    {
        isInteracting = false;
        holdTimer = 0f;

        // Reseta visual da barra
        if (progressBarObj != null) progressBarObj.SetActive(false);
        if (progressMaterial != null) progressMaterial.SetFloat(percentageProperty, 0f);

        // Libera movimento do jogador
        if (CleaningEventController.instance != null)
            CleaningEventController.instance.isCleaningDirt = false;

        // O ícone voltará a aparecer no próximo frame pelo Update
    }

    private void CleanDirt()
    {
        // Libera o jogador antes de destruir
        if (CleaningEventController.instance != null)
            CleaningEventController.instance.isCleaningDirt = false;

        // Opcional: Contabilizar progresso na Quest Global
        // CleaningEventController.instance.dirtZonesCleaned++;
        // CleaningEventController.instance.CheckProgress();

        // Destroi a sujeira
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
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            ResetInteraction(); // Reseta tudo se sair da área
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }
}