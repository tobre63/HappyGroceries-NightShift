using UnityEngine;
using TMPro;
using System.Collections;

public class SecondaryObjectiveFeedback : MonoBehaviour
{
    public static SecondaryObjectiveFeedback instance;

    [Header("UI Reference")]
    public TMP_Text secondaryObjectiveText; // Arrasta o TextMeshPro do SecondaryObjectiveInfo para aqui

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Garante que o texto secundário começa escondido
        if (secondaryObjectiveText != null)
        {
            secondaryObjectiveText.gameObject.SetActive(false);
        }
    }

    public void SetObjective(string newText)
    {
        if (secondaryObjectiveText == null) return;

        // Pára qualquer fade que esteja a acontecer e começa um novo
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(SetObjectiveRoutine(newText));
    }

    public void HideObjective()
    {
        if (secondaryObjectiveText == null || !secondaryObjectiveText.gameObject.activeSelf) return;

        // Pára qualquer fade e faz o texto desaparecer
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator SetObjectiveRoutine(string newText)
    {
        yield return FadeOut();

        secondaryObjectiveText.text = newText;
        secondaryObjectiveText.gameObject.SetActive(true);

        Color c = secondaryObjectiveText.color;
        c.a = 1f;
        secondaryObjectiveText.color = c;
    }

    private IEnumerator HideRoutine()
    {
        yield return FadeOut();
        secondaryObjectiveText.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        if (!secondaryObjectiveText.gameObject.activeSelf) yield break;

        float t = 0f;
        Color c = secondaryObjectiveText.color;
        float startAlpha = c.a;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            secondaryObjectiveText.color = c;
            yield return null;
        }

        c.a = 0f;
        secondaryObjectiveText.color = c;
    }
}