using UnityEngine;
using TMPro; // Necessário para usar o TextMeshPro

public class ShelfInteractable : MonoBehaviour
{
    [Header("Shelf Settings")]
    public int acceptedBoxID;
    public float placeInterval = 0.5f;
    public GameObject interactionIcon;

    [Header("Shelf Items")]
    // Array para guardares os itens/sprites que já são filhos desta prateleira
    public GameObject[] shelfItems;

    [Header("Floating Text Settings")]
    public GameObject floatingTextObj; // O GameObject vazio que contém o texto
    public TMP_Text progressText; // O componente TextMeshPro que vai mostrar os números

    // Variável estática para o PlayerController saber se o jogador está a colocar itens
    public static bool isPlacingBox = false;

    private bool inRange = false;
    private bool isInteracting = false;
    private float placeTimer = 0f;
    private int itemsPlaced = 0;
    private bool isCompleted = false;

    private void Start()
    {
        interactionIcon.SetActive(false);

        // Garante que o texto flutuante começa desligado
        if (floatingTextObj != null) floatingTextObj.SetActive(false);

        // Por segurança, garantimos que todos os itens começam desativados
        foreach (GameObject item in shelfItems)
        {
            item.SetActive(false);
        }

        UpdateProgressText(); // Atualiza logo no início para "0 / X"
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCompleted) return;

        if (collision.CompareTag("Player"))
        {
            inRange = true;

            if (TaskManager.instance.currentBoxHeldID == acceptedBoxID)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inRange = false;
            interactionIcon.SetActive(false);
            ResetCurrentItemInteraction();
        }
    }

    private void Update()
    {
        if (isCompleted || !inRange) return;

        if (TaskManager.instance.currentBoxHeldID == acceptedBoxID)
        {
            // Mostra o ícone de interação se não estiver a interagir e o ícone estiver desligado
            if (!interactionIcon.activeSelf && !isInteracting) interactionIcon.SetActive(true);

            if (Input.GetKey(KeyCode.E))
            {
                if (!isInteracting)
                {
                    isInteracting = true;
                    isPlacingBox = true; // Avisa o PlayerController

                    // Troca o ícone pelo texto flutuante
                    interactionIcon.SetActive(false);
                    if (floatingTextObj != null) floatingTextObj.SetActive(true);

                    UpdateProgressText(); // Garante que mostra o valor correto
                }

                placeTimer += Time.deltaTime;

                if (placeTimer >= placeInterval)
                {
                    // Ativa o GameObject atual na hierarquia, correspondente ao progresso
                    if (itemsPlaced < shelfItems.Length)
                    {
                        shelfItems[itemsPlaced].SetActive(true);
                        itemsPlaced++; // Aumenta o número de itens colocados
                        UpdateProgressText(); // Atualiza o texto para "1/X", "2/X", etc.
                    }

                    placeTimer = 0f;

                    // Verifica se já ativou todos os itens do array
                    if (itemsPlaced >= shelfItems.Length)
                    {
                        isCompleted = true;
                        isInteracting = false;
                        isPlacingBox = false; // Liberta o jogador

                        TaskManager.instance.currentBoxHeldID = 0;

                        // Desliga o texto flutuante no fim
                        if (floatingTextObj != null) floatingTextObj.SetActive(false);
                        interactionIcon.SetActive(false);
                    }
                }
            }
            else
            {
                if (isInteracting)
                {
                    ResetCurrentItemInteraction();
                }
            }
        }
        else
        {
            if (interactionIcon.activeSelf) interactionIcon.SetActive(false);
        }
    }

    private void ResetCurrentItemInteraction()
    {
        if (isInteracting)
        {
            isInteracting = false;
            isPlacingBox = false; // Liberta o jogador caso cancele a meio
            placeTimer = 0f;

            // Oculta o texto flutuante e volta a mostrar o ícone de interação
            if (floatingTextObj != null) floatingTextObj.SetActive(false);
            if (inRange && TaskManager.instance.currentBoxHeldID == acceptedBoxID)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    // Função auxiliar para atualizar o texto visualmente
    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text = itemsPlaced + "/" + shelfItems.Length;
        }
    }
}