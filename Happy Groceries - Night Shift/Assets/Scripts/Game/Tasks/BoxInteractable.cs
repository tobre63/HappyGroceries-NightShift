using UnityEngine;

public class BoxInteractable : MonoBehaviour
{
    [Header("Box Settings")]
    public int boxID; // Define 1 para a primeira caixa, 2 para a segunda
    public float timeToPickUp = 5f; // Os 5 segundos necessários para apanhar
    public GameObject interactionIcon; // O ícone que surge por cima da caixa

    private bool inRange = false;
    private bool isInteracting = false;
    private float holdTimer = 0f;

    private void Start()
    {
        interactionIcon.SetActive(false);
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
                }

                holdTimer += Time.deltaTime;

                // Concluiu os 5 segundos
                if (holdTimer >= timeToPickUp)
                {
                    isInteracting = false;
                    TaskManager.instance.currentBoxHeldID = boxID; // Guarda a caixa
                    TaskManager.instance.onInteractionStop.Invoke(); // Liberta o movimento

                    interactionIcon.SetActive(false);
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
            TaskManager.instance.onInteractionStop.Invoke(); // Liberta o movimento
        }
    }
}