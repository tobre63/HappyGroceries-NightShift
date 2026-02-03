using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    public CanvasGroup fadePanel;   // Painel usado para o efeito de fade
    public float fadeDuration = 2.0f; // Duracao do fade em segundos

    void Start()
    {
        if (fadePanel != null)
        {
            // Ativa o objeto caso esteja desativado no Inspector
            if (!fadePanel.gameObject.activeSelf)
            {
                fadePanel.gameObject.SetActive(true);
            }

            // Comeca totalmente opaco e a bloquear interacoes
            fadePanel.alpha = 1;
            fadePanel.blocksRaycasts = true;

            StartCoroutine(DoFadeOut());
        }
        else
        {
            Debug.LogWarning("FadePanel nao foi atribuido no script SceneFadeIn");
        }
    }

    IEnumerator DoFadeOut()
    {
        float timer = 0;

        // Reduz a opacidade gradualmente ate ficar transparente
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }

        // Deixa de bloquear interacoes e desativa o painel
        fadePanel.blocksRaycasts = false;
        fadePanel.gameObject.SetActive(false);
    }
}
