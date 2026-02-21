using UnityEngine;
using UnityEngine.EventSystems;

// Este script deteta o rato automaticamente sem precisares de configurar nada no botão
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Quando o rato passa por cima, avisa o GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayHoverSound();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Quando o rato clica, avisa o GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayClickSound();
        }
    }
}