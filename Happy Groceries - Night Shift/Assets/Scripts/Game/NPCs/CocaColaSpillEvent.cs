using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class CocaColaSpillEvent : MonoBehaviour
{
    [Header("Visuals & Audio")]
    public GameObject[] spillObjects;
    public AudioClip jumpscareSound;
    private AudioSource audioSource;

    [Header("Camera Focus Settings")]
    public Camera mainCamera;
    public float zoomInSize = 2.5f;
    public float zoomDuration = 0.15f;
    public Vector2 focusOffset = new Vector2(0f, 0.5f);

    // NOVO: A força do tremor da câmara durante o susto!
    [Header("Screen Shake Settings")]
    public float shakeMagnitude = 0.3f;

    [Header("Creepy Dialogue")]
    public DialogueNode creepyDialogue;

    [HideInInspector] public bool hasSpilled = false;
    public static bool isSpillEventActive = false;

    private NPCInteraction npcInteraction;
    private float originalCameraSize;
    private Vector3 originalCameraPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        npcInteraction = GetComponent<NPCInteraction>();

        if (mainCamera == null) mainCamera = Camera.main;

        if (spillObjects != null && spillObjects.Length > 0)
        {
            foreach (var obj in spillObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }

    public void StartSpillSequence()
    {
        hasSpilled = true;
        isSpillEventActive = true;

        NPCController npcCtrl = GetComponent<NPCController>();
        if (npcCtrl != null)
        {
            npcCtrl.preserveInteractionState = true;
        }

        StartCoroutine(SpillRoutine());
    }

    private IEnumerator SpillRoutine()
    {
        if (mainCamera != null)
        {
            originalCameraSize = mainCamera.orthographicSize;
            originalCameraPosition = mainCamera.transform.position;
        }

        if (jumpscareSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(jumpscareSound);
        }

        NPCController npcCtrl = GetComponent<NPCController>();
        if (npcCtrl != null && npcCtrl.counterItems != null)
        {
            foreach (var item in npcCtrl.counterItems)
            {
                if (item != null) item.SetActive(false);
            }
        }

        if (spillObjects != null && spillObjects.Length > 0)
        {
            foreach (var obj in spillObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        if (mainCamera != null)
        {
            float t = 0f;
            Vector3 targetPosition = new Vector3(transform.position.x + focusOffset.x, transform.position.y + focusOffset.y, originalCameraPosition.z);

            // 1. FAZ O ZOOM IN SUPER RÁPIDO (Sem tremer, para ser preciso)
            while (t < zoomDuration)
            {
                t += Time.deltaTime;
                float normalizedTime = t / zoomDuration;
                mainCamera.orthographicSize = Mathf.Lerp(originalCameraSize, zoomInSize, normalizedTime);
                mainCamera.transform.position = Vector3.Lerp(originalCameraPosition, targetPosition, normalizedTime);
                yield return null;
            }

            // Garante que a câmara chegou ao destino exato
            mainCamera.orthographicSize = zoomInSize;
            mainCamera.transform.position = targetPosition;

            // 2. NOVO: O TERRAMOTO! Treme violentamente durante 0.5 segundos (ou mais)
            float shakeDuration = 0.5f;
            float shakeTimer = 0f;

            while (shakeTimer < shakeDuration)
            {
                shakeTimer += Time.deltaTime;

                // Cria uma posição caótica baseada no targetPosition
                Vector2 randomShake = Random.insideUnitCircle * shakeMagnitude;
                mainCamera.transform.position = new Vector3(targetPosition.x + randomShake.x, targetPosition.y + randomShake.y, targetPosition.z);

                yield return null;
            }

            // 3. Fim do tremor: volta a estabilizar a câmara antes de o NPC falar
            mainCamera.transform.position = targetPosition;
        }

        // Já não precisamos do WaitForSeconds(0.5f) solto aqui, porque o tremor já ocupou esse tempo!

        npcInteraction.dialogueNodes = new DialogueNode[] { creepyDialogue };
        npcInteraction.StartDialogue();
    }

    public void EndSpillSequence()
    {
        StartCoroutine(ZoomOutRoutine());

        if (CleaningEventController.instance != null)
        {
            CleaningEventController.instance.isQuestActive = true;
        }

        if (ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.SetObjective("Pick up the Mop", true);
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

        isSpillEventActive = false;
    }
}