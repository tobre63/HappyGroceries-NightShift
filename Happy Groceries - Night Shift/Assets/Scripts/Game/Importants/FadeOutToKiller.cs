using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeOutToKiller : MonoBehaviour
{
    [Header("UI")]
    public Image blackScreen; // Arrasta a imagem preta do Canvas para aqui

    [Header("Configurações Visuais")]
    public float flashDuration = 0.15f; // Tempo do clarão branco
    public float fadeDuration = 1.5f;   // Tempo para ir de branco a preto

    [Header("Áudio da Flashbang")]
    public AudioSource flashbangAudio;  // Arrasta o AudioSource com o som para aqui
    public float maxVolume = 1f;        // Volume máximo do impacto

    private void Start()
    {
        // Garante que a imagem começa invisível/transparente no jogo normal
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 0);
            blackScreen.gameObject.SetActive(false);
        }
    }

    public void IniciarFadeE_MudarCena(string sceneName)
    {
        StartCoroutine(FlashbangEFade(sceneName));
    }

    private IEnumerator FlashbangEFade(string sceneName)
    {
        blackScreen.gameObject.SetActive(true);

        // 1. IMPACTO DA FLASHBANG (Ecrã Branco + Som no máximo)
        blackScreen.color = Color.white;

        if (flashbangAudio != null)
        {
            flashbangAudio.volume = maxVolume;
            flashbangAudio.Play();
        }

        // Espera a fração de segundo do clarão inicial
        yield return new WaitForSeconds(flashDuration);

        // 2. FADE OUT (Ecrã escurece + Volume do som diminui)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Visual: Transita de Branco para Preto
            blackScreen.color = Color.Lerp(Color.white, Color.black, progress);

            // Áudio: O volume desce suavemente do máximo até zero
            if (flashbangAudio != null)
            {
                flashbangAudio.volume = Mathf.Lerp(maxVolume, 0f, progress);
            }

            yield return null;
        }

        // 3. GARANTIAS FINAIS (Tudo escuro e som a zeros)
        blackScreen.color = Color.black;
        if (flashbangAudio != null)
        {
            flashbangAudio.volume = 0f;
            flashbangAudio.Stop(); // Pára o som totalmente para não ficar a tocar em silêncio
        }

        // Pausa extra para a transição não ser tão brusca
        yield return new WaitForSeconds(0.2f);

        // Muda de cena
        SceneManager.LoadScene(sceneName);
    }
}