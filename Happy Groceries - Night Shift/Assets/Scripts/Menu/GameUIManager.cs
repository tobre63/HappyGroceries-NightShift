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
    [SerializeField] private CanvasGroup fadePanel; // Objeto 'Transition' (CanvasGroup usado só para bloquear interações)

    [Header("Transition Visual")]
    [SerializeField] private Image fadeImage; // nova: imagem full-screen usada como fundo do fade (preta)

    [Header("Configuração do Texto (Julian Frost)")]
    [SerializeField] private TextMeshProUGUI loadingText; // Objeto 'Transition Text'
    [TextArea(10, 20)]
    [SerializeField] private string fullText;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float waitAfterText = 2.0f;

    [Header("Configurações de Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private void Awake()
    {
        // Limpa o texto e prepara os painéis
        if (loadingText != null)
        {
            loadingText.text = "";
            loadingText.gameObject.SetActive(false);
        }

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Inicializa CanvasGroup (usado apenas para bloquear interações)
        if (fadePanel != null)
        {
            fadePanel.alpha = 1f; // podemos deixar opaco no arranque se cena iniciar escura
            fadePanel.gameObject.SetActive(true);
            fadePanel.blocksRaycasts = true;
        }

        // Inicializa imagem de fade
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f; // começa opaca (se queres começar escuro)
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);

            // garante que a imagem fica atrás do texto
            fadeImage.transform.SetAsFirstSibling();
            if (loadingText != null) loadingText.transform.SetAsLastSibling();
        }
    }

    private void Start()
    {
        // Fade inicial para revelar o menu usando a imagem
        StartCoroutine(DoFadeImage(1f, 0f, 1.5f));
    }

    public void StartGame(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        PlaySound(clickSound);
        StartCoroutine(SequenceWithDiary(sceneName));
    }

    private IEnumerator SequenceWithDiary(string sceneName)
    {
        // 1. Escurece a tela (fade da imagem para alpha = 1)
        yield return StartCoroutine(DoFadeImage(0f, 1f, 1.0f));

        // 2. Garante que o texto esteja activo e visível
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "";

            // garante que o texto está por cima visualmente
            loadingText.transform.SetAsLastSibling();

            foreach (char letter in fullText.ToCharArray())
            {
                loadingText.text += letter;
                if (typeSound != null && audioSource != null)
                    audioSource.PlayOneShot(typeSound);

                yield return new WaitForSeconds(typingSpeed);
            }
        }

        yield return new WaitForSeconds(waitAfterText);

        // 3. Carrega a cena
        SceneManager.LoadScene(sceneName);
    }

    // Faz fade na imagem (recomendado para não esconder o texto)
    private IEnumerator DoFadeImage(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null)
        {
            // fallback para CanvasGroup fade se imagem não estiver atribuída
            yield return StartCoroutine(DoFadeCanvasGroup(startAlpha, endAlpha, duration));
            yield break;
        }

        fadeImage.gameObject.SetActive(true);
        if (fadePanel != null) fadePanel.blocksRaycasts = true;

        float timer = 0f;
        Color c = fadeImage.color;
        // assegura cor inicial
        c.a = startAlpha;
        fadeImage.color = c;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;

        // Se terminou com alpha 0, desativa a imagem e permite interações
        if (endAlpha <= 0f)
        {
            fadeImage.gameObject.SetActive(false);
            if (fadePanel != null) fadePanel.blocksRaycasts = false;
        }
    }

    // Fallback: fade usando CanvasGroup (mantive para compatibilidade)
    private IEnumerator DoFadeCanvasGroup(float start, float end, float duration)
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        fadePanel.blocksRaycasts = true;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(start, end, timer / duration);
            yield return null;
        }

        fadePanel.alpha = end;

        if (end <= 0f)
        {
            fadePanel.gameObject.SetActive(false);
            fadePanel.blocksRaycasts = false;
        }
    }

    public void OpenSettings() { PlaySound(clickSound); mainMenuPanel.SetActive(false); settingsPanel.SetActive(true); }
    public void CloseSettings() { PlaySound(clickSound); settingsPanel.SetActive(false); mainMenuPanel.SetActive(true); }
    public void QuitGame() { PlaySound(clickSound); Application.Quit(); }
    public void OnButtonHover() => PlaySound(hoverSound);
    private void PlaySound(AudioClip clip) { if (clip != null && audioSource != null) audioSource.PlayOneShot(clip); }
}