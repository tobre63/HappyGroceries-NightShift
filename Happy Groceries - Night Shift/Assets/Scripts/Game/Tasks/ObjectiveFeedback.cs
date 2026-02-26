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

    // Propriedades para o NPC conseguir ler o estado do objetivo
    public string CurrentObjective { get; private set; }
    public bool IsVisible { get; private set; }

    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetObjective("Pick up a box.");
    }

    public void SetObjective(string newText)
    {
        if (objectiveText == null) return;

        CurrentObjective = newText;
        IsVisible = true; // Marca como visível

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(SetObjectiveRoutine(newText));
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

    public void HideObjective()
    {
        if (objectiveText == null) return;

        IsVisible = false; // Marca como escondido

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(HideRoutine());
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