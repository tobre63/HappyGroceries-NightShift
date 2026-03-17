using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillerVision : MonoBehaviour
{
    [Tooltip("Arrasta o GameObject Pai (Killer) para aqui no Inspector")]
    public Killer killer; // <-- Tipo alterado para 'Killer'

    private void Reset()
    {
        // Agora procura pelo componente 'Killer' no objeto Pai
        killer = GetComponentInParent<Killer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Quando o jogador entra na área de deteção (o BoxCollider2D filho)
        if (collision.CompareTag("Player"))
        {
            killer.SetPlayerTarget(collision.transform); // Chama a função no script 'Killer'
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Quando o jogador sai da área de deteção
        if (collision.CompareTag("Player"))
        {
            killer.ClearPlayerTarget(); // Chama a função no script 'Killer'
        }
    }
}