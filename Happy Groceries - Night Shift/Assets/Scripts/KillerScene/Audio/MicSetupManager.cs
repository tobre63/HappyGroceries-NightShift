using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MicSetupManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup parentUIGroup; // O GameObject vazio que tem a SetupUI e a PersistentUI dentro
    public CanvasGroup setupUIGroup;  // Apenas as definições do microfone
    public Image blackScreen;

    [Header("Textos de Introdução")]
    public TMPro.TextMeshProUGUI introTextComponent; // Arrasta o teu ÚNICO texto para aqui (se usares TextMeshPro, troca 'Text' por 'TMPro.TextMeshProUGUI')
    [TextArea(2, 3)]
    public string[] introMessages; // Aqui no Inspector, adiciona os teus 6 textos
    public float textFadeDuration = 0.5f;
    public float textVisibleDuration = 3f;

    [Header("Configurações Finais")]
    public float fadeDuration = 2f;

    public static bool isSetupActive = false;
    private bool isShowingMicSetup = false;

    private void Start()
    {
        isSetupActive = true;
        isShowingMicSetup = false;

        Time.timeScale = 0f;
        AudioListener.volume = 0f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Ecrã preto totalmente visível
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 1);
            blackScreen.gameObject.SetActive(true);
        }

        // Esconde as UIs (Setup e Persistent)
        if (parentUIGroup != null)
        {
            parentUIGroup.alpha = 0f;
            parentUIGroup.gameObject.SetActive(true);
        }

        // Garante que o setupUI está a 1 para quando o parentUI aparecer, ele aparecer junto
        if (setupUIGroup != null)
        {
            setupUIGroup.alpha = 1f;
            setupUIGroup.interactable = false;
            setupUIGroup.blocksRaycasts = false;
        }

        // Deixa o texto transparente no início
        if (introTextComponent != null)
        {
            SetTextAlpha(0f);
            introTextComponent.gameObject.SetActive(true);
        }

        StartCoroutine(PlayIntroSequence());
    }

    private void Update()
    {
        if (isSetupActive && isShowingMicSetup)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        // 1. Mostrar cada texto um a um no mesmo componente
        if (introTextComponent != null && introMessages != null)
        {
            for (int i = 0; i < introMessages.Length; i++)
            {
                // Muda a frase
                introTextComponent.text = introMessages[i];

                // Fade In
                float timer = 0f;
                while (timer < textFadeDuration)
                {
                    timer += Time.unscaledDeltaTime;
                    SetTextAlpha(Mathf.Lerp(0f, 1f, timer / textFadeDuration));
                    yield return null;
                }
                SetTextAlpha(1f);

                // Espera visível
                yield return new WaitForSecondsRealtime(textVisibleDuration);

                // Fade Out
                timer = 0f;
                while (timer < textFadeDuration)
                {
                    timer += Time.unscaledDeltaTime;
                    SetTextAlpha(Mathf.Lerp(1f, 0f, timer / textFadeDuration));
                    yield return null;
                }
                SetTextAlpha(0f);

                // Pequena pausa entre textos (opcional)
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        // 2. Acabaram os textos. Mostra as UIs (Parent)
        isShowingMicSetup = true; // Permite que o rato apareça

        if (parentUIGroup != null)
        {
            // Fade in do ParentUIGroup
            float timer = 0f;
            while (timer < 0.5f)
            {
                timer += Time.unscaledDeltaTime;
                parentUIGroup.alpha = Mathf.Lerp(0f, 1f, timer / 0.5f);
                yield return null;
            }
            parentUIGroup.alpha = 1f;

            // Ativa os botões do SetupUI
            if (setupUIGroup != null)
            {
                setupUIGroup.interactable = true;
                setupUIGroup.blocksRaycasts = true;
            }
        }
    }

    public void OnSkipButtonClicked()
    {
        StartCoroutine(FadeOutAndStartGame());
    }

    private IEnumerator FadeOutAndStartGame()
    {
        setupUIGroup.interactable = false;
        setupUIGroup.blocksRaycasts = false;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float percent = timer / fadeDuration;

            // Fazemos Fade Out APENAS do SetupUI e do Ecrã Preto
            float alpha = 1f - percent;

            if (setupUIGroup != null)
                setupUIGroup.alpha = alpha;

            if (blackScreen != null)
                blackScreen.color = new Color(0, 0, 0, alpha);

            AudioListener.volume = percent; // Aumenta o som do jogo
            yield return null;
        }

        // Desativa a UI de Setup para não interferir
        if (setupUIGroup != null)
        {
            setupUIGroup.alpha = 0f;
            setupUIGroup.gameObject.SetActive(false);
        }

        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 0);
            blackScreen.gameObject.SetActive(false);
        }

        // O parentUIGroup fica a 1, logo a persistentUI fica vísivel no jogo!
        AudioListener.volume = 1f;
        StartNormalGame();
    }

    private void StartNormalGame()
    {
        isSetupActive = false;
        isShowingMicSetup = false;
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Função auxiliar para mudar a transparência do texto sem duplicar código
    private void SetTextAlpha(float alpha)
    {
        if (introTextComponent != null)
        {
            Color c = introTextComponent.color;
            c.a = alpha;
            introTextComponent.color = c;
        }
    }
}