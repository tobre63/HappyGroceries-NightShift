using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SimpleAutoSave : MonoBehaviour
{
    private void Start()
    {
        // Inicia o carregamento com um pequeno atraso para garantir 
        // que o NightTimer e o cenário já foram carregados
        StartCoroutine(LoadDataRoutine());
    }

    private IEnumerator LoadDataRoutine()
    {
        // Espera 1 frame. Isso é CRUCIAL para garantir que o NightTimer.Start() 
        // já rodou e não vai sobrescrever nosso load.
        yield return null;

        // -----------------------------------------------------
        // 1. CARREGAR POSIÇÃO
        // -----------------------------------------------------
        if (PlayerPrefs.HasKey("PlayerX"))
        {
            float x = PlayerPrefs.GetFloat("PlayerX");
            float y = PlayerPrefs.GetFloat("PlayerY");
            float z = PlayerPrefs.GetFloat("PlayerZ");

            // Desativa o CharacterController (se houver) para mover sem conflitos
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = new Vector3(x, y, z);

            // Carrega rotação (para o player não virar para a parede do nada)
            if (PlayerPrefs.HasKey("PlayerRotY"))
            {
                float rotY = PlayerPrefs.GetFloat("PlayerRotY");
                transform.rotation = Quaternion.Euler(0, rotY, 0);
            }

            if (cc != null) cc.enabled = true;
        }

        // -----------------------------------------------------
        // 2. CARREGAR TEMPO (Conectado ao NightTimer)
        // -----------------------------------------------------
        // Verifica se existe save E se o NightTimer está na cena
        if (PlayerPrefs.HasKey("SavedTime") && NightTimer.instance != null)
        {
            float savedTime = PlayerPrefs.GetFloat("SavedTime");

            // Atualiza a variável do seu script de tempo
            NightTimer.instance.currentTime = savedTime;

            Debug.Log($"Tempo carregado: {savedTime}");
        }
    }

    // Salva automaticamente ao sair, fechar ou mudar de cena
    private void OnDisable() => SaveGame();
    private void OnApplicationQuit() => SaveGame();

    public void SaveGame()
    {
        // 1. SALVAR POSIÇÃO
        PlayerPrefs.SetFloat("PlayerX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerY", transform.position.y);
        PlayerPrefs.SetFloat("PlayerZ", transform.position.z);
        PlayerPrefs.SetFloat("PlayerRotY", transform.rotation.eulerAngles.y);

        // 2. SALVAR CENA ATUAL (Para o botão Continue funcionar)
        PlayerPrefs.SetString("SavedScene", SceneManager.GetActiveScene().name);

        // 3. SALVAR TEMPO (Pega direto do NightTimer)
        if (NightTimer.instance != null)
        {
            PlayerPrefs.SetFloat("SavedTime", NightTimer.instance.currentTime);
        }

        PlayerPrefs.Save();
        Debug.Log("Jogo Salvo: Posição e Tempo (NightTimer) guardados.");
    }
}