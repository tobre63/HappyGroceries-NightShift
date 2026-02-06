using UnityEngine;

public class TaskToggleController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Arraste aqui o objeto 'Task' que fica no canto superior esquerdo")]
    public GameObject taskObject;

    void Update()
    {
        if (taskObject != null)
        {
            // Define o estado do objeto diretamente baseado se a tecla TAB está sendo segurada
            // Input.GetKey retorna true enquanto a tecla está pressionada e false caso contrário
            bool isTabPressed = Input.GetKey(KeyCode.Tab);

            // Só atualiza se o estado for diferente para evitar chamadas desnecessárias
            if (taskObject.activeSelf != isTabPressed)
            {
                taskObject.SetActive(isTabPressed);
            }
        }
    }
}