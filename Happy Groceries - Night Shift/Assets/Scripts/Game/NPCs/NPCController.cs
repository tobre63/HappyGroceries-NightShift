using System.Collections;
using UnityEngine;

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
    public float waitTime = 2f;
    public float stoppingDistance = 0.1f;

    [Header("Interaction Settings")]
    public int interactionWaypointIndex = 1;
    [HideInInspector] public bool isWaitingForInteraction = false; // Bloqueia o NPC no waypoint até haver interação

    [Header("Fading Settings")]
    public float fadeDuration = 1.5f;
    [HideInInspector] public bool isFading = false;

    [Header("Waypoint Settings")]
    public Transform waypointsParent;
    private Transform[] waypoints;
    private int currentWaypointIndex;
    private bool isWaiting;
    [HideInInspector] public bool reachedEnd;

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
        // Inicializa todas as referências dos componentes no próprio objeto
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();

        // Configurações iniciais de física e áudio
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // Caso o NightTimer não tenha sido atribuído no Inspector, procura automaticamente pela cena
        if (nightTimer == null) nightTimer = Object.FindFirstObjectByType<NightTimer>();

        // Preenche o array de waypoints percorrendo todos os objetos filhos de waypointsParent
        if (waypointsParent != null)
        {
            waypoints = new Transform[waypointsParent.childCount];
            for (int i = 0; i < waypointsParent.childCount; i++)
            {
                waypoints[i] = waypointsParent.GetChild(i);
            }
        }

        // Faz uma verificação inicial do horário para definir se o NPC já deve começar ativo ou não
        if (nightTimer != null)
        {
            bool shouldBeActive = nightTimer.currentTime >= activationHour;
            isActiveInWorld = !shouldBeActive;
            CheckSchedule();
        }
    }

    void Update()
    {
        // Verifica todos os frames se a hora de aparecimento foi atingida
        CheckSchedule();

        // Se o NPC não deve estar visível/ativo no mundo, não corre mais nenhuma lógica do Update
        if (!isActiveInWorld) return;

        // Verifica se há alguma condição que impeça o NPC de andar (estar a falar, fade, etc.)
        if (CanMove() == false)
        {
            StopMovement();
            UpdateAnimation();
            return;
        }

        // Calcula o movimento de acordo com os waypoints e atualiza o estado da animação
        ProcessWaypointLogic();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // O FixedUpdate lida com a parte da física. 
        // Se estiver inativo, a desvanecer ou à espera de interação, remove-se qualquer velocidade.
        if (!isActiveInWorld || isFading || isWaitingForInteraction)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Se o caminho estiver livre para se mover, aplica-se a direção multiplicada pela velocidade
        if (shouldMove) rb.linearVelocity = currentDirection * moveSpeed;
        else rb.linearVelocity = Vector2.zero;
    }

    void CheckSchedule()
    {
        if (nightTimer == null) return;

        // Compara a hora do jogo com a hora de ativação configurada
        bool shouldBeActive = nightTimer.currentTime >= activationHour;

        // Apenas corre esta lógica quando ocorre uma alteração no estado (de inativo para ativo, ou vice-versa)
        if (isActiveInWorld != shouldBeActive)
        {
            isActiveInWorld = shouldBeActive;

            if (isActiveInWorld)
            {
                // Reinicia os estados principais para quando o NPC "nasce" na cena
                reachedEnd = false;
                isWaitingForInteraction = false;
                currentWaypointIndex = 0;

                // Coloca o NPC diretamente no primeiro waypoint
                if (waypoints != null && waypoints.Length > 0)
                    transform.position = waypoints[0].position;

                // Prepara o NPC para fazer o "Fade In", ativando o sprite transparente e os colisores
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
                // Esconde instantaneamente o sprite, colisões e interrompe o movimento (se a hora terminar)
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

        // Aumenta a opacidade (alpha) progressivamente até atingir o limite
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            spriteRenderer.color = c;
            yield return null;
        }

        // Garante que a opacidade fica no máximo no fim do Fade In
        c.a = 1f;
        spriteRenderer.color = c;
        isFading = false;
    }

    IEnumerator FadeOutCoroutine()
    {
        isFading = true;
        float elapsedTime = 0f;
        Color c = spriteRenderer.color;

        // Reduz a opacidade (alpha) progressivamente para desaparecer de forma fluida
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            spriteRenderer.color = c;
            yield return null;
        }

        // Garante que a opacidade fica no zero no fim do Fade Out e desativa componentes
        c.a = 0f;
        spriteRenderer.color = c;
        isFading = false;

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        foreach (var col in colliders) if (col != null) col.enabled = false;
    }

    bool CanMove()
    {
        // Se estiver bloqueado até falarmos com ele, não pode andar
        if (isWaitingForInteraction) return false;

        // Verifica outras condições bloqueadoras de movimento como: Fade, Waiting standard, Fim de Rota ou falta de array de Waypoints.
        if (isFading || isWaiting || reachedEnd || waypoints == null || waypoints.Length == 0) return false;

        return true;
    }

    void ProcessWaypointLogic()
    {
        Transform target = waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, target.position);

        // Se a distância ao alvo for menor que o limite imposto, decide que já lá chegou
        if (distance <= stoppingDistance) StartCoroutine(WaitAtWaypoint());
        else
        {
            // Caso contrário, recalcula a direção para continuar o trajeto
            currentDirection = (target.position - transform.position).normalized;
            shouldMove = true;
        }
    }

    void StopMovement()
    {
        // Remove ativamente os indicadores e a velocidade para parar no lugar
        shouldMove = false;
        currentDirection = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    void UpdateAnimation()
    {
        // Atualiza as variáveis do Animator Baseado na intenção de movimento
        bool isMoving = currentDirection.magnitude > 0.1f && shouldMove;
        anim.SetBool("isMoving", isMoving);

        // Regista a última direção conhecida para manter o NPC virado para o sítio certo quando parado
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
            // Chegou ao fim do percurso de waypoints definidos, portanto começa a desaparecer
            reachedEnd = true;
            yield return StartCoroutine(FadeOutCoroutine());
        }
        else if (currentWaypointIndex == interactionWaypointIndex)
        {
            // Atingiu o ponto exato da rotina onde vai parar à espera que o jogador venha interagir
            isWaitingForInteraction = true;
        }
        else
        {
            // É um waypoint de rotina. Espera uns segundos normais e segue o caminho
            yield return new WaitForSeconds(waitTime);
            currentWaypointIndex++;
            isWaiting = false;
        }
    }

    // Função pública projetada para que um script de interação (ex: Sistema de Diálogo) possa libertar o NPC
    public void ResumeMovement()
    {
        if (isWaitingForInteraction)
        {
            isWaitingForInteraction = false;
            currentWaypointIndex++;
            isWaiting = false;
        }
    }

    // A ser usada via Eventos de Animação para sincronizar os passos (audio) com a animação de andar
    public void PlayFootstep()
    {
        if (shouldMove && currentDirection.magnitude > 0.1f)
        {
            // Lança um OverlapCircle simples para detetar a TAG do chão sob o NPC
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.2f, floorLayer);
            AudioClip clipToPlay = defaultFootstep;

            if (hit != null)
            {
                // Escolhe um som específico caso o jogador esteja a pisar algum tipo de superfície mapeada (ex: Carpete, Azulejos)
                if (hit.CompareTag("Carpet")) clipToPlay = carpetFootstep;
                else if (hit.CompareTag("Tiles")) clipToPlay = tilesFootstep;
            }

            if (clipToPlay != null)
            {
                // Pequena oscilação de "pitch" para o som dos passos não ser demasiado repetitivo
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clipToPlay);
            }
        }
    }
}