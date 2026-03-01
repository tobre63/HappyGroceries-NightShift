using UnityEngine;

public class TutorialIconTrigger : MonoBehaviour
{
    private void OnEnable()
    {
        // Sempre que este objeto for ativado (SetActive(true)), ele tenta chamar o tutorial.
        // Como o Manager tem o bloqueio "hasShown", o tutorial só vai obedecer à PRIMEIRA chamada de todas!
        if (InteractionTutorialManager.instance != null)
        {
            InteractionTutorialManager.instance.ShowTutorial();
        }
    }
}