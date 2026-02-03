using UnityEngine;

public class SeeThrough : MonoBehaviour
{
    [Header("Configurations")]
    [Range(0, 1)]
    public float transparency = 0.3f;     // 0 e invisivel, 1 e opaco
    public float fadeSpeed = 5f;          // Velocidade da transicao

    private SpriteRenderer spriteRend;
    private float targetAlpha = 1f;

    void Awake()
    {
        // Obtem o SpriteRenderer do objeto
        spriteRend = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (spriteRend == null) return;

        // Calcula o novo alpha de forma suave
        Color color = spriteRend.color;
        float newAlpha = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * fadeSpeed);

        // Aplica a nova cor com o alpha atualizado
        spriteRend.color = new Color(color.r, color.g, color.b, newAlpha);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ativa apenas se o objeto for o Player
        if (collision.CompareTag("Player"))
        {
            targetAlpha = transparency;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Volta ao alpha normal quando o Player sai
        if (collision.CompareTag("Player"))
        {
            targetAlpha = 1f;
        }
    }
}
