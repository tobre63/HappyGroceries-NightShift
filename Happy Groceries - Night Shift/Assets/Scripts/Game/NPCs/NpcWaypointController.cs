using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class NpcWaypointController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public float stoppingDistance = 0.1f;

    [Header("Waypoints")]
    public Transform waypointsParent; // Arraste o objeto pai dos waypoints para aqui
    private Transform[] waypoints;
    private int currentWaypointIndex;
    private bool isWaiting;
    private bool reachedEnd;

    [Header("Schedule (NightTimer)")]
    [SerializeField] private NightTimer nightTimer;
    public float startMovingHour = 22f;

    [Header("Audio Settings")]
    public AudioClip defaultFootstep;
    public AudioClip carpetFootstep;
    public AudioClip tilesFootstep;
    public LayerMask floorLayer;

    // Variáveis Internas
    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource audioSource;
    private Vector2 currentDirection;
    private Vector2 lastDirection = Vector2.down;
    private bool shouldMove = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Configuração segura do Rigidbody para Top-Down
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // Encontrar o NightTimer se não estiver atribuído
        if (nightTimer == null)
            nightTimer = Object.FindFirstObjectByType<NightTimer>();

        // Inicializar Waypoints
        if (waypointsParent != null)
        {
            waypoints = new Transform[waypointsParent.childCount];
            for (int i = 0; i < waypointsParent.childCount; i++)
            {
                waypoints[i] = waypointsParent.GetChild(i);
            }
        }
    }

    void Update()
    {
        // 1. Verificar Pausa e Horário
        if (CanMove() == false)
        {
            StopMovement();
            UpdateAnimation(); // Atualiza para idle
            return;
        }

        // 2. Lógica de Waypoints
        ProcessWaypointLogic();

        // 3. Atualizar Animação
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // Movimento físico real
        if (shouldMove)
        {
            // NOTA: Se usares uma versão antiga do Unity (antes da 6), troca 'linearVelocity' por 'velocity'
            rb.linearVelocity = currentDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // --- Lógica Principal ---

    bool CanMove()
    {
        // Verifica Pausa
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return false;

        // Verifica Horário (se o timer existir)
        if (nightTimer != null && nightTimer.currentTime < startMovingHour) return false;

        // Verifica se terminou o caminho ou está à espera
        if (isWaiting || reachedEnd) return false;

        // Verifica se existem waypoints
        if (waypoints == null || waypoints.Length == 0) return false;

        return true;
    }

    void ProcessWaypointLogic()
    {
        Transform target = waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= stoppingDistance)
        {
            // Chegou ao ponto
            StartCoroutine(WaitAtWaypoint());
        }
        else
        {
            // Calcular direção
            Vector2 direction = (target.position - transform.position).normalized;
            currentDirection = direction;
            shouldMove = true;
        }
    }

    void StopMovement()
    {
        shouldMove = false;
        currentDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    void UpdateAnimation()
    {
        bool isMoving = currentDirection.magnitude > 0.1f && shouldMove;
        anim.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            lastDirection = currentDirection;
            anim.SetFloat("moveX", lastDirection.x);
            anim.SetFloat("moveY", lastDirection.y);
        }
        else
        {
            anim.SetFloat("moveX", lastDirection.x);
            anim.SetFloat("moveY", lastDirection.y);
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        StopMovement();

        yield return new WaitForSeconds(waitTime);

        if (currentWaypointIndex >= waypoints.Length - 1)
        {
            reachedEnd = true;
        }
        else
        {
            currentWaypointIndex++;
            isWaiting = false;
        }
    }

    // --- Sistema de Áudio (Chamado via Animation Event) ---
    public void PlayFootstep()
    {
        // Só toca som se estiver realmente a mover-se
        if (shouldMove && currentDirection.magnitude > 0.1f)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.2f, floorLayer);
            AudioClip clipToPlay = defaultFootstep;

            if (hit != null)
            {
                if (hit.CompareTag("Carpet")) clipToPlay = carpetFootstep;
                else if (hit.CompareTag("Tiles")) clipToPlay = tilesFootstep;
            }

            if (clipToPlay != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clipToPlay);
            }
        }
    }

    void OnDrawGizmos()
    {
        // Se não houver pai dos waypoints, não desenha nada
        if (waypointsParent == null) return;

        Gizmos.color = Color.red; // Cor da linha

        // Percorre todos os filhos para desenhar linhas entre eles
        for (int i = 0; i < waypointsParent.childCount - 1; i++)
        {
            Transform current = waypointsParent.GetChild(i);
            Transform next = waypointsParent.GetChild(i + 1);

            if (current != null && next != null)
            {
                Gizmos.DrawLine(current.position, next.position);
                // Desenha uma bolinha em cada ponto para ser mais fácil ver
                Gizmos.DrawSphere(current.position, 0.2f);
            }
        }

        // Desenha a bolinha do último ponto
        if (waypointsParent.childCount > 0)
        {
            Gizmos.DrawSphere(waypointsParent.GetChild(waypointsParent.childCount - 1).position, 0.2f);
        }
    }
}