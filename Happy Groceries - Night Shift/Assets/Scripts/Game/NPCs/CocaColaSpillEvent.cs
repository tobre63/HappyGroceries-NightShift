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

        // Avisa o NPCController base para NÃO apagar os objetivos/itens quando for embora!
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

        // Toca o som do susto
        if (jumpscareSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(jumpscareSound);
        }

        // 1. Faz desaparecer a Coca-Cola intacta do balcão imediatamente!
        NPCController npcCtrl = GetComponent<NPCController>();
        if (npcCtrl != null && npcCtrl.counterItems != null)
        {
            foreach (var item in npcCtrl.counterItems)
            {
                if (item != null) item.SetActive(false);
            }
        }

        // 2. Faz aparecer os objetos do derrame (Sujidade e Garrafa caída)
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

        // OBJETIVO DE PRIORIDADE MÁXIMA: Apanhar a Mop
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
    }
}