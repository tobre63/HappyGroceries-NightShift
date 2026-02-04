using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    [Header("Configurações de Telas")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private CanvasGroup fadePanel;

    [Header("Configuração do Texto (Imagem)")]
    [SerializeField] private TextMeshProUGUI loadingText; // O campo que aparece na sua imagem
    [TextArea(8, 15)]
    [SerializeField] private string fullText; // Cole aqui o texto do Julian Frost
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float waitAfterText = 2.0f;

    [Header("Configurações de Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    // CanvasGroup usado especificamente para o texto (para fazermos fade só nele)
    private CanvasGroup loadingTextGroup;

    private void Awake()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadingText != null) loadingText.text = "";

        // Garante que o texto tem um CanvasGroup (para podermos animar alpha apenas do texto)
        if (loadingText != null)
        {
            loadingTextGroup = loadingText.GetComponent<CanvasGroup>();
            if (loadingTextGroup == null)
                loadingTextGroup = loadingText.gameObject.AddComponent<CanvasGroup>();

            loadingTextGroup.alpha = 1f;
            loadingTextGroup.blocksRaycasts = false; // evita bloquear input enquanto texto
        }

        if (fadePanel != null)
        {
            fadePanel.alpha = 1;
            fadePanel.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // Fade In inicial ao abrir o menu
        StartCoroutine(DoFade(1, 0, 1.5f));
    }

    public void StartGame(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        PlaySound(clickSound);
        StartCoroutine(SequenceWithDiary(sceneName));
    }

    private IEnumerator SequenceWithDiary(string sceneName)
    {
        // 1. Escurece a tela para o fundo ficar preto
        yield return StartCoroutine(DoFade(0, 1, 1.5f));

        // 2. Anima o "Full Text" no componente "loadingText" (letra por letra)
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "";
            foreach (char letter in fullText.ToCharArray())
            {
                loadingText.text += letter;
                if (typeSound != null && audioSource != null) audioSource.PlayOneShot(typeSound);
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // 3. Tempo de espera para leitura
        yield return new WaitForSeconds(waitAfterText);

        // 4. FADE OUT do texto (assim que acabar)
        if (loadingTextGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(loadingTextGroup, 1f, 0f, 1.0f)); // duração 1s (ajusta se desejar)
            loadingText.gameObject.SetActive(false);
        }

        // Opcional: se quiseres também fazer fade do fadePanel (tela preta) antes de mudar de cena,
        // podes descomentar a linha abaixo e ajustar duração:
        // yield return StartCoroutine(DoFade(1, 0, 0.5f));

        // 5. Muda de cena
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator DoFade(float start, float end, float duration)
    {
        if (fadePanel == null) yield break;
        fadePanel.gameObject.SetActive(true);

        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }
        fadePanel.alpha = end;
        if (end <= 0) fadePanel.gameObject.SetActive(false);
    }

    // Coroutine genérica para fazer fade de qualquer CanvasGroup
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null) yield break;
        float timer = 0f;
        cg.alpha = start;
        cg.gameObject.SetActive(true);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }

        cg.alpha = end;
        // Se finalizou com alpha 0, opcionalmente desativa o objeto
        if (end <= 0f) cg.gameObject.SetActive(false);
    }

    // Funções de Menu e Áudio
    public void OpenSettings() { PlaySound(clickSound); mainMenuPanel.SetActive(false); settingsPanel.SetActive(true); }
    public void CloseSettings() { PlaySound(clickSound); settingsPanel.SetActive(false); mainMenuPanel.SetActive(true); }
    public void QuitGame() { PlaySound(clickSound); Application.Quit(); }
    public void OnButtonHover() => PlaySound(hoverSound);
    private void PlaySound(AudioClip clip) { if (clip != null && audioSource != null) audioSource.PlayOneShot(clip); }
}