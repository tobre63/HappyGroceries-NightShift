using UnityEngine;

public class PersistentSound : MonoBehaviour
{
    // Toca um AudioClip que persiste mesmo ao mudar de cena
    public static void PlayClip(AudioClip clip)
    {
        // Cria um objeto temporario para tocar o som
        GameObject tempGO = new GameObject("TempAudio");
        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.Play();

        // Mantem o objeto ao mudar de cena
        DontDestroyOnLoad(tempGO);

        // Destroi o objeto apos o som terminar
        Destroy(tempGO, clip.length);
    }
}