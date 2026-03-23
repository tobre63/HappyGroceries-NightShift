using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Audio Settings")]
    [Tooltip("Arraste o componente AudioSource deste GameObject para cá.")]
    public AudioSource audioSource;
    [Tooltip("O som que vai tocar quando uma nova missão aparecer.")]
    public AudioClip newTaskSound;

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
        // Começa com a missão padrão, MAS passa 'false' para não tocar som ao carregar o jogo
        UpdateUI(false);
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

        // Passamos 'true' porque estamos a adicionar uma NOVA task
        UpdateUI(true);
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

        // Passamos 'false' para NÃO tocar som quando voltamos a uma task antiga da pilha
        UpdateUI(false);
    }

    public void RemoveSpecificObjective(string objectiveToRemove)
    {
        if (activeObjectives.Contains(objectiveToRemove))
        {
            activeObjectives.Remove(objectiveToRemove);
            UpdateUI(false); // Remoção não deve tocar som da nova task revelada
        }
    }

    public void ForceClearAll()
    {
        activeObjectives.Clear(); // Deita fora todos os papéis
        ChangeMainObjective("", true, false); // Muda o objetivo principal e não toca som
    }

    // LÓGICA DE DECISÃO DO QUE MOSTRAR
    // Adicionado parâmetro playSound
    private void UpdateUI(bool playSound = false)
    {
        if (activeObjectives.Count > 0)
        {
            // Lê sempre o que está no TOPO da pilha
            string textToShow = activeObjectives[activeObjectives.Count - 1];
            UpdateScreenText(textToShow, playSound);
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
                UpdateScreenText(defaultObjective, playSound);
            }
        }
    }

    // ==========================================
    //  SISTEMA DE ANIMAÇÕES E ÁUDIO
    // ==========================================

    private void UpdateScreenText(string textToShow, bool playSound)
    {
        // Previne que o ecrã pisque se a missão que vai entrar for igual à que já lá está!
        if (objectiveText.text == textToShow && objectiveText.gameObject.activeSelf) return;

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);

        // Passamos a permissão de som para a Coroutine
        activeCoroutine = StartCoroutine(SetObjectiveRoutine(textToShow, playSound));
    }

    private void HideScreenText()
    {
        // Se já estiver escondido, não precisa de fazer Fade Out de novo
        if (!objectiveText.gameObject.activeSelf) return;

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator SetObjectiveRoutine(string newText, bool playSound)
    {
        yield return FadeOut();

        // O texto só muda visualmente aqui
        objectiveText.text = newText;
        objectiveText.gameObject.SetActive(true);

        // --- NOVO: TOCAR O SOM ---
        // Se for permitido tocar som, e tivermos as referências configuradas
        if (playSound && audioSource != null && newTaskSound != null)
        {
            // PlayOneShot é excelente para UI, pois não corta outros sons que o mesmo AudioSource possa estar a tocar
            audioSource.PlayOneShot(newTaskSound);
        }

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

    // Adicionado parâmetro playSound
    public void ChangeMainObjective(string newObjective, bool finishQuest = false, bool playSound = true)
    {
        defaultObjective = newObjective;
        hideUIWhenEmpty = finishQuest; // Se for true, a UI desaparece no fim
        UpdateUI(playSound);
    }
}