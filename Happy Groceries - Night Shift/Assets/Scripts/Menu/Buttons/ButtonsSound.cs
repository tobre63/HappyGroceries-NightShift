using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // Necessario para detectar o mouse

[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour, IPointerEnterHandler // Interface para hover
{
    [Header("Audio Settings")]
    public AudioClip clickSound;
    public AudioClip hoverSound; // Som ao passar o mouse

    [Header("Scene Settings")]
    public string sceneToLoad;

    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnButtonClick);
    }

    // Disparado automaticamente quando o mouse entra na area do botão
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            PlaySimpleSound(hoverSound);
        }
    }

    void OnButtonClick()
    {
        if (clickSound != null)
        {
            PlayPersistentSound(clickSound);
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // Toca som de forma que ele persista mesmo ao mudar de cena
    void PlayPersistentSound(AudioClip clip)
    {
        GameObject soundObj = new GameObject("ButtonClickSound_Temp");
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();

        DontDestroyOnLoad(soundObj);
        Destroy(soundObj, clip.length);
    }

    // Toca som apenas na cena atual (hover)
    void PlaySimpleSound(AudioClip clip)
    {
        GameObject soundObj = new GameObject("ButtonHoverSound_Temp");
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();

        Destroy(soundObj, clip.length);
    }
}