using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody2D))]
public class Killer : MonoBehaviour
{
    private Animator anim;
    private AudioSource audioSource;
    private Rigidbody2D rb;

    [Header("Referências")]
    public Transform playerTransform;

    public enum State { Patrol, Investigate, Chase }
    public State currentState = State.Patrol;

    [Header("Velocidades")]
    public float patrolSpeed = 2.0f;
    public float investSpeed = 2.0f;
    public float chaseSpeed = 2.5f;
    public float waitTime = 5f;
    public float investWaitTime = 5f;

    [Header("Limites da Área de Patrulha")]
    public float minX = -2.85f;
    public float maxX = 2.85f;
    public float minY = -5.25f;
    public float maxY = -1f;

    [Header("Visão e Deteção")]
    public float visionDistance = 5f;
    public float visionAngle = 45f;
    [Tooltip("Área circular (ex: 1.5) onde o assassino deteta o jogador mesmo de costas")]
    public float instantDetectionRadius = 1.5f;
    [Tooltip("Layer das paredes para a visão e para não encravar (Usa a layer 'colisoes')")]
    public LayerMask obstacleLayer;

    [Header("Audição e Microfone")]
    public float alwaysHearRadius = 6.0f;
    public float investigateProbability = 0.5f;

    [Header("Áudio")]
    public AudioClip footstepSound;

    // Controlo de Movimento
    private Vector2 targetPosition;
    private Vector2 lastKnownPlayerPos;
    private float timer;
    private bool isMovingX = true;
    private bool currentPriorityIsX = true;

    private float gameStartTime;
    private Vector2 lookDirection = Vector2.down;

    // Sistema de Colisão e Desvio
    private Vector2 lastPosition;
    private float stuckTimer = 0f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();

        targetPosition = rb.position;
        lastPosition = rb.position;
        timer = waitTime;
        currentState = State.Patrol;
        gameStartTime = Time.time;
    }

    void OnEnable()
    {
        FillFromMicrophone.OnScreamDetected += HandleScream;
    }

    void OnDisable()
    {
        FillFromMicrophone.OnScreamDetected -= HandleScream;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused)
            return;

        CheckVision();

        bool isMoving = false;

        switch (currentState)
        {
            case State.Patrol:
                isMoving = HandlePatrol();
                break;
            case State.Investigate:
                isMoving = HandleInvestigate();
                break;
            case State.Chase:
                isMoving = HandleChase();
                break;
        }

        CheckIfStuck(isMoving);

        bool actuallyMoved = Vector2.Distance(rb.position, lastPosition) > 0.001f;
        anim.SetBool("isMoving", isMoving && actuallyMoved);

        lastPosition = rb.position;
    }

    // ================= VISÃO E DETEÇÃO =================

    void CheckVision()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else return;
        }

        Vector2 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. VERIFICAÇÃO DE DETEÇÃO IMEDIATA (Sexto Sentido / Tocar)
        if (distanceToPlayer <= instantDetectionRadius)
        {
            // Raycast rápido só para garantir que não o "sente" através de uma parede fininha
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleLayer);
            if (hit.collider == null)
            {
                lastKnownPlayerPos = playerTransform.position;
                currentState = State.Chase;
                return; // Detetou imediatamente, não precisa de calcular a visão frontal
            }
        }

        // 2. VERIFICAÇÃO DE VISÃO FRONTAL (Cone)
        if (distanceToPlayer <= visionDistance)
        {
            float angle = Vector2.Angle(lookDirection, dirToPlayer);

            if (angle < visionAngle)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distanceToPlayer, obstacleLayer);

                if (hit.collider == null)
                {
                    lastKnownPlayerPos = playerTransform.position;
                    currentState = State.Chase;
                    return; // Detetou, sai da função
                }
            }
        }

        // 3. SE NÃO VÊ, MAS ESTAVA A PERSEGUIR (Perdeu o jogador)
        if (currentState == State.Chase)
        {
            currentState = State.Investigate;
            timer = investWaitTime;
        }
    }

    // ================= MICROFONE E PAUSA =================

    void HandleScream()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return;

        if (Time.time < gameStartTime + 2.5f) return;
        if (currentState == State.Chase) return;

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else return;
        }

        float distToPlayer = Vector2.Distance(rb.position, playerTransform.position);
        float currentProb = (distToPlayer <= alwaysHearRadius) ? 1.0f : investigateProbability;

        if (Random.value <= currentProb)
        {
            lastKnownPlayerPos = playerTransform.position;
            currentState = State.Investigate;
            timer = investWaitTime;
            FaceTarget(lastKnownPlayerPos);
        }
    }

    // ================= MOVIMENTO E DESVIO DE OBSTÁCULOS =================

    bool HandlePatrol()
    {
        if (Vector2.Distance(rb.position, targetPosition) < 0.1f)
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0)
            {
                SetNewPatrolTarget();
                timer = waitTime;
            }
            return false;
        }
        return MoveAxisAligned(targetPosition, patrolSpeed);
    }

    bool HandleInvestigate()
    {
        if (Vector2.Distance(rb.position, lastKnownPlayerPos) < 0.1f)
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0)
            {
                currentState = State.Patrol;
                SetNewPatrolTarget();
                timer = waitTime;
            }
            return false;
        }
        return MoveAxisAligned(lastKnownPlayerPos, investSpeed);
    }

    bool HandleChase()
    {
        if (playerTransform != null)
        {
            return MoveAxisAligned(playerTransform.position, chaseSpeed);
        }
        return false;
    }

    bool MoveAxisAligned(Vector2 target, float speed)
    {
        Vector2 currentPos = rb.position;
        Vector2 nextPos = currentPos;
        bool moved = false;

        float diffX = target.x - currentPos.x;
        float diffY = target.y - currentPos.y;

        float dirX = Mathf.Sign(diffX);
        float dirY = Mathf.Sign(diffY);

        bool needsMoveX = Mathf.Abs(diffX) > 0.05f;
        bool needsMoveY = Mathf.Abs(diffY) > 0.05f;

        float colDist = 0.35f;

        bool canMoveX = needsMoveX && !Physics2D.CircleCast(currentPos, 0.2f, new Vector2(dirX, 0), colDist, obstacleLayer);
        bool canMoveY = needsMoveY && !Physics2D.CircleCast(currentPos, 0.2f, new Vector2(0, dirY), colDist, obstacleLayer);

        if (currentPriorityIsX)
        {
            if (canMoveX)
            {
                nextPos.x = Mathf.MoveTowards(currentPos.x, target.x, speed * Time.fixedDeltaTime);
                moved = true;
            }
            else if (canMoveY)
            {
                nextPos.y = Mathf.MoveTowards(currentPos.y, target.y, speed * Time.fixedDeltaTime);
                moved = true;
                currentPriorityIsX = false;
            }
            else if (needsMoveX && !needsMoveY && currentState != State.Patrol)
            {
                bool canSlideUp = !Physics2D.CircleCast(currentPos, 0.2f, Vector2.up, colDist, obstacleLayer);
                bool canSlideDown = !Physics2D.CircleCast(currentPos, 0.2f, Vector2.down, colDist, obstacleLayer);
                if (canSlideUp) { nextPos.y += speed * Time.fixedDeltaTime; moved = true; }
                else if (canSlideDown) { nextPos.y -= speed * Time.fixedDeltaTime; moved = true; }
            }
        }
        else
        {
            if (canMoveY)
            {
                nextPos.y = Mathf.MoveTowards(currentPos.y, target.y, speed * Time.fixedDeltaTime);
                moved = true;
            }
            else if (canMoveX)
            {
                nextPos.x = Mathf.MoveTowards(currentPos.x, target.x, speed * Time.fixedDeltaTime);
                moved = true;
                currentPriorityIsX = true;
            }
            else if (needsMoveY && !needsMoveX && currentState != State.Patrol)
            {
                bool canSlideRight = !Physics2D.CircleCast(currentPos, 0.2f, Vector2.right, colDist, obstacleLayer);
                bool canSlideLeft = !Physics2D.CircleCast(currentPos, 0.2f, Vector2.left, colDist, obstacleLayer);
                if (canSlideRight) { nextPos.x += speed * Time.fixedDeltaTime; moved = true; }
                else if (canSlideLeft) { nextPos.x -= speed * Time.fixedDeltaTime; moved = true; }
            }
        }

        if (!moved && currentState == State.Patrol)
        {
            SetNewPatrolTarget();
        }

        if (moved)
        {
            rb.MovePosition(nextPos);
            FaceTarget(nextPos);
        }
        return moved;
    }

    void CheckIfStuck(bool isTryingToMove)
    {
        if (!isTryingToMove)
        {
            stuckTimer = 0f;
            return;
        }

        if (Vector2.Distance(rb.position, lastPosition) < 0.005f)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > 0.15f)
            {
                currentPriorityIsX = !currentPriorityIsX;
                if (currentState == State.Patrol) SetNewPatrolTarget();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    // ================= PATRULHA E ANIMAÇÃO =================

    private void SetNewPatrolTarget()
    {
        if (isMovingX)
        {
            float randomX = Mathf.Round(Random.Range(minX, maxX) * 100f) / 100f;
            float currentY = Mathf.Round(rb.position.y * 100f) / 100f;
            targetPosition = new Vector2(randomX, currentY);
        }
        else
        {
            float randomY = Mathf.Round(Random.Range(minY, maxY) * 100f) / 100f;
            float currentX = Mathf.Round(rb.position.x * 100f) / 100f;
            targetPosition = new Vector2(currentX, randomY);
        }
        isMovingX = !isMovingX;
    }

    private void FaceTarget(Vector2 targetPos)
    {
        Vector2 direction = targetPos - rb.position;
        if (direction.magnitude < 0.001f) return;

        float moveX = 0f;
        float moveY = 0f;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            moveX = direction.x > 0 ? 1f : -1f;
            lookDirection = moveX > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            moveY = direction.y > 0 ? 1f : -1f;
            lookDirection = moveY > 0 ? Vector2.up : Vector2.down;
        }

        anim.SetFloat("moveX", moveX);
        anim.SetFloat("moveY", moveY);
    }

    public void PlayFootstep()
    {
        if (footstepSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(footstepSound);
        }
    }
}