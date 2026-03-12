using UnityEngine;
using UnityEngine.SceneManagement;

public class TrashEventController : MonoBehaviour
{
    public static TrashEventController instance;

    [Header("Settings")]
    public float questStartHour = 25f; // 01:00 da manhã
    public string killerSceneName = "KillerScene";

    [Header("Killer Event")]
    public GameObject killerGameObject; // Arrasta o assassino da hierarquia para aqui
    public bool isKillerEventActive = false; // Flag para bloquear o jogador durante a cena

    [Header("Event State")]
    public bool isQuestActive = false;
    public bool isTaskCompleted = false;

    [HideInInspector] public int trashCollectedCount = 0;
    [HideInInspector] public int trashDisposedCount = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Garante que o assassino começa desativado invisível
        if (killerGameObject != null)
        {
            killerGameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isTaskCompleted || isQuestActive) return;

        if (NightTimer.instance != null && NightTimer.instance.currentTime >= questStartHour)
        {
            StartTrashQuest();
        }
    }

    public void StartTrashQuest()
    {
        isQuestActive = true;
        trashCollectedCount = 0;
        trashDisposedCount = 0;

        if (ObjectiveFeedback.instance != null)
            ObjectiveFeedback.instance.SetObjective("Take out the trash", true);
    }

    public void OnTrashPickedUp()
    {
        if (!isQuestActive) return;
        trashCollectedCount++;

        if (ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.RemoveSpecificObjective("Take out the trash");
            ObjectiveFeedback.instance.RemoveSpecificObjective("Take out the other trash");
            ObjectiveFeedback.instance.SetObjective("Throw it in the dumpster", true);
        }
    }

    public void OnTrashDisposed()
    {
        if (!isQuestActive) return;
        trashDisposedCount++;

        if (ObjectiveFeedback.instance != null)
            ObjectiveFeedback.instance.RemoveSpecificObjective("Throw it in the dumpster");

        if (trashDisposedCount == 1 && ObjectiveFeedback.instance != null)
        {
            ObjectiveFeedback.instance.SetObjective("Take out the other trash", true);
        }
    }

    // Nova função para ATIVAR o assassino e fazê-lo correr
    // Nova função para ATIVAR o assassino e fazê-lo correr
    public void SpawnKillerAndChase(Transform playerTransform)
    {
        if (isKillerEventActive) return; // Evita chamar múltiplas vezes
        isKillerEventActive = true; // Bloqueia o jogador definitivamente

        if (killerGameObject != null)
        {
            // ALINHA O ASSASSINO: X igual ao do jogador, Y fixo em 4
            killerGameObject.transform.position = new Vector2(playerTransform.position.x, 4f);

            killerGameObject.SetActive(true); // Fica visível

            KillerController killerScript = killerGameObject.GetComponent<KillerController>();
            if (killerScript != null)
            {
                killerScript.StartChasing(playerTransform);
            }
        }
    }

    // Função que o Assassino chama quando toca no jogador
    public void TriggerKillerSceneChange()
    {
        isQuestActive = false;

        if (TaskManager.instance != null)
            TaskManager.instance.hasTrash = false;

        TrashInteractable.isInteractingWithTrash = false;

        SceneManager.LoadScene(killerSceneName);
    }
}