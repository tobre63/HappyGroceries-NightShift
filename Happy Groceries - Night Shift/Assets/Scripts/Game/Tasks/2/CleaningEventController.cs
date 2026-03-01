using UnityEngine;

public class CleaningEventController : MonoBehaviour
{
    public static CleaningEventController instance;

    [Header("Event State")]
    public bool isQuestActive = false;
    public bool isMopEquipped = false;
    public bool isBottlePickedUp = false;
    public int dirtZonesCleaned = 0;

    [Header("Settings")]
    public int totalDirtZones = 3;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void CheckProgress()
    {
        // Se já limpou tudo...
        if (dirtZonesCleaned >= totalDirtZones && isBottlePickedUp)
        {
            // OBJETIVO 3: Pousar a Mop (Com PRIORIDADE = true)
            if (ObjectiveFeedback.instance != null)
            {
                ObjectiveFeedback.instance.SetObjective("Put the mop back.", true);
            }
        }
    }

    public bool IsTaskReadyToFinish()
    {
        return (dirtZonesCleaned >= totalDirtZones && isBottlePickedUp);
    }
}