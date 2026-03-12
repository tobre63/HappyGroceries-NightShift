using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
// Mantemos este RequireComponent para garantir que o assassino tem o seu próprio AudioSource para os passos
[RequireComponent(typeof(AudioSource))]
public class KillerController : MonoBehaviour
{
    public float runSpeed = 8f;

    [Header("Audio")]
    [Tooltip("Arrasta para aqui o teu GameObject Vazio com o AudioSource 2D do Jumpscare")]
    public AudioSource jumpscareAudioSource; // <-- Variável nova para o teu GameObject 2D!

    [Tooltip("Coloca aqui o som de passos no concreto")]
    public AudioClip concreteFootstep;

    private Transform targetPlayer;
    private bool isRunning = false;
    private Animator anim;

    // Este AudioSource será usado APENAS para os passos, emitidos pelo próprio assassino
    private AudioSource localAudioSource;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        localAudioSource = GetComponent<AudioSource>();
        localAudioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Garante que o collider está em modo Trigger para detetar o toque
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Update()
    {
        if (isRunning && targetPlayer != null)
        {
            // Calcula a direção para onde o assassino está a ir
            Vector2 direction = (targetPlayer.position - transform.position).normalized;

            // Atualiza o Animator para ele andar na direção certa (igual aos NPCs)
            anim.SetFloat("moveX", direction.x);
            anim.SetFloat("moveY", direction.y);
            anim.SetBool("isMoving", true);

            // Move fisicamente o assassino
            transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, runSpeed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    public void StartChasing(Transform playerTransform)
    {
        targetPlayer = playerTransform;
        isRunning = true;

        // Toca o som de jumpscare através do GameObject externo (2D)
        if (jumpscareAudioSource != null)
        {
            jumpscareAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Esqueceste-te de arrastar o GameObject do Jumpscare para o script!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isRunning)
        {
            isRunning = false;
            TrashEventController.instance.TriggerKillerSceneChange();
        }
    }

    // --- FUNÇÃO CHAMADA PELOS ANIMATION EVENTS ---
    public void PlayFootstep()
    {
        // Só toca som se ele estiver a correr atrás do jogador
        if (isRunning && concreteFootstep != null)
        {
            // Pequena variação de pitch para os passos soarem mais naturais
            localAudioSource.pitch = Random.Range(0.9f, 1.1f);
            localAudioSource.PlayOneShot(concreteFootstep);
        }
    }
}