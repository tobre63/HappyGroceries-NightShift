using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Screens & Fade")]
    [SerializeField] private GameObject mainMenuPanel; // O painel principal do menu
    [SerializeField] private GameObject settingsPanel; // O painel de opções
    [SerializeField] private CanvasGroup fadePanel;    // O painel preto usado para transições (Fade In/Out)

    [Header("Elements to Hide on Transition")]
    [SerializeField] private GameObject buttonsPanel;    // O grupo que contém os botões (para ser desativado)
    [SerializeField] private GameObject backgroundPanel; // A imagem de fundo (para ser desativada)

    [Header("Story Text Settings")]
    [SerializeField] private TextMeshProUGUI loadingText; // O componente de texto onde a história aparece
    [TextArea(8, 15)]
    [SerializeField] private string fullText;             // O texto completo que será escrito letra por letra
    [SerializeField] private float typingSpeed = 0.05f;   // Velocidade de escrita de cada letra
    [SerializeField] private float waitAfterText = 2.0f;  // Tempo de espera após o texto terminar de escrever

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; // Fonte para Efeitos Sonoros (SFX)
    [SerializeField] private AudioSource musicSource; // Fonte para a Música de Fundo (BGM)
    [SerializeField] private AudioClip typeSound;     // Som de "teclar"
    [SerializeField] private AudioClip hoverSound;    // Som ao passar o rato
    [SerializeField] private AudioClip clickSound;    // Som ao clicar

    private CanvasGroup loadingTextGroup; // Controla a transparência do texto da história
    private bool isTransitioning = false; // Flag de segurança para impedir cliques durante a animação

    private void Awake()
    {
        // Inicialização: Garante que o menu principal está visível e as opções escondidas
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadingText != null) loadingText.text = "";

        // Adiciona ou obtém um CanvasGroup no texto para podermos fazer Fade In/Out apenas nas letras
        if (loadingText != null)
        {
            loadingTextGroup = loadingText.GetComponent<CanvasGroup>();
            if (loadingTextGroup == null)
                loadingTextGroup = loadingText.gameObject.AddComponent<CanvasGroup>();

            loadingTextGroup.alpha = 1f;
            loadingTextGroup.blocksRaycasts = false; // Impede que o texto bloqueie cliques do rato
        }

        // Prepara o painel de fade (começa preto para fazer o fade-in no Start)
        if (fadePanel != null)
        {
            fadePanel.alpha = 1;
            fadePanel.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // Ao iniciar a cena, faz o painel preto desaparecer (Fade In da imagem do menu)
        StartCoroutine(DoFade(1, 0, 1.5f));
    }

    // Chamado pelo botão "Start" no Unity
    public void StartGame(string sceneName)
    {
        // Segurança: Se já estiver a transitar ou o nome da cena for inválido, ignora o clique
        if (isTransitioning || string.IsNullOrEmpty(sceneName)) return;

        isTransitioning = true; // Ativa o bloqueio de inputs
        PlaySound(clickSound);

        // Inicia a sequência cinematográfica de entrada no jogo
        StartCoroutine(SequenceWithDiary(sceneName));
    }

    // A Corrotina principal que gere toda a sequência de entrada
    private IEnumerator SequenceWithDiary(string sceneName)
    {
        // 1. Começa a baixar o volume da música suavemente
        if (musicSource != null)
        {
            StartCoroutine(FadeMusic(musicSource, 0f, 1.5f));
        }

        // 2. Escurece o ecrã (Fade Out visual para preto)
        yield return StartCoroutine(DoFade(0, 1, 1.5f));

        // 3. Oculta os elementos do menu (Botões e Fundo) agora que o ecrã está preto
        // Isto garante que o jogador foca apenas no texto que vai aparecer
        if (buttonsPanel != null) buttonsPanel.SetActive(false);
        if (backgroundPanel != null) backgroundPanel.SetActive(false);

        // 4. Efeito de Máquina de Escrever
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "";

            // Escreve letra a letra
            foreach (char letter in fullText.ToCharArray())
            {
                loadingText.text += letter;
                // Toca o som de teclar a cada letra
                if (typeSound != null && audioSource != null) audioSource.PlayOneShot(typeSound);
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // 5. Tempo de leitura (espera um pouco com o texto completo no ecrã)
        yield return new WaitForSeconds(waitAfterText);

        // 6. Faz o texto desaparecer suavemente (Fade Out do texto)
        if (loadingTextGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(loadingTextGroup, 1f, 0f, 1.0f));
            loadingText.gameObject.SetActive(false);
        }

        // 7. Carrega a próxima cena do jogo
        SceneManager.LoadScene(sceneName);
    }

    // --- Helper Coroutines (Utilitários) ---

    // Controla o Fade do painel preto (Alpha de 0 a 1 ou vice-versa)
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

        // Se o alpha for 0 (transparente), desativa o objeto para não bloquear cliques
        if (end <= 0) fadePanel.gameObject.SetActive(false);
    }

    // Controla o Fade do volume da música
    private IEnumerator FadeMusic(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    // Controla o Fade de qualquer elemento com CanvasGroup (usado no texto)
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
        if (end <= 0f) cg.gameObject.SetActive(false);
    }

    // --- UI & Audio Methods ---

    public void OpenSettings()
    {
        if (isTransitioning) return; // Não permite abrir se o jogo estiver a iniciar
        PlaySound(clickSound);
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (isTransitioning) return;
        PlaySound(clickSound);
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        if (isTransitioning) return;
        PlaySound(clickSound);
        Application.Quit();
    }

    public void OnButtonHover()
    {
        if (isTransitioning) return;
        PlaySound(hoverSound);
    }

    // Método auxiliar para tocar sons apenas se o clip existir
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }
}