using UnityEngine;

// Mantemos este RequireComponent porque a casa de banho AINDA precisa de um som 3D local
[RequireComponent(typeof(AudioSource))]
public class ChildBathroomEvent : MonoBehaviour
{
    [Header("Time Settings")]
    public NightTimer nightTimer;
    public float appearanceTime = 24.5f;

    [Header("Objective Settings")]
    public float distanciaParaOuvir = 12.5f;

    // A variável para o teu GameObject Vazio em 2D
    [Tooltip("Arrasta para aqui o teu GameObject Vazio com o AudioSource 2D (Som do UHMMM / Susto)")]
    public AudioSource somDeRepararAudioSource;

    [Header("References")]
    public Collider2D bathroomDoorCollider;
    public GameObject visualsAndTrigger;
    
    // NOVO: A variável para o BoxCollider de interação do NPC
    [Tooltip("Arrasta o Collider de interação do NPC para aqui")]
    public Collider2D npcInteractionCollider;

    // Renomeei para ser mais claro que este é o som ambiente 3D
    private AudioSource localAudioSource;
    private bool hasAppeared = false;

    void Start()
    {
        // Vai buscar o som 3D da casa de banho
        localAudioSource = GetComponent<AudioSource>();

        if (nightTimer == null)
        {
            nightTimer = Object.FindFirstObjectByType<NightTimer>();
        }

        if (visualsAndTrigger != null) visualsAndTrigger.SetActive(false);
        if (bathroomDoorCollider != null) bathroomDoorCollider.enabled = false;
    }

    void Update()
    {
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

        // Toca o som ambiente da casa de banho (em 3D)
        localAudioSource.Play();

        if (bathroomDoorCollider != null) bathroomDoorCollider.enabled = true;
        
        // Garante que a interação está ativa quando o evento começa
        if (npcInteractionCollider != null) npcInteractionCollider.enabled = true;

        StartCoroutine(EsperarAteOuvir());
    }

    private System.Collections.IEnumerator EsperarAteOuvir()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            yield return new WaitUntil(() => Vector2.Distance(transform.position, player.transform.position) <= distanciaParaOuvir);
        }

        // --- O JOGADOR ENTROU NO RAIO DE AUDIÇÃO! ---

        // 1. Toca o "UHMMMM" usando o GameObject 2D externo (direto nos ouvidos)
        if (somDeRepararAudioSource != null)
        {
            somDeRepararAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Esqueceste-te de arrastar o GameObject do som de reação no Inspector!");
        }

        // 2. Mostra o objetivo no ecrã
        if (ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.SetObjective("Investigate the sound.", true);
        }
    }

    [Header("Timer")]
    public float tempoAteDesaparecer = 1.5f;

    public void OnDialogueFinished()
    {
        // NOVO: Desativa o collider de interação IMEDIATAMENTE para impedir duplo clique
        if (npcInteractionCollider != null)
        {
            npcInteractionCollider.enabled = false;
        }

        StartCoroutine(SequenciaDeFecho());
    }

    private System.Collections.IEnumerator SequenciaDeFecho()
    {
        if (localAudioSource != null && localAudioSource.isPlaying)
        {
            localAudioSource.Stop();
        }

        if (ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.HideObjective(true);
        }

        if (bathroomDoorCollider != null)
        {
            bathroomDoorCollider.enabled = false;
        }

        yield return new WaitForSeconds(tempoAteDesaparecer);

        gameObject.SetActive(false);
    }

    public void StopAudio()
    {
        if (localAudioSource != null && localAudioSource.isPlaying)
        {
            localAudioSource.Stop();
        }
    }
}