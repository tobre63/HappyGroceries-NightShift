using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems; // Necessário para detetar o clique do rato!

public class SliderToggle : MonoBehaviour, IPointerClickHandler
{
    [Header("Configurações Base")]
    public Slider toggleSlider;
    public bool isOn = false; // Estado inicial do botão (Ligado ou Desligado)

    [Header("Animação")]
    public float animationDuration = 0.2f; // Tempo que demora a deslizar de 0 a 1

    [Header("Eventos (O que acontece quando clicas)")]
    public UnityEvent onToggleOn;  // Executa quando fica TRUE
    public UnityEvent onToggleOff; // Executa quando fica FALSE

    private Coroutine animateCoroutine;

    void Start()
    {
        // Garante que o slider começa na posição certa imediatamente, sem animação
        if (toggleSlider != null)
        {
            toggleSlider.value = isOn ? 1f : 0f;

            // MUITO IMPORTANTE: Desligar a interação padrão do slider 
            // para o jogador não conseguir arrastar a bolinha com o rato, apenas clicar!
            toggleSlider.interactable = false;
        }
    }

    // Esta função é chamada automaticamente quando clicas no objeto com o rato
    public void OnPointerClick(PointerEventData eventData)
    {
        if (toggleSlider == null) return;

        // Inverte o estado (Se estava ligado, desliga. Se estava desligado, liga)
        isOn = !isOn;

        // Chama o evento correspondente para ligar/desligar coisas no teu jogo (ex: Fullscreen)
        if (isOn)
        {
            onToggleOn.Invoke();
        }
        else
        {
            onToggleOff.Invoke();
        }

        // Pára a animação anterior (se o jogador clicar bué rápido) e começa uma nova
        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
        }

        float targetValue = isOn ? 1f : 0f;
        animateCoroutine = StartCoroutine(AnimateSlider(targetValue));

        // OPCIONAL: Se tiveres o GameManager com som, podes chamar aqui o som do clique!
        // if (GameManager.Instance != null) GameManager.Instance.PlayClickSound();
    }

    // A "Mágica" que faz a bolinha deslizar suavemente
    private IEnumerator AnimateSlider(float targetValue)
    {
        float startValue = toggleSlider.value;
        float timeElapsed = 0f;

        while (timeElapsed < animationDuration)
        {
            // unscaledDeltaTime permite que a animação funcione mesmo no Menu de Pausa!
            timeElapsed += Time.unscaledDeltaTime;

            // Lerp faz a transição linear suave entre o valor atual e o destino
            toggleSlider.value = Mathf.Lerp(startValue, targetValue, timeElapsed / animationDuration);

            yield return null;
        }

        // Garante que no fim bate certinho no 0 ou no 1
        toggleSlider.value = targetValue;
    }

    public void SetStateWithoutNotify(bool state)
    {
        isOn = state;
        if (toggleSlider != null)
        {
            toggleSlider.value = isOn ? 1f : 0f;
        }
    }
}