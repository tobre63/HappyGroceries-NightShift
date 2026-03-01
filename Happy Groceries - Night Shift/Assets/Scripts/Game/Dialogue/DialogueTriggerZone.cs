using UnityEngine;

public class DialogueTriggerZone : MonoBehaviour
{
    // --- DEPENDENCIES ---

    private NPCInteraction npcInteraction;

    void Start()
    {
        // Procura e armazena a referência do script de interação que se encontra no objeto pai (o NPC)
        // Isto é útil para quando o colisor de Trigger está num objeto filho para não interferir com a física principal do NPC
        npcInteraction = GetComponentInParent<NPCInteraction>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Deteta se o objeto que entrou na zona de colisão (Trigger) é o jogador
        if (collision.CompareTag("Player") && npcInteraction != null)
        {
            // Avisa o script principal do NPC que o jogador entrou no raio de alcance para iniciar conversa
            npcInteraction.OnPlayerEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Deteta se o jogador saiu da zona de colisão (Trigger)
        if (collision.CompareTag("Player") && npcInteraction != null)
        {
            // Avisa o script principal do NPC que o jogador já não se encontra no raio de alcance
            // (O que habitualmente força o fecho da interface de diálogo)
            npcInteraction.OnPlayerExit();
        }
    }
}