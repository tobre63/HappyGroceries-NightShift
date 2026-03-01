using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class InteractionTutorialManager : MonoBehaviour
{
    public static InteractionTutorialManager instance;

    [Header("Tutorial Settings")]
    public float fadeDuration = 0.5f; // Tempo do Fade In / Fade Out
    public float displayTime = 10f;   // Tempo que a frase fica no ecrã

    private bool hasShown = false;    // Tranca para garantir que só aparece 1 vez no jogo todo
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();

        // Garante que o texto começa invisível assim que o jogo arranca
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // Esta função vai ser chamada pelos ícones
    public void ShowTutorial()
    {
        if (!hasShown)
        {
            hasShown = true;
            StartCoroutine(TutorialRoutine());
        }
    }

    private IEnumerator TutorialRoutine()
    {
        // 1. FADE IN
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // 2. ESPERA OS 10 SEGUNDOS
        yield return new WaitForSeconds(displayTime);

        // 3. FADE OUT
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}