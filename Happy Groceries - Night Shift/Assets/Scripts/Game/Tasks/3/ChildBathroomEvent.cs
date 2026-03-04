using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ChildBathroomEvent : MonoBehaviour
{
    [Header("Time Settings")]
    public NightTimer nightTimer;
    public float appearanceTime = 24.5f;

    [Header("Objective Settings")]
    public float distanciaParaOuvir = 12.5f;
    public AudioClip somDeRepararNoBarulho;

    [Header("References")]
    public Collider2D bathroomDoorCollider;
    public GameObject visualsAndTrigger;

    private AudioSource audioSource;
    private bool hasAppeared = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Se te esqueceres de arrastar o relógio, ele tenta procurar por segurança
        if (nightTimer == null)
        {
            nightTimer = Object.FindFirstObjectByType<NightTimer>();
        }

        // Esconde a criança no início
        if (visualsAndTrigger != null) visualsAndTrigger.SetActive(false);
        if (bathroomDoorCollider != null) bathroomDoorCollider.enabled = false;
    }

    void Update()
    {
        // Só verifica se ainda não apareceu e se o relógio existe
        if (!hasAppeared && nightTimer != null)
        {
            if (nightTimer.currentTime >= appearanceTime)
            {
                StartEvent();
            }
        }
    }

    private void StartEvent()
    {
        hasAppeared = true;

        if (visualsAndTrigger != null) visualsAndTrigger.SetActive(true);
        audioSource.Play();
        if (bathroomDoorCollider != null) bathroomDoorCollider.enabled = true;

        StartCoroutine(EsperarAteOuvir());
    }

    private System.Collections.IEnumerator EsperarAteOuvir()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // O jogo espera que o jogador se aproxime...
            yield return new WaitUntil(() => Vector2.Distance(transform.position, player.transform.position) <= distanciaParaOuvir);
        }

        // --- O JOGADOR ENTROU NO RAIO DE AUDIÇÃO! ---

        // 1. Toca o "UHMMMM" por cima dos barulhos da casa de banho
        if (somDeRepararNoBarulho != null && audioSource != null)
        {
            audioSource.PlayOneShot(somDeRepararNoBarulho);
        }

        // 2. Mostra o objetivo no ecrã
        if (ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.SetObjective("Investigate the sound.", true);
        }
    }

    [Header("Timer")]
    public float tempoAteDesaparecer = 1.5f; // Podes mudar isto no Inspector!

    public void OnDialogueFinished()
    {
        // Em vez de desligar logo, inicia a contagem de tempo
        StartCoroutine(SequenciaDeFecho());
    }

    private System.Collections.IEnumerator SequenciaDeFecho()
    {
        // 1. Para o som imediatamente (opcional, ou podes deixar tocar enquanto a porta fecha)
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 2. Limpa o objetivo
        if (ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.HideObjective(true);
        }

        // 3. Desativa o colisor da porta (Se tiveres alguma animação/código que mande a porta fechar, coloca aqui!)
        if (bathroomDoorCollider != null)
        {
            bathroomDoorCollider.enabled = false;
        }

        // 4. O COMPASSO DE ESPERA: O jogo espera o tempo exato da porta fechar
        yield return new WaitForSeconds(tempoAteDesaparecer);

        // 5. Agora sim, com a porta completamente fechada, a criança desaparece em segurança
        gameObject.SetActive(false);
    }

    public void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}