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
    [SerializeField] private CanvasGroup fadePanel; // Objeto 'Transition'

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
        }

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (fadePanel != null)
        {
            fadePanel.alpha = 1;
            fadePanel.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // Fade inicial para revelar o menu
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
        // 1. Escurece a tela
        yield return StartCoroutine(DoFade(0, 1, 1.0f));

        // 2. Garante que o texto esteja ativo e visível
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "";

            // Força o texto a ir para a frente de todos os outros filhos do objeto Transition
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

        // REGRA: Só desativa se o Alpha for 0 (tela clara)
        if (end <= 0) fadePanel.gameObject.SetActive(false);
    }

    public void OpenSettings() { PlaySound(clickSound); mainMenuPanel.SetActive(false); settingsPanel.SetActive(true); }
    public void CloseSettings() { PlaySound(clickSound); settingsPanel.SetActive(false); mainMenuPanel.SetActive(true); }
    public void QuitGame() { PlaySound(clickSound); Application.Quit(); }
    public void OnButtonHover() => PlaySound(hoverSound);
    private void PlaySound(AudioClip clip) { if (clip != null && audioSource != null) audioSource.PlayOneShot(clip); }
}