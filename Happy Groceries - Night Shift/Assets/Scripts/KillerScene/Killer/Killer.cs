using UnityEngine;

// RequireComponent garante que o Unity adiciona automaticamente o Animator e o AudioSource
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class Killer : MonoBehaviour
{
    private Animator anim;
    private Transform playerTransform;
    private AudioSource audioSource; // Referência para o emissor de som

    [Header("Configurações de Patrulha")]
    public float speed = 2f;
    public float waitTime = 5f;

    [Header("Limites da Área de Patrulha")]
    public float minX = -2.85f;
    public float maxX = 2.85f;
    public float minY = -5.25f;
    public float maxY = -1f;

    [Header("Áudio")]
    [Tooltip("Arrasta o teu som de passo agressivo para aqui")]
    public AudioClip footstepSound;

    private Vector2 targetPosition;
    private float waitTimer;
    private bool isMovingX = true;

    private enum State { Waiting, Moving, PlayerDetected }
    private State currentState;

    void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // Vai buscar o AudioSource

        targetPosition = transform.position;
        waitTimer = waitTime;
        currentState = State.Waiting;
    }

    void Update()
    {
        if (playerTransform != null)
        {
            currentState = State.PlayerDetected;
        }
        else if (currentState == State.PlayerDetected && playerTransform == null)
        {
            currentState = State.Waiting;
            waitTimer = waitTime;
        }

        anim.SetBool("isMoving", currentState == State.Moving);

        switch (currentState)
        {
            case State.Waiting:
                HandleWaiting();
                break;
            case State.Moving:
                HandleMovement();
                break;
            case State.PlayerDetected:
                FaceTarget(playerTransform.position);
                break;
        }
    }

    private void HandleWaiting()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0)
        {
            SetNewTarget();
            currentState = State.Moving;
        }
    }

    private void SetNewTarget()
    {
        if (isMovingX)
        {
            float randomX = Random.Range(minX, maxX);
            randomX = Mathf.Round(randomX * 100f) / 100f;

            float currentY = Mathf.Round(transform.position.y * 100f) / 100f;
            targetPosition = new Vector2(randomX, currentY);
        }
        else
        {
            float randomY = Random.Range(minY, maxY);
            randomY = Mathf.Round(randomY * 100f) / 100f;

            float currentX = Mathf.Round(transform.position.x * 100f) / 100f;
            targetPosition = new Vector2(currentX, randomY);
        }

        isMovingX = !isMovingX;
        FaceTarget(targetPosition);
    }

    private void HandleMovement()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            currentState = State.Waiting;
            waitTimer = waitTime;
        }
    }

    private void FaceTarget(Vector2 targetPos)
    {
        Vector2 direction = targetPos - (Vector2)transform.position;

        float moveX = 0f;
        float moveY = 0f;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            moveX = direction.x > 0 ? 1f : -1f;
        }
        else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            moveY = direction.y > 0 ? 1f : -1f;
        }

        if (moveX != 0 || moveY != 0)
        {
            anim.SetFloat("moveX", moveX);
            anim.SetFloat("moveY", moveY);
        }
    }

    public void SetPlayerTarget(Transform player)
    {
        playerTransform = player;
    }

    public void ClearPlayerTarget()
    {
        playerTransform = null;
    }

    // ==========================================
    // ESTA É A FUNÇÃO QUE O ANIMATOR ESTÁ À PROCURA!
    // ==========================================
    public void PlayFootstep()
    {
        // Verifica se tens um som atribuído no Inspector para não dar erro
        if (footstepSound != null && audioSource != null)
        {
            // PlayOneShot permite tocar o som sem interromper o som do passo anterior
            // (caso ele ande muito rápido)
            audioSource.PlayOneShot(footstepSound);
        }
    }
}