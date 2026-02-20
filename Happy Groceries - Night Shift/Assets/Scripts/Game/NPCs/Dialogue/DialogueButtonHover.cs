using UnityEngine;
using UnityEngine.EventSystems; // Necessário para utilizar o sistema de eventos de UI (rato, toques, etc.)

// A classe implementa IPointerEnterHandler e IPointerExitHandler para detetar quando o rato entra e sai da área do botão na UI
public class DialogueButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // --- SETTINGS ---

    [Header("UI References")]
    public GameObject hoverSquare;

    void OnEnable()
    {
        // Garante que, sempre que o botão é ativado e aparece no ecrã, o indicador visual (quadrado de seleção) começa desligado
        if (hoverSquare != null)
        {
            hoverSquare.SetActive(false);
        }
    }

    // Função chamada automaticamente pelo EventSystem da Unity quando o ponteiro do rato ENTRA na área delimitada pelo botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ativa o indicador visual (efeito de Hover)
        if (hoverSquare != null)
        {
            hoverSquare.SetActive(true);
        }
    }

    // Função chamada automaticamente pelo EventSystem da Unity quando o ponteiro do rato SAI da área delimitada pelo botão
    public void OnPointerExit(PointerEventData eventData)
    {
        // Desativa o indicador visual
        if (hoverSquare != null)
        {
            hoverSquare.SetActive(false);
        }
    }

    void OnDisable()
    {
        // Medida de segurança: se o botão for desativado (ex: o jogador clicou numa opção e os botões sumiram),
        // garante que o indicador visual também é desligado para evitar que o quadrado fique "preso" na próxima vez que a UI abrir
        if (hoverSquare != null)
        {
            hoverSquare.SetActive(false);
        }
    }
}