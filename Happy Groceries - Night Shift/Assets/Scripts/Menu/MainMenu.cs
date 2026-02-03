using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject mainMenu;      // Painel principal (Start/Settings/Exit)
    public GameObject settingsMenu;  // Painel de Settings

    [Header("Lore & Fade Settings")]
    public CanvasGroup fadePanel;    // O painel preto com Canvas Group
    public TextMeshProUGUI loreText;
    public AudioSource menuMusic;
    public AudioSource sfxSource;
    public AudioClip typingSound;

    public float fadeDuration = 2f; // Duração do efeito de fade
    public float typingSpeed = 0.05f;

    [TextArea(5, 10)]
    public string fullLoreMessage;   // Escreve a tua história aqui no Inspector

    void Start()
    {
        // Garante que o menu principal esteja ativo e o de settings escondido
        if (mainMenu != null) mainMenu.SetActive(true);
        if (settingsMenu != null) settingsMenu.SetActive(false);

        if (fadePanel != null)
        {
            fadePanel.alpha = 0;
            fadePanel.gameObject.SetActive(false);
        }

        if (loreText != null) loreText.text = "";

        // Tempo do jogo normal
        Time.timeScale = 1f;

        // Cursor visível e livre no menu principal
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Botão para iniciar o jogo
    public void PlayGame()
    {
        StartCoroutine(LoreSequence());

        IEnumerator LoreSequence()
        {
            fadePanel.gameObject.SetActive(true);
            float startVolume = menuMusic != null ? menuMusic.volume : 0;

            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeDuration;

                fadePanel.alpha = Mathf.Lerp(0, 1, progress);
                if (menuMusic != null) menuMusic.volume = Mathf.Lerp(startVolume, 0, progress);

                yield return null;
            }

            if (menuMusic != null) menuMusic.Stop();

            loreText.text = "";
            if (!string.IsNullOrEmpty(fullLoreMessage))
            {
                foreach (char letter in fullLoreMessage.ToCharArray())
                {
                    loreText.text += letter;

                    // Toca o som se ele existir e a letra não for um espaço
                    if (sfxSource != null && typingSound != null && letter != ' ')
                    {
                        sfxSource.PlayOneShot(typingSound);
                    }

                    yield return new WaitForSeconds(typingSpeed);
                }
            }

            //tempo extra para ler a última frase
            yield return new WaitForSeconds(3f);

            SceneManager.LoadSceneAsync(1);
        }
    }

    // Abre o menu de settings
    public void OpenSettings()
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (settingsMenu != null) settingsMenu.SetActive(true);
    }

    // Fecha o menu de settings e volta ao menu principal
    public void CloseSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(true);
    }

    // Botão para sair do jogo
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}