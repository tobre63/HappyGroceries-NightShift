using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MicSetupManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup setupUIGroup;
    public Image blackScreen;

    [Header("Configurações")]
    public float fadeDuration = 2f;

    // Variável global para os teus outros scripts saberem que estamos no menu!
    public static bool isSetupActive = false;

    private void Start()
    {
        isSetupActive = true;

        // Congela física e animações
        Time.timeScale = 0f;
        AudioListener.volume = 0f; // Muta o jogo

        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 1);
            blackScreen.gameObject.SetActive(true);
        }

        if (setupUIGroup != null)
        {
            setupUIGroup.alpha = 1f;
            setupUIGroup.interactable = true;
            setupUIGroup.blocksRaycasts = true;
        }
    }

    private void Update()
    {
        // O SEGREDO ESTÁ AQUI: Enquanto o setup estiver ativo, obriga o rato a aparecer!
        // Isto impede o GameManager de esconder o rato no início da cena.
        if (isSetupActive)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
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

            float alpha = 1f - percent;
            setupUIGroup.alpha = alpha;

            if (blackScreen != null)
                blackScreen.color = new Color(0, 0, 0, alpha);

            AudioListener.volume = percent;
            yield return null;
        }

        setupUIGroup.alpha = 0f;
        setupUIGroup.gameObject.SetActive(false);

        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 0);
            blackScreen.gameObject.SetActive(false);
        }

        AudioListener.volume = 1f;
        StartNormalGame();
    }

    private void StartNormalGame()
    {
        isSetupActive = false;
        Time.timeScale = 1f;

        // Esconde o rato quando fores jogar
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}