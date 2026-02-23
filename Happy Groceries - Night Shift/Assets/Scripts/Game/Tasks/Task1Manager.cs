using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestor opcional da Tarefa 1.
/// Coloca num GameObject vazio na cena e liga as duas prateleiras.
/// Dispara OnTask1Complete quando as duas prateleiras estiverem reabastecidas.
/// </summary>
public class Task1Manager : MonoBehaviour
{
    [Header("=== Referências ===")]
    public ShelfObject shelf1;
    public ShelfObject shelf2;

    [Header("=== Evento de Conclusão ===")]
    [Tooltip("Liga aqui qualquer função que deva correr quando a tarefa terminar")]
    public UnityEvent OnTask1Complete;

    private int shelvesDone = 0;

    public void OnShelfRestocked(ShelfObject shelf)
    {
        shelvesDone++;
        Debug.Log($"[Task1Manager] {shelvesDone}/2 prateleiras reabastecidas.");

        if (shelvesDone >= 2)
        {
            Debug.Log("[Task1Manager] ✔✔ TAREFA 1 COMPLETA!");
            OnTask1Complete?.Invoke();
        }
    }
}