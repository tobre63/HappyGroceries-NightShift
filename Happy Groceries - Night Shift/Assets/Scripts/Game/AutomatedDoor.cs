using UnityEngine;
// using UnityEngine.InputSystem; // Não é mais estritamente necessário se não usarmos PlayerInput, mas pode manter se usar em outros lugares

public class AutomatedDoor : MonoBehaviour
{
    bool triggerOpen = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Verifica se o objeto tem a tag "Player" OU a tag "NPCs"
        if (col.CompareTag("Player") || col.CompareTag("NPCs"))
        {
            triggerOpen = true;
            animator.SetBool("triggerOpen", triggerOpen);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        // Verifica novamente as tags para fechar a porta apenas quando esses objetos saírem
        if (col.CompareTag("Player") || col.CompareTag("NPCs"))
        {
            triggerOpen = false;
            animator.SetBool("triggerOpen", triggerOpen);
        }
    }
}