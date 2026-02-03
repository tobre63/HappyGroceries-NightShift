using UnityEngine;
using UnityEngine.InputSystem;

// Garante que o GameObject tem sempre estes componentes essenciais anexados
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class PlayerController : MonoBehaviour
{
    // Velocidade de movimento do jogador
    public float speed = 5f;

    // Referencias para os componentes principais
    private Rigidbody2D rb;
    private Animator anim;

    // Direcao atual do input, limitada a 4 direcoes
    private Vector2 input;

    // Ultima direcao valida, usada para manter a orientacao quando parado
    private Vector2 lastDirection = Vector2.down;

    [Header("Audio")]
    public AudioClip defaultFootstep;
    public AudioClip carpetFootstep; // Som para o GameObject "Carpet"
    public AudioClip tilesFootstep;  // Som para o GameObject "Tiles"

    [Header("Ground Detection")]
    public LayerMask floorLayer;
    private AudioSource audioSource;

    // Chamado quando o script e carregado
    void Awake()
    {
        // Obtem referencias aos componentes obrigatorios
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Configuracao tipica para jogos top down
        rb.gravityScale = 0f;     // Remove gravidade
        rb.freezeRotation = true; // Impede rotacao por fisica
        audioSource.playOnAwake = false;
    }

    // Chamado a cada frame, ideal para input e animacoes
    void Update()
    {
        // Acede ao teclado atual atraves do Input System
        var k = Keyboard.current;

        // Se nao existir teclado, cancela movimento
        if (k == null)
        {
            input = Vector2.zero;
            return;
        }

        // Leitura do estado das teclas
        bool pressLeft = k.aKey.isPressed;
        bool pressRight = k.dKey.isPressed;
        bool pressUp = k.wKey.isPressed;
        bool pressDown = k.sKey.isPressed;

        // Logica de bloqueio horizontal
        if (input.x != 0 && ((input.x < 0 && pressLeft) || (input.x > 0 && pressRight)))
        {
            // Mantem movimento horizontal e bloqueia vertical
            input.y = 0;
        }
        // Logica de bloqueio vertical
        else if (input.y != 0 && ((input.y < 0 && pressDown) || (input.y > 0 && pressUp)))
        {
            // Mantem movimento vertical e bloqueia horizontal
            input.x = 0;
        }
        else
        {
            // Novo input quando nao ha direcao bloqueada
            input = Vector2.zero;

            // Define nova direcao com base na tecla premida
            if (pressLeft) input.x = -1;
            else if (pressRight) input.x = 1;
            else if (pressDown) input.y = -1;
            else if (pressUp) input.y = 1;
        }

        // Verifica se existe movimento
        bool isMoving = input != Vector2.zero;

        // Atualiza parametro de movimento no Animator
        anim.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            // Guarda ultima direcao valida
            lastDirection = input;

            // Envia direcao atual para a Blend Tree
            anim.SetFloat("moveX", input.x);
            anim.SetFloat("moveY", input.y);
        }
        else
        {
            // Mantem direcao anterior quando parado
            anim.SetFloat("moveX", lastDirection.x);
            anim.SetFloat("moveY", lastDirection.y);
        }
    }

    // Chamado em intervalos fixos, ideal para fisica
    void FixedUpdate()
    {
        // Aplica velocidade ao Rigidbody para mover o personagem
        rb.linearVelocity = input * speed;
    }

    public void PlayFootstep()
    {
        // Só toca som se o jogador estiver em movimento
        if (input != Vector2.zero)
        {
            // Deteta o objeto que esta debaixo do jogador
            // Usamos um pequeno circulo na posição do jogador
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.2f, floorLayer);

            AudioClip clipToPlay = defaultFootstep; // Comeca com o som padrao

            if (hit != null)
            {
                // Verifica as Tags do objeto em que estamos a pisar
                if (hit.CompareTag("Carpet"))
                {
                    clipToPlay = carpetFootstep;
                }
                else if (hit.CompareTag("Tiles"))
                {
                    clipToPlay = tilesFootstep;
                }
            }

            // Reproduz o som escolhido
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clipToPlay);
        }
    }


}