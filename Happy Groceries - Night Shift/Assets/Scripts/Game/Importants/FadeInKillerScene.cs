using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInKillerScene : MonoBehaviour
{
    [Header("UI")]
    public Image blackScreen; // Arrasta a imagem preta para aqui

    [Header("Configurações")]
    public float waitBeforeFade = 3f; // Tempo que o ecrã fica 100% preto (para leres o texto)
    public float fadeDuration = 2f;   // Tempo que demora a clarear

    private void Start()
    {
        // Garante que a cena começa com o ecrã totalmente preto
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 1);
            blackScreen.gameObject.SetActive(true);
            StartCoroutine(FadeFromBlack());
        }
    }

    private IEnumerator FadeFromBlack()
    {
        // 1. Espera o tempo definido (onde o teu texto vai estar visível)
        yield return new WaitForSeconds(waitBeforeFade);

        // 2. Começa a clarear o ecrã
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // Calcula a transparência (de 1 para 0)
            float alpha = 1f - (timer / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 3. Fica 100% transparente e desativa para não bloquear o jogo
        blackScreen.color = new Color(0, 0, 0, 0);
        blackScreen.gameObject.SetActive(false);
    }
}