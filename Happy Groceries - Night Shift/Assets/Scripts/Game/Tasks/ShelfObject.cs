using System.Collections;
using UnityEngine;

/// <summary>
/// Coloca este script em cada PRATELEIRA.
/// O GameObject deve ter um BoxCollider2D marcado como Is Trigger.
///
/// Setup dos itens:
///   1. Coloca os itens como filhos da prateleira na cena (posicionados onde queres)
///   2. Desativa cada item no Inspector (checkbox ao lado do nome, desmarcada)
///   3. Arrasta-os para o array "shelfItems" no Inspector, pela ordem que devem aparecer
/// </summary>
public class ShelfObject : MonoBehaviour
{
    [Header("=== Itens da Prateleira ===")]
    [Tooltip("Objetos JÁ EXISTENTES na cena (filhos da prateleira), desativados.\n" +
             "Serão ativados um por um quando o jogador reabastecer.")]
    public GameObject[] shelfItems;

    [Tooltip("Tempo em segundos entre a ativação de cada item.")]
    public float timeBetweenItems = 1.5f;

    [Header("=== Ícone de Interação ===")]
    [Tooltip("Filho do GameObject com o sprite do ícone. " +
             "Só aparece quando o jogador está na zona COM uma caixa.")]
    public GameObject interactionIcon;

    // ── Estado público ─────────────────────────────────────────────────────
    public bool IsRestocked { get; private set; } = false;
    public bool IsRestocking { get; private set; } = false;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        SetIconVisible(false);

        // Garante que todos os itens começam desativados
        if (shelfItems != null)
            foreach (var item in shelfItems)
                if (item != null) item.SetActive(false);
    }

    // ── Trigger ────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsRestocked || IsRestocking) return;

        PlayerInteraction player = other.GetComponentInParent<PlayerInteraction>();
        if (player == null) return;

        // Regista SEMPRE o jogador
        player.OnEnterShelf(this);

        // Só mostra ícone se tiver caixa
        if (player.IsHoldingBox)
            SetIconVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInteraction player = other.GetComponentInParent<PlayerInteraction>();
        if (player == null) return;

        player.OnExitShelf(this);
        SetIconVisible(false);
    }

    // ── Chamados pelo PlayerInteraction ───────────────────────────────────
    public void ShowIcon()
    {
        if (!IsRestocked && !IsRestocking)
            SetIconVisible(true);
    }

    public void HideIcon()
    {
        SetIconVisible(false);
    }

    // ── Início do reabastecer ──────────────────────────────────────────────
    public void StartRestocking(BoxObject box)
    {
        if (IsRestocked || IsRestocking) return;

        IsRestocking = true;
        SetIconVisible(false);

        if (shelfItems == null || shelfItems.Length == 0)
        {
            Debug.LogWarning($"[ShelfObject] '{name}' não tem shelfItems definidos!");
            FinishRestocking();
            return;
        }

        StartCoroutine(ActivateItemsRoutine());
    }

    // ── Ativa os itens um por um ───────────────────────────────────────────
    private IEnumerator ActivateItemsRoutine()
    {
        Debug.Log($"[Task1] Prateleira '{name}' a ser reabastecida...");

        for (int i = 0; i < shelfItems.Length; i++)
        {
            if (shelfItems[i] == null) continue;

            shelfItems[i].SetActive(true);
            StartCoroutine(AnimateItemAppear(shelfItems[i]));

            Debug.Log($"[Task1] Item {i + 1}/{shelfItems.Length} ativado em '{name}'");

            if (i < shelfItems.Length - 1)
                yield return new WaitForSeconds(timeBetweenItems);
        }

        FinishRestocking();
    }

    // ── Animação pop ao aparecer ───────────────────────────────────────────
    private IEnumerator AnimateItemAppear(GameObject item)
    {
        float duration = 0.4f;
        float elapsed = 0f;
        item.transform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            item.transform.localScale = Vector3.one * EaseOutBack(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        item.transform.localScale = Vector3.one;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // ── Fim do reabastecer ─────────────────────────────────────────────────
    private void FinishRestocking()
    {
        IsRestocking = false;
        IsRestocked = true;
        SetIconVisible(false);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log($"[Task1] ✔ Prateleira '{name}' completamente reabastecida!");

        Task1Manager manager = FindObjectOfType<Task1Manager>();
        if (manager != null)
            manager.OnShelfRestocked(this);
    }

    // ── Helper ────────────────────────────────────────────────────────────
    private void SetIconVisible(bool visible)
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(visible);
    }
}