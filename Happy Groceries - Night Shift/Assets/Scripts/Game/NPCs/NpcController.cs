using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class NpcController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource audioSource;

    private Vector2 movementInput; // Agora controlado externamente
    private Vector2 lastDirection = Vector2.down;

    [Header("Audio")]
    public AudioClip defaultFootstep;
    public AudioClip carpetFootstep;
    public AudioClip tilesFootstep;

    [Header("Ground Detection")]
    public LayerMask floorLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        audioSource.playOnAwake = false;
    }

    // Método para o WaypointMover enviar a direção
    public void SetInput(Vector2 direction)
    {
        movementInput = direction;
    }

    void Update()
    {
        bool isMoving = movementInput.magnitude > 0.1f;
        anim.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            lastDirection = movementInput.normalized;
            anim.SetFloat("moveX", lastDirection.x);
            anim.SetFloat("moveY", lastDirection.y);
        }
        else
        {
            anim.SetFloat("moveX", lastDirection.x);
            anim.SetFloat("moveY", lastDirection.y);
        }
    }

    void FixedUpdate()
    {
        // O movimento físico agora é baseado no input recebido
        rb.linearVelocity = movementInput * speed;
    }

    public void PlayFootstep()
    {
        if (movementInput.magnitude > 0.1f)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.2f, floorLayer);
            AudioClip clipToPlay = defaultFootstep;

            if (hit != null)
            {
                if (hit.CompareTag("Carpet")) clipToPlay = carpetFootstep;
                else if (hit.CompareTag("Tiles")) clipToPlay = tilesFootstep;
            }

            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}