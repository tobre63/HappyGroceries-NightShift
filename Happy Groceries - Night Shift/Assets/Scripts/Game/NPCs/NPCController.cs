using System.Collections;
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
    }

    void Start()
    {
        if (nightTimer == null) nightTimer = Object.FindFirstObjectByType<NightTimer>();

        if (nightTimer != null)
        {
            bool shouldBeActive = nightTimer.currentTime >= activationHour;
            isActiveInWorld = !shouldBeActive;
            CheckSchedule();
        }
    }

    void Update()
    {
        CheckSchedule();

        if (!isActiveInWorld) return;

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

    void CheckSchedule()
    {
        if (nightTimer == null) return;

        bool shouldBeActive = nightTimer.currentTime >= activationHour;

        if (isActiveInWorld != shouldBeActive)
        {
            isActiveInWorld = shouldBeActive;
            if (isActiveInWorld)
            {
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
            }
            else
            {
                if (spriteRenderer != null) spriteRenderer.enabled = false;
                foreach (var col in colliders) if (col != null) col.enabled = false;
                StopMovement();
            }
        }
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
            gameObject.SetActive(false);
            //Destroy(gameObject);
        }
        else if (currentWaypointIndex == interactionWaypointIndex)
        {
            isWaitingForInteraction = true;

            // --- Lógica do Balcão e Objetivo ---
            if (ObjectiveFeedback.instance != null)
            {
                // Repara no 'true' no final. Significa: Sou prioridade!
                ObjectiveFeedback.instance.SetObjective("Serve the customer.", true);
            }

            if (counterItems != null)
            {
                foreach (var item in counterItems)
                {
                    if (item != null) item.SetActive(true);
                }
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

            // --- Reverter Lógica do Balcão e Objetivo ---
            if (ObjectiveFeedback.instance != null)
            {
                // Repara no 'true'. Ele diz: "A prioridade acabou, volta ao normal".
                ObjectiveFeedback.instance.HideObjective(true);
            }

            if (counterItems != null)
            {
                foreach (var item in counterItems)
                {
                    if (item != null) item.SetActive(false);
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
}