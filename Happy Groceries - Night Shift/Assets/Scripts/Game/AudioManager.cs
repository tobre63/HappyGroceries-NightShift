using UnityEngine;

// Gere a musica de fundo do jogo
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // Fonte dedicada a musica
    [SerializeField] private AudioSource sfxSource; // Fonte dedicada a efeitos sonoros

    [Header("Music Clips")]
    [SerializeField] private AudioClip background;      // Clip de musica de fundo

    private void Start()
    {
        // Garante que existe um clip atribuido antes de tocar
        if (background == null || musicSource == null) return;

        musicSource.clip = background;   // Define o clip de musica
        musicSource.loop = true;         // Mantem a musica em loop
        musicSource.Play();              // Inicia a reproducao
    }
}