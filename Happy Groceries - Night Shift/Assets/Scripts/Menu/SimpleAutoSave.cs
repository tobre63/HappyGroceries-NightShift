using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleAutoSave : MonoBehaviour
{
    [Header("Configurações")]
    // Arraste aqui o objeto que tem o script de Tempo/Dia-Noite
    [SerializeField] private GameObject timeSystemGameObject;

    private void Start()
    {
        // Tenta carregar os dados com um pequeno atraso para garantir que
        // o cenário já carregou e o CharacterController não bloqueie o movimento.
        StartCoroutine(LoadDataRoutine());
    }

    private System.Collections.IEnumerator LoadDataRoutine()
    {
        // Espera 1 frame para garantir que tudo inicializou
        yield return null;

        // 1. CARREGAR POSIÇÃO
        if (PlayerPrefs.HasKey("PlayerX"))
        {
            float x = PlayerPrefs.GetFloat("PlayerX");
            float y = PlayerPrefs.GetFloat("PlayerY");
            float z = PlayerPrefs.GetFloat("PlayerZ");
            Vector3 targetPos = new Vector3(x, y, z);

            // Desativa o CharacterController momentaneamente para mover (se existir)
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = targetPos;

            // Se tiver rotação salva, restaura também (opcional, mas recomendado)
            if (PlayerPrefs.HasKey("PlayerRotY"))
            {
                float rotY = PlayerPrefs.GetFloat("PlayerRotY");
                transform.rotation = Quaternion.Euler(0, rotY, 0);
            }

            if (cc != null) cc.enabled = true;
            Debug.Log("Posição carregada: " + targetPos);
        }

        // 2. CARREGAR TEMPO
        if (PlayerPrefs.HasKey("SavedTime") && timeSystemGameObject != null)
        {
            float savedTime = PlayerPrefs.GetFloat("SavedTime");

            // --- ATENÇÃO: ADAPTE AQUI PARA O SEU SCRIPT DE TEMPO ---
            // Exemplo: Se seu script se chama 'DayNightCycle' e a variável é 'currentTime':

            /*
            var timeScript = timeSystemGameObject.GetComponent<DayNightCycle>();
            if (timeScript != null)
            {
                timeScript.currentTime = savedTime;
            }
            */

            Debug.Log("Tempo carregado: " + savedTime);
        }
    }

    // Salva automaticamente ao sair ou mudar de cena
    private void OnDisable()
    {
        SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame()
    {
        // 1. SALVAR POSIÇÃO
        PlayerPrefs.SetFloat("PlayerX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerY", transform.position.y);
        PlayerPrefs.SetFloat("PlayerZ", transform.position.z);
        PlayerPrefs.SetFloat("PlayerRotY", transform.rotation.eulerAngles.y);

        // 2. SALVAR CENA
        PlayerPrefs.SetString("SavedScene", SceneManager.GetActiveScene().name);

        // 3. SALVAR TEMPO
        if (timeSystemGameObject != null)
        {
            // --- ATENÇÃO: ADAPTE AQUI TAMBÉM ---
            // Você precisa pegar o valor atual do seu script de tempo

            /*
            var timeScript = timeSystemGameObject.GetComponent<DayNightCycle>();
            if (timeScript != null)
            {
                PlayerPrefs.SetFloat("SavedTime", timeScript.currentTime);
            }
            */
        }

        PlayerPrefs.Save();
        Debug.Log("Jogo Salvo (Posição e Tempo)!");
    }
}