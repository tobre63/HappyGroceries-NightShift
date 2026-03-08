using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic; // Necessário para usar Listas (Pilhas)

public class ObjectiveFeedback : MonoBehaviour
{
    public static ObjectiveFeedback instance;

    [Header("UI Reference")]
    public TMP_Text objectiveText;

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f;

    [Header("Stack Settings")]
    [Tooltip("Se estiver ativo, a UI desaparece completamente quando não há missões.")]
    public bool hideUIWhenEmpty = false;
    [Tooltip("A missão padrão que aparece quando a pilha está vazia.")]
    public string defaultObjective = "Pick up a box.";

    // --- A NOSSA "PILHA" DE OBJETIVOS ---
    private List<string> activeObjectives = new List<string>();
    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Começa com a missão padrão
        UpdateUI();
    }

    // ADICIONA À PILHA
    public void SetObjective(string newText, bool isPriority = false)
    {
        if (objectiveText == null) return;

        // Se este objetivo já estiver na lista, tira-o para não haver repetidos
        if (activeObjectives.Contains(newText))
        {
            activeObjectives.Remove(newText);
        }

        // Se for prioridade, vai para o TOPO (Fim da lista). Se não, vai para o FUNDO (Início da lista).
        if (isPriority)
        {
            activeObjectives.Add(newText);
        }
        else
        {
            activeObjectives.Insert(0, newText);
        }

        UpdateUI();
    }

    // REMOVE DA PILHA
    public void HideObjective(bool wasPriority = false)
    {
        if (objectiveText == null || activeObjectives.Count == 0) return;

        if (wasPriority)
        {
            // Remove o último papel (O que está no Topo das prioridades)
            activeObjectives.RemoveAt(activeObjectives.Count - 1);
        }
        else
        {
            // Remove o primeiro papel (O que está no Fundo, normalmente as caixas)
            activeObjectives.RemoveAt(0);
        }

        UpdateUI();
    }

    public void RemoveSpecificObjective(string objectiveToRemove)
    {
        if (activeObjectives.Contains(objectiveToRemove))
        {
            activeObjectives.Remove(objectiveToRemove);
            UpdateUI();
        }
    }

    public void ForceClearAll()
    {
        activeObjectives.Clear(); // Deita fora todos os papéis (incluindo clientes fantasmas)
        ChangeMainObjective("", true); // Muda o objetivo principal para vazio e desliga a UI
    }

    // LÓGICA DE DECISÃO DO QUE MOSTRAR
    private void UpdateUI()
    {
        if (activeObjectives.Count > 0)
        {
            // Lê sempre o que está no TOPO da pilha
            string textToShow = activeObjectives[activeObjectives.Count - 1];
            UpdateScreenText(textToShow);
        }
        else
        {
            // A pilha está vazia!
            if (hideUIWhenEmpty)
            {
                HideScreenText();
            }
            else
            {
                UpdateScreenText(defaultObjective);
            }
        }
    }

    // ==========================================
    //  SISTEMA DE ANIMAÇÕES (MANTIDO DO ORIGINAL)
    // ==========================================

    private void UpdateScreenText(string textToShow)
    {
        // Previne que o ecrã pisque se a missão que vai entrar for igual à que já lá está!
        if (objectiveText.text == textToShow && objectiveText.gameObject.activeSelf) return;

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(SetObjectiveRoutine(textToShow));
    }

    private void HideScreenText()
    {
        // Se já estiver escondido, não precisa de fazer Fade Out de novo
        if (!objectiveText.gameObject.activeSelf) return;

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

    public void ChangeMainObjective(string newObjective, bool finishQuest = false)
    {
        defaultObjective = newObjective;
        hideUIWhenEmpty = finishQuest; // Se for true, a UI desaparece no fim
        UpdateUI();
    }
}