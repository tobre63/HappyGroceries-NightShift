using UnityEngine;

public class AutomatedDoorSide : MonoBehaviour
{
    [Header("Configurations")]
    public Animator doorAnimator;
    public string myBoolName = "triggerLeft";
    public string otherBoolName = "triggerRight";

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip doorOpenSound;

    [Header("Lock System")]
    [Tooltip("Coloca um 'certo' aqui se esta porta for a porta final que precisa da chave!")]
    public bool isLockedByAKey = false; // NOVO!

    private int objectsInside = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        // NOVO: Se a porta estiver trancada à chave, não faz absolutamente nada!
        if (isLockedByAKey) return;

        if (ShouldOpen(other))
        {
            objectsInside++;

            if (objectsInside == 1)
            {
                if (doorAnimator.GetBool(otherBoolName) == true)
                {
                    return;
                }

                doorAnimator.SetBool(myBoolName, true);
                PlaySound();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // NOVO: Não processa saídas se a porta estiver trancada, para não bugar a matemática.
        if (isLockedByAKey) return;

        if (ShouldOpen(other))
        {
            objectsInside--;

            if (objectsInside <= 0)
            {
                objectsInside = 0;
                doorAnimator.SetBool(myBoolName, false);
            }
        }
    }

    bool ShouldOpen(Collider2D collision)
    {
        if (collision.CompareTag("Player")) return true;
        if (collision.CompareTag("Killer")) return true;
        if (collision.GetComponent<NPCController>() != null) return true;
        return false;
    }

    void PlaySound()
    {
        if (audioSource != null && doorOpenSound != null)
        {
            // PlayOneShot permite que o som toque sem cortar outros sons que possam estar a dar
            audioSource.PlayOneShot(doorOpenSound);
        }
    }
}