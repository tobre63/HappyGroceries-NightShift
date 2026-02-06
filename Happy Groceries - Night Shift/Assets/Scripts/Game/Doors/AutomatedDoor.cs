using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AutomatedDoor : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    public AudioClip doorSound;
    [SerializeField] private int entitiesInside = 0;
    private bool isOpen = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Configurações básicas do AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // Som 3D
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") || col.CompareTag("NPCs"))
        {
            entitiesInside++;
            UpdateDoorState();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player") || col.CompareTag("NPCs"))
        {
            entitiesInside--;
            if (entitiesInside < 0) entitiesInside = 0;
            UpdateDoorState();
        }
    }

    void UpdateDoorState()
    {
        bool shouldBeOpen = entitiesInside > 0;

        // Só toca o som se o estado da porta realmente mudar
        if (shouldBeOpen && !isOpen)
        {
            PlaySound(doorSound);
            isOpen = true;
        }
        else if (!shouldBeOpen && isOpen)
        {
            PlaySound(doorSound);
            isOpen = false;
        }

        animator.SetBool("triggerOpen", isOpen);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Pequena variação para não ser repetitivo
            audioSource.PlayOneShot(clip);
        }
    }
}