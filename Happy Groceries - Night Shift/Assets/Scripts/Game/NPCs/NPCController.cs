using System.Collections;
using System.Collections.Generic; // Necessário para a Lista
using UnityEngine;

[System.Serializable]
public class NPCWaypoint
{
    public Transform point;
    public float waitTime = 2f;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class NPCController : MonoBehaviour
{
    // --- LÓGICA DE FILA ESTÁTICA (NOVO) ---
    private static NPCController currentActiveNPC = null; // Quem está na cena agora
    private static List<NPCController> waitingQueue = new List<NPCController>(); // Fila de espera
    private bool isQueued = false; // Garante que não entra na fila duas vezes

    // --- SETTINGS ---

    [Header("Schedule Settings")]
    [SerializeField] private NightTimer nightTimer;
    public float activationHour = 22f;
    [HideInInspector] public bool isActiveInWorld = false;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float stoppingDistance = 0.1f;

    [Header("Interaction Settings")]
    public int interactionWaypointIndex = 1;
    [HideInInspector] public bool isWaitingForInteraction = false;
    [HideInInspector] public bool preserveInteractionState = false;

    [Header("Counter Settings")]
    public GameObject[] counterItems;

    [Header("Fading Settings")]
    public float fadeDuration = 1.5f;
    [HideInInspector] public bool isFading = false;

    [Header("Waypoint Settings")]
    public NPCWaypoint[] waypoints;
    private int currentWaypointIndex;
    private bool isWaiting;
    [HideInInspector] public bool reachedEnd;

    [Header("Item Collection Settings")]
    public GameObject[] targetItems;

    [Header("Audio Settings")]
    public AudioClip defaultFootstep;
    public AudioClip carpetFootstep;
    public AudioClip tilesFootstep;
    public LayerMask floorLayer;

    // --- COMPONENTES E CONTROLOS INTERNOS ---

    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;

    private Vector2 currentDirection;
    private Vector2 lastDirection = Vector2.down;
    private bool shouldMove = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        audioSource.playOnAwake = false;

        // Reset de segurança ao iniciar (caso a Unity mantenha estáticos entre execuções no Editor)
        currentActiveNPC = null;
        waitingQueue.Clear();
    }

    void Start()
    {
        if (nightTimer == null) nightTimer = Object.FindFirstObjectByType<NightTimer>();

        // Inicialmente desativado até ser chamado pela fila
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        foreach (var col in colliders) if (col != null) col.enabled = false;
    }

    void Update()
    {
        // Se ainda não está no mundo, verifica se deve entrar na fila
        if (!isActiveInWorld)
        {
            CheckQueueRegistration();
            return;
        }

        // Se está ativo, processa movimento e animação
        if (CanMove() == false)
        {
            StopMovement();
            UpdateAnimation();
            return;
        }

        ProcessWaypointLogic();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (!isActiveInWorld || isFading || isWaitingForInteraction)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (shouldMove) rb.linearVelocity = currentDirection * moveSpeed;
        else rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Gerencia a entrada na fila e a ativação baseada na disponibilidade da cena.
    /// </summary>
    void CheckQueueRegistration()
    {
        if (nightTimer == null) return;

        // 1. Verificar se já passou da hora de aparecer
        bool reachedTime = nightTimer.currentTime >= activationHour;

        // 2. Se chegou a hora e ainda não está na fila, entra nela
        if (reachedTime && !isQueued)
        {
            waitingQueue.Add(this);
            isQueued = true;
            // Debug.Log($"{gameObject.name} entrou na fila de espera.");
        }

        // 3. Se eu estou na fila, verificar se sou o primeiro e se a cena está vazia
        if (isQueued && currentActiveNPC == null)
        {
            if (waitingQueue.Count > 0 && waitingQueue[0] == this)
            {
                ActivateNPC();
            }
        }
    }

    void ActivateNPC()
    {
        isActiveInWorld = true;
        currentActiveNPC = this; // Ocupa a vaga
        waitingQueue.RemoveAt(0); // Remove-se da fila

        reachedEnd = false;
        isWaitingForInteraction = false;
        currentWaypointIndex = 0;

        if (waypoints != null && waypoints.Length > 0 && waypoints[0].point != null)
            transform.position = waypoints[0].point.position;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Color c = spriteRenderer.color;
            c.a = 0f;
            spriteRenderer.color = c;
        }

        foreach (var col in colliders) if (col != null) col.enabled = true;

        StartCoroutine(FadeInCoroutine());
        // Debug.Log($"{gameObject.name} começou o FadeIn e ocupou o balcão.");
    }

    IEnumerator FadeInCoroutine()
    {
        isFading = true;
        float elapsedTime = 0f;
        Color c = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            spriteRenderer.color = c;
            yield return null;
        }

        c.a = 1f;
        spriteRenderer.color = c;
        isFading = false;
    }

    IEnumerator FadeOutCoroutine()
    {
        isFading = true;
        float elapsedTime = 0f;
        Color c = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            spriteRenderer.color = c;
            yield return null;
        }

        c.a = 0f;
        spriteRenderer.color = c;
        isFading = false;

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        foreach (var col in colliders) if (col != null) col.enabled = false;

        // LIBERAR A VAGA PARA O PRÓXIMO DA FILA
        currentActiveNPC = null;
        isActiveInWorld = false;
        gameObject.SetActive(false);
    }

    bool CanMove()
    {
        if (isWaitingForInteraction) return false;
        if (isFading || isWaiting || reachedEnd || waypoints == null || waypoints.Length == 0) return false;
        if (waypoints[currentWaypointIndex].point == null) return false;
        return true;
    }

    void ProcessWaypointLogic()
    {
        Transform target = waypoints[currentWaypointIndex].point;
        float distance = Vector2.Distance(transform.position, target.position);

        if (distance <= stoppingDistance) StartCoroutine(WaitAtWaypoint());
        else
        {
            currentDirection = (target.position - transform.position).normalized;
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

        if (isMoving) lastDirection = currentDirection;
        anim.SetFloat("moveX", lastDirection.x);
        anim.SetFloat("moveY", lastDirection.y);
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        StopMovement();

        if (currentWaypointIndex >= waypoints.Length - 1)
        {
            reachedEnd = true;
            yield return StartCoroutine(FadeOutCoroutine());
            // O SetActive(false) agora acontece dentro do Coroutine após o FadeOut
        }
        else if (currentWaypointIndex == interactionWaypointIndex)
        {
            isWaitingForInteraction = true;

            if (ObjectiveFeedback.instance != null)
            {
                ObjectiveFeedback.instance.SetObjective("Serve the customer.", true);
            }

            if (counterItems != null)
            {
                foreach (var item in counterItems) if (item != null) item.SetActive(true);
            }
        }
        else
        {
            float currentWaitTime = waypoints[currentWaypointIndex].waitTime;
            yield return new WaitForSeconds(currentWaitTime);

            currentWaypointIndex++;
            isWaiting = false;
        }
    }

    public void ResumeMovement()
    {
        if (isWaitingForInteraction)
        {
            isWaitingForInteraction = false;
            currentWaypointIndex++;
            isWaiting = false;

            if (!preserveInteractionState)
            {
                if (ObjectiveFeedback.instance != null) ObjectiveFeedback.instance.HideObjective(true);
                if (counterItems != null)
                {
                    foreach (var item in counterItems) if (item != null) item.SetActive(false);
                }
            }
        }
    }

    public void PlayFootstep()
    {
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (targetItems == null || targetItems.Length == 0) return;
        for (int i = 0; i < targetItems.Length; i++)
        {
            if (targetItems[i] != null && collision.gameObject == targetItems[i])
            {
                targetItems[i].SetActive(false);
                break;
            }
        }
    }

    // Limpeza de segurança caso o objeto seja destruído por outro motivo
    private void OnDestroy()
    {
        if (currentActiveNPC == this) currentActiveNPC = null;
        if (waitingQueue.Contains(this)) waitingQueue.Remove(this);
    }
}