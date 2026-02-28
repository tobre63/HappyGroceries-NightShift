using UnityEngine;
using TMPro;
using System.Collections;

public class ObjectiveFeedback : MonoBehaviour
{
    public static ObjectiveFeedback instance;

    [Header("UI Reference")]
    public TMP_Text objectiveText;

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f;

    // --- MEMÓRIA DOS OBJETIVOS ---
    private string backgroundObjective = "";
    private bool isBackgroundVisible = false;

    private string priorityObjective = "";
    private bool isPriorityActive = false;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetObjective("Pick up a box.");
    }

    // O "isPriority" diz-nos se isto é um objetivo super importante (NPC) ou normal (Caixas)
    public void SetObjective(string newText, bool isPriority = false)
    {
        if (objectiveText == null) return;

        if (isPriority)
        {
            priorityObjective = newText;
            isPriorityActive = true;
            UpdateScreenText(priorityObjective); // Força a mostrar o do NPC
        }
        else
        {
            backgroundObjective = newText;
            isBackgroundVisible = true;

            // Só atualiza o ecrã com as caixas se o NPC não estiver à espera!
            // (Mas guarda a informação na mesma em background)
            if (!isPriorityActive)
            {
                UpdateScreenText(backgroundObjective);
            }
        }
    }

    public void HideObjective(bool isPriority = false)
    {
        if (objectiveText == null) return;

        if (isPriority)
        {
            isPriorityActive = false;
            priorityObjective = "";

            // O NPC foi-se embora. Volta a mostrar o objetivo que as caixas/prateleiras definiram!
            if (isBackgroundVisible && !string.IsNullOrEmpty(backgroundObjective))
                UpdateScreenText(backgroundObjective);
            else
                HideScreenText();
        }
        else
        {
            isBackgroundVisible = false;
            backgroundObjective = "";

            // Só esconde do ecrã se o NPC não estiver a dominar a UI
            if (!isPriorityActive)
                HideScreenText();
        }
    }

    private void UpdateScreenText(string textToShow)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(SetObjectiveRoutine(textToShow));
    }

    private void HideScreenText()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator SetObjectiveRoutine(string newText)
    {
        yield return FadeOut();

        objectiveText.text = newText;
        objectiveText.gameObject.SetActive(true);

        Color c = objectiveText.color;
        c.a = 1f;
        objectiveText.color = c;
    }

    private IEnumerator HideRoutine()
    {
        yield return FadeOut();
        objectiveText.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        Color c = objectiveText.color;
        float startAlpha = c.a;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            objectiveText.color = c;
            yield return null;
        }

        c.a = 0f;
        objectiveText.color = c;
    }
}