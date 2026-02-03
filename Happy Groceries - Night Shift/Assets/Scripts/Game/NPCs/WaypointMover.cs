using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    [Header("Dependencies")]
    public Transform Waypoints;
    // Referência ao script do relógio. Arraste o objeto que tem o NightTimer para aqui no Inspector
    [SerializeField] private NightTimer nightTimer;

    [Header("Settings")]
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public float startMovingHour = 22f; // Hora que o NPC começa a andar (22 = 22:00)

    private Transform[] waypoints;
    private int currentWaypointIndex;
    private bool isWaiting;
    private bool reachedEnd = false;
    private NpcController npcController;

    void Start()
    {
        npcController = GetComponent<NpcController>();

        // Se o nightTimer não foi atribuído no Inspector, tenta achar automaticamente na cena
        if (nightTimer == null)
        {
            nightTimer = FindObjectOfType<NightTimer>();
        }

        // Inicializa os waypoints baseados nos filhos do objeto pai
        if (Waypoints != null)
        {
            waypoints = new Transform[Waypoints.childCount];
            for (int i = 0; i < Waypoints.childCount; i++)
            {
                waypoints[i] = Waypoints.GetChild(i);
            }
        }
    }

    void Update()
    {
        // 1. Verifica se o jogo está pausado
        if (GameManager.Instance.isPaused)
        {
            StopMovement();
            return;
        }

        // 2. NOVA LÓGICA: Verifica o horário
        // Se temos o relógio E a hora atual é MENOR que 22h, o NPC fica parado
        if (nightTimer != null && nightTimer.currentTime < startMovingHour)
        {
            StopMovement(); // Garante que ele fique parado esperando dar o horário
            return;
        }

        // 3. Verifica se está esperando no waypoint ou se já acabou o caminho
        if (isWaiting || reachedEnd)
        {
            StopMovement();
            return;
        }

        MoveToWaypoint();
    }

    // Criei uma função auxiliar para parar o NPC e limpar o input, evitando repetição de código
    void StopMovement()
    {
        if (npcController != null) npcController.SetInput(Vector2.zero);
    }

    void MoveToWaypoint()
    {
        if (waypoints == null || currentWaypointIndex >= waypoints.Length) return;

        Transform target = waypoints[currentWaypointIndex];

        // Calcula a direção para o Animator
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        if (npcController != null) npcController.SetInput(direction);

        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
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
}