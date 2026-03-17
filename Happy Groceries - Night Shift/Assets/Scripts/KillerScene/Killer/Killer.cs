using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Killer : MonoBehaviour
{
    private Animator anim;
    private Transform playerTransform;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Se tivermos um alvo (jogador), viramos para ele
        if (playerTransform != null)
        {
            FacePlayer();
        }
    }

    // Método chamado pelo script filho quando o jogador entra
    public void SetPlayerTarget(Transform player)
    {
        playerTransform = player;
    }

    // Método chamado pelo script filho quando o jogador sai
    public void ClearPlayerTarget()
    {
        playerTransform = null;
    }

    private void FacePlayer()
    {
        // Calcula a direção do jogador em relação ao assassino principal
        Vector2 direction = playerTransform.position - transform.position;

        float moveX = 0f;
        float moveY = 0f;

        // Verifica se a maior distância é no eixo X (esquerda/direita) ou Y (cima/baixo)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            moveX = direction.x > 0 ? 1f : -1f;
        }
        else
        {
            moveY = direction.y > 0 ? 1f : -1f;
        }

        // Atualiza o Animator
        anim.SetFloat("moveX", moveX);
        anim.SetFloat("moveY", moveY);
    }
}