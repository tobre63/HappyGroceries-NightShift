using UnityEngine;

/// <summary>
/// Coloca este script em cada CAIXA no armazém.
/// O GameObject deve ter um BoxCollider2D marcado como Is Trigger.
///
/// Setup do ícone:
///   1. Cria um GameObject FILHO da caixa (ex: "InteractionIcon")
///   2. Adiciona um SpriteRenderer com o sprite do ícone (ex: tecla E, ícone de mão, etc.)
///   3. Arrasta esse filho para o campo "interactionIcon" no Inspector
/// </summary>
public class BoxObject : MonoBehaviour
{
    [Header("=== Visual da Caixa ===")]
    [Tooltip("Sprite/visual da caixa. Normalmente o próprio GameObject ou um filho com SpriteRenderer.")]
    public GameObject visualObject;

    [Header("=== Ícone de Interação ===")]
    [Tooltip("Filho do GameObject com o sprite do ícone (ex: tecla E ou ícone de mão). " +
             "Aparece quando o jogador entra na zona da caixa.")]
    public GameObject interactionIcon;

    private bool isPickedUp = false;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        SetIconVisible(false);
    }

    // ── Trigger ────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp) return;

        PlayerInteraction player = other.GetComponent<PlayerInteraction>();
        if (player != null)
        {
            player.OnEnterBox(this);
            SetIconVisible(true);   // mostra ícone ao entrar na zona
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInteraction player = other.GetComponent<PlayerInteraction>();
        if (player != null)
        {
            player.OnExitBox(this);
            SetIconVisible(false);  // esconde ícone ao sair da zona
        }
    }

    // ── Chamado pelo PlayerInteraction ─────────────────────────────────────
    public void OnPickedUp()
    {
        isPickedUp = true;
        SetIconVisible(false);  // esconde imediatamente ao apanhar

        if (visualObject != null)
            visualObject.SetActive(false);
        else
            gameObject.SetActive(false);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    // ── Repor caixa (ex: falha na tarefa) ─────────────────────────────────
    public void ResetBox()
    {
        isPickedUp = false;
        SetIconVisible(false);

        if (visualObject != null)
            visualObject.SetActive(true);
        else
            gameObject.SetActive(true);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    // ── Helper ────────────────────────────────────────────────────────────
    private void SetIconVisible(bool visible)
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(visible);
    }
}