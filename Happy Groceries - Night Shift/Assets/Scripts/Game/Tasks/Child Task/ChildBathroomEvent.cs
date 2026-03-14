using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ChildBathroomEvent : MonoBehaviour
{
    [Header("Time Settings")]
    public NightTimer nightTimer;
    public float appearanceTime = 24.5f;

    [Header("Objective Settings")]
    public float distanciaParaOuvir = 12.5f;

    [Tooltip("Arrasta para aqui o teu GameObject Vazio com o AudioSource 2D (Som do UHMMM / Susto)")]
    public AudioSource somDeRepararAudioSource;

    [Header("References")]
    public Collider2D bathroomDoorCollider;
    public GameObject visualsAndTrigger;

    [Tooltip("Arrasta o Collider de interação do NPC para aqui")]
    public Collider2D npcInteractionCollider;

    private AudioSource localAudioSource;
    private bool hasAppeared = false;

    // NOVO: A nossa tranca de segurança anti-spam
    private bool isClosing = false;

    void Start()
    {
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

        localAudioSource.Play();

        if (bathroomDoorCollider != null) bathroomDoorCollider.enabled = true;

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

        if (somDeRepararAudioSource != null)
        {
            somDeRepararAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Esqueceste-te de arrastar o GameObject do som de reação no Inspector!");
        }

        if (ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.SetObjective("Investigate the sound.", true);
        }
    }

    [Header("Timer")]
    public float tempoAteDesaparecer = 1.5f;

    public void OnDialogueFinished()
    {
        // NOVO: Se já estiver a fechar, ignora completamente qualquer outro clique!
        if (isClosing) return;

        // Tranca a porta virtual para não aceitar mais interações
        isClosing = true;

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