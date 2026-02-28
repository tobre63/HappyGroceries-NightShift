using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class CocaColaSpillEvent : MonoBehaviour
{
    [Header("Visuals & Audio")]
    public GameObject cocaColaPuddle;
    public AudioClip jumpscareSound;
    private AudioSource audioSource;

    [Header("Camera Focus Settings")]
    public Camera mainCamera;
    public float zoomInSize = 2.5f;
    public float zoomDuration = 0.15f;
    public Vector2 focusOffset = new Vector2(0f, 0.5f);

    [Header("Creepy Dialogue")]
    public DialogueNode creepyDialogue;

    [HideInInspector] public bool hasSpilled = false;

    private NPCInteraction npcInteraction;
    private float originalCameraSize;
    private Vector3 originalCameraPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        npcInteraction = GetComponent<NPCInteraction>();

        if (mainCamera == null) mainCamera = Camera.main;
        if (cocaColaPuddle != null) cocaColaPuddle.SetActive(false);
    }

    public void StartSpillSequence()
    {
        hasSpilled = true;
        StartCoroutine(SpillRoutine());
    }

    private IEnumerator SpillRoutine()
    {
        if (mainCamera != null)
        {
            originalCameraSize = mainCamera.orthographicSize;
            originalCameraPosition = mainCamera.transform.position;
        }

        // 1. Som e Poça
        if (jumpscareSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(jumpscareSound);
        }
        if (cocaColaPuddle != null) cocaColaPuddle.SetActive(true);

        // 2. Faz Zoom e sobe a câmara ligeiramente
        if (mainCamera != null)
        {
            float t = 0f;

            Vector3 targetPosition = new Vector3(
                transform.position.x + focusOffset.x,
                transform.position.y + focusOffset.y,
                originalCameraPosition.z
            );

            while (t < zoomDuration)
            {
                t += Time.deltaTime;
                float normalizedTime = t / zoomDuration;

                mainCamera.orthographicSize = Mathf.Lerp(originalCameraSize, zoomInSize, normalizedTime);
                mainCamera.transform.position = Vector3.Lerp(originalCameraPosition, targetPosition, normalizedTime);

                yield return null;
            }

            mainCamera.orthographicSize = zoomInSize;
            mainCamera.transform.position = targetPosition;
        }

        yield return new WaitForSeconds(0.5f);

        // 3. Diálogo assustador
        npcInteraction.dialogueNodes = new DialogueNode[] { creepyDialogue };
        npcInteraction.StartDialogue();
    }

    public void EndSpillSequence()
    {
        StartCoroutine(ZoomOutRoutine());

        if (ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.SetObjective("Pick up the Mop.");
        }
    }

    private IEnumerator ZoomOutRoutine()
    {
        if (mainCamera != null)
        {
            float t = 0f;
            float duration = 1f;

            Vector3 startPos = mainCamera.transform.position;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalizedTime = t / duration;

                mainCamera.orthographicSize = Mathf.Lerp(zoomInSize, originalCameraSize, normalizedTime);
                mainCamera.transform.position = Vector3.Lerp(startPos, originalCameraPosition, normalizedTime);

                yield return null;
            }

            mainCamera.orthographicSize = originalCameraSize;
            mainCamera.transform.position = originalCameraPosition;
        }
    }
}