using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Coloca este script no GameObject do JOGADOR.
/// Gere o input (segurar E), o pickup de caixas e a entrega na prateleira.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("=== Caixas no Armazém ===")]
    public BoxObject box1;
    public BoxObject box2;

    [Header("=== Prateleiras ===")]
    public ShelfObject shelf1;
    public ShelfObject shelf2;

    [Header("=== UI (opcional) ===")]
    [Tooltip("Slider que mostra o progresso de segurar E (pode ser null)")]
    public Slider progressBar;
    [Tooltip("Texto que aparece quando há uma ação disponível (pode ser null)")]
    public TextMeshProUGUI actionLabel;

    // ── Tempos ──────────────────────────────────────────────────────────
    private const float PICKUP_TIME = 5f;
    private const float PLACE_TIME = 15f;

    // ── Estado interno ───────────────────────────────────────────────────
    private BoxObject nearBox;
    private ShelfObject nearShelf;
    private BoxObject heldBox;

    private float holdTimer = 0f;

    // ─────────────────────────────────────────────────────────────────────
    private void Update()
    {
        UpdateActionLabel();

        if (Input.GetKey(KeyCode.E))
        {
            bool canPickup = heldBox == null && nearBox != null;
            bool canPlace = heldBox != null && nearShelf != null && !nearShelf.IsRestocking;

            if (canPickup)
            {
                holdTimer += Time.deltaTime;
                SetProgress(holdTimer / PICKUP_TIME);

                if (holdTimer >= PICKUP_TIME)
                    PickupBox(nearBox);
            }
            else if (canPlace)
            {
                holdTimer += Time.deltaTime;
                SetProgress(holdTimer / PLACE_TIME);

                if (holdTimer >= PLACE_TIME)
                    PlaceOnShelf(nearShelf);
            }
            else
            {
                ResetTimer();
            }
        }
        else
        {
            ResetTimer();
        }
    }

    // ── Pickup ────────────────────────────────────────────────────────────
    private void PickupBox(BoxObject box)
    {
        heldBox = box;
        box.OnPickedUp();
        nearBox = null;
        ResetTimer();

        // Se o jogador já está dentro de uma zona de prateleira,
        // mostra o ícone da prateleira agora que tem a caixa
        if (nearShelf != null && !nearShelf.IsRestocking && !nearShelf.IsRestocked)
            nearShelf.ShowIcon();

        Debug.Log($"[Task1] Caixa '{box.name}' apanhada!");
    }

    // ── Colocar na Prateleira ─────────────────────────────────────────────
    private void PlaceOnShelf(ShelfObject shelf)
    {
        shelf.StartRestocking(heldBox);
        Debug.Log($"[Task1] Caixa '{heldBox.name}' entregue na prateleira '{shelf.name}'!");
        heldBox = null;
        nearShelf = null;
        ResetTimer();
    }

    // ── Chamados pelos BoxObject / ShelfObject via OnTrigger ──────────────
    public void OnEnterBox(BoxObject box)
    {
        nearBox = box;
    }

    public void OnExitBox(BoxObject box)
    {
        if (nearBox == box) nearBox = null;
        ResetTimer();
    }

    public void OnEnterShelf(ShelfObject shelf)
    {
        nearShelf = shelf;
    }

    public void OnExitShelf(ShelfObject shelf)
    {
        if (nearShelf == shelf) nearShelf = null;
        ResetTimer();
    }

    // ── Helpers de UI ─────────────────────────────────────────────────────
    private void ResetTimer()
    {
        holdTimer = 0f;
        SetProgress(0f);
    }

    private void SetProgress(float value)
    {
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(value > 0f);
            progressBar.value = Mathf.Clamp01(value);
        }
    }

    private void UpdateActionLabel()
    {
        if (actionLabel == null) return;

        if (heldBox == null && nearBox != null)
            actionLabel.text = "Segura [E] para apanhar a caixa (5s)";
        else if (heldBox != null && nearShelf != null && !nearShelf.IsRestocking)
            actionLabel.text = "Segura [E] para reabastecer a prateleira (15s)";
        else if (heldBox != null)
            actionLabel.text = $"A carregar: {heldBox.name}";
        else
            actionLabel.text = "";
    }

    // ── Getter público ────────────────────────────────────────────────────
    public bool IsHoldingBox => heldBox != null;
}