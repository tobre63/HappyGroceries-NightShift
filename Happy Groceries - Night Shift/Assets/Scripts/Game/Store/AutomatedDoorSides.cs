using UnityEngine;

public class AutomatedDoorSide : MonoBehaviour
{
    [Header("Configurations")]
    public Animator doorAnimator;
    public string myBoolName = "triggerLeft";
    public string otherBoolName = "triggerRight";

    [Header("Audio Settings")]
    public AudioSource audioSource; // Arrasta o AudioSource (pode ser o da porta pai)
    public AudioClip doorOpenSound; // O som da porta a abrir

    private int objectsInside = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (ShouldOpen(other))
        {
            objectsInside++;

            // Só tentamos abrir se for o primeiro objeto E se o outro lado não estiver já ativo
            if (objectsInside == 1)
            {
                // VERIFICAÇÃO CRÍTICA:
                if (doorAnimator.GetBool(otherBoolName) == true)
                {
                    return;
                }

                doorAnimator.SetBool(myBoolName, true);

                // --- TOCA O SOM AQUI ---
                PlaySound();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (ShouldOpen(other))
        {
            objectsInside--;

            if (objectsInside <= 0)
            {
                objectsInside = 0;
                doorAnimator.SetBool(myBoolName, false);
                // Nota: Geralmente não se mete som ao fechar aqui, 
                // porque a porta pode demorar a reagir à animação. 
                // Se quiseres som ao fechar, o ideal é usar Animation Events na animação de fechar.
            }
        }
    }

    bool ShouldOpen(Collider2D collision)
    {
        if (collision.CompareTag("Player")) return true;
        if (collision.GetComponent<NpcWaypointController>() != null) return true;
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