using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 input;
    private Vector2 lastDirection = Vector2.down;

    [Header("Audio")]
    public AudioClip defaultFootstep;
    public AudioClip carpetFootstep;
    public AudioClip tilesFootstep;

    [Header("Ground Detection")]
    public LayerMask floorLayer;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        bool isBlocked = NPCInteraction.isPlayerTalking
               || BoxInteractable.isPickingUpBox
               || ShelfInteractable.isPlacingBox
               || MopInteractable.isInteractingWithMop
               || DirtZoneInteractable.isCleaningDirt
               || BottleInteractable.isPickingUpBottle
               || CocaColaSpillEvent.isSpillEventActive;

        if (isBlocked)
        {
            input = Vector2.zero; // Garante que o FixedUpdate também para
            anim.speed = 1;
            anim.SetBool("isMoving", false);
            anim.SetFloat("moveX", lastDirection.x); // Mantém a direção que estava
            anim.SetFloat("moveY", lastDirection.y);
            return;
        }

        anim.speed = 1;

        var k = Keyboard.current;

        if (k == null)
        {
            input = Vector2.zero;
            return;
        }

        bool pressLeft = k.aKey.isPressed;
        bool pressRight = k.dKey.isPressed;
        bool pressUp = k.wKey.isPressed;
        bool pressDown = k.sKey.isPressed;

        if (input.x != 0 && ((input.x < 0 && pressLeft) || (input.x > 0 && pressRight)))
        {
            input.y = 0;
        }
        else if (input.y != 0 && ((input.y < 0 && pressDown) || (input.y > 0 && pressUp)))
        {
            input.x = 0;
        }
        else
        {
            input = Vector2.zero;

            if (pressLeft) input.x = -1;
            else if (pressRight) input.x = 1;
            else if (pressDown) input.y = -1;
            else if (pressUp) input.y = 1;
        }

        bool isMoving = input != Vector2.zero;
        anim.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            lastDirection = input;
            anim.SetFloat("moveX", input.x);
            anim.SetFloat("moveY", input.y);
        }
        else
        {
            anim.SetFloat("moveX", lastDirection.x);
            anim.SetFloat("moveY", lastDirection.y);
        }
    }

    void FixedUpdate()
    {
        // Como input é zerado no Update quando bloqueado, isto para automaticamente
        rb.linearVelocity = input * speed;
    }

    public void PlayFootstep()
    {
        if (input != Vector2.zero)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.2f, floorLayer);

            AudioClip clipToPlay = defaultFootstep;

            if (hit != null)
            {
                if (hit.CompareTag("Carpet"))
                    clipToPlay = carpetFootstep;
                else if (hit.CompareTag("Tiles"))
                    clipToPlay = tilesFootstep;
            }

            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}