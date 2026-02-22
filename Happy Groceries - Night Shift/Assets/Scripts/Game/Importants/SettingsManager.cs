using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("--- ÁUDIO ---")]
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider generalSlider;

    [Header("--- TABS ---")]
    public GameObject[] Tabs;
    public Image[] TabButtons;
    public Color inactiveTabColor = new Color(87f / 255f, 31f / 255f, 31f / 255f, 1f);
    public Color activeTabColor = new Color(99f / 255f, 48f / 255f, 48f / 255f, 1f);
    public Vector2 InactiveTabButtonSize;
    public Vector2 ActiveTabButtonSize;

    void Start()
    {
        // 1. CARREGAR O ÁUDIO
        // Bug corrigido: Usar && e garantir que carrega corretamente
        if (PlayerPrefs.HasKey("musicVolume") && PlayerPrefs.HasKey("SFXVolume") && PlayerPrefs.HasKey("generalVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
            SetGeneralVolume();
        }
    }

    // ==========================================
    //                  ÁUDIO
    // ==========================================

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        myMixer.SetFloat("sfx", Mathf.Log10(volume) * 20);
        // Bug corrigido: Agora guarda na variável certa "SFXVolume"
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void SetGeneralVolume()
    {
        float volume = generalSlider.value;
        myMixer.SetFloat("general", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("generalVolume", volume);
    }

    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        generalSlider.value = PlayerPrefs.GetFloat("generalVolume");

        SetMusicVolume();
        SetSFXVolume();
        SetGeneralVolume(); // Bug corrigido: Faltava atualizar o General!
    }

    // ==========================================
    //            TABS (SEPARADORES)
    // ==========================================

    public void SwitchToTab(int TabID)
    {
        foreach (GameObject go in Tabs)
        {
            go.SetActive(false);
        }
        Tabs[TabID].SetActive(true);

        foreach (Image im in TabButtons)
        {
            im.color = inactiveTabColor;
            im.rectTransform.sizeDelta = InactiveTabButtonSize;
        }

        TabButtons[TabID].color = activeTabColor;
        TabButtons[TabID].rectTransform.sizeDelta = ActiveTabButtonSize;
    }

    // ==========================================
    //               GRÁFICOS
    // ==========================================

    public void SetFullscreenOn()
    {
        Screen.fullScreen = true;
        Debug.Log("Fullscreen LIGADO");
    }

    public void SetFullscreenOff()
    {
        Screen.fullScreen = false;
        Debug.Log("Fullscreen DESLIGADO");
    }

    public void SetVSyncOn()
    {
        QualitySettings.vSyncCount = 1;
        Debug.Log("VSync LIGADO");
    }

    public void SetVSyncOff()
    {
        QualitySettings.vSyncCount = 0;
        Debug.Log("VSync DESLIGADO");
    }
}