using UnityEngine;
using UnityEngine.Events;

public class TaskManager : MonoBehaviour
{
    [Header("Manager Settings")]
    // 0 significa que o jogador tem as mãos livres. 
    // 1 será a Caixa 1, 2 será a Caixa 2.
    public int currentBoxHeldID = 0;

    public static TaskManager instance;

    private void Awake()
    {
        // Padrão Singleton simples para aceder facilmente a partir de outros scripts
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}