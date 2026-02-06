using UnityEngine;

public class TaskToggleController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Arraste aqui o objeto 'Task' que fica no canto superior esquerdo")]
    public GameObject taskObject;

    void Update()
    {
        // Verifica se a tecla TAB foi pressionada
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleTaskDisplay();
        }
    }

    private void ToggleTaskDisplay()
    {
        if (taskObject != null)
        {
            // Liga ou desliga o objeto dependendo do estado atual
            taskObject.SetActive(!taskObject.activeSelf);
        }
    }
}