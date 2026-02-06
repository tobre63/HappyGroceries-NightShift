using UnityEngine;

public class TaskToggleController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject taskObject;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.Tab;

    public bool holdToShow = true;

    private bool isToggled = false;

    void Update()
    {
        if (taskObject == null) return;

        if (holdToShow)
        {
            // Modo HOLD: Mostra apenas enquanto a tecla está pressionada (comportamento atual)
            bool isKeyPressed = Input.GetKey(toggleKey);

            if (taskObject.activeSelf != isKeyPressed)
            {
                taskObject.SetActive(isKeyPressed);
            }
        }
        else
        {
            // Modo TOGGLE: Clica uma vez para mostrar, clica de novo para esconder
            if (Input.GetKeyDown(toggleKey))
            {
                isToggled = !isToggled;
                taskObject.SetActive(isToggled);
            }
        }
    }

    // Função pública para outros scripts controlarem (opcional)
    public void ShowTasks(bool show)
    {
        if (taskObject != null)
        {
            taskObject.SetActive(show);
            if (!holdToShow) isToggled = show;
        }
    }

    // Fecha as tasks se o jogador sair do alcance de algo, por exemplo
    public void ForceClose()
    {
        if (taskObject != null)
        {
            taskObject.SetActive(false);
            isToggled = false;
        }
    }
}