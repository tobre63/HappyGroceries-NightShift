using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Slider generalSlider;

    [Header("Tabs")]
    public GameObject[] Tabs;
    public Image[] TabButtons;
    public Color inactiveTabColor = new Color(87f / 255f, 31f / 255f, 31f / 255f, 1f);
    public Color activeTabColor = new Color(99f / 255f, 48f / 255f, 48f / 255f, 1f);
    public Vector2 InactiveTabButtonSize;
    public Vector2 ActiveTabButtonSize;

    [Header("Graphic Settings")]
    public TMP_Dropdown ResDropDown;
    public SliderToggle fullscreenToggle;
    public SliderToggle vsyncToggle;

    Resolution[] AllResolutions;
    int SelectedResolution;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    void Start()
    {
    // 1. CARREGAR O �UDIO
    if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
            SetGeneralVolume();
        }

    // 2. CONFIGURAR E CARREGAR OS GR�FICOS
    SetupResolutions();
        LoadGraphicsSettings();
    }

    // ==========================================
    //� � � � � � � � � �UDIO
    // ==========================================

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
        PlayerPrefs.Save(); // Adicionado
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        myMixer.SetFloat("sfx", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save(); // Adicionado
    }

    public void SetGeneralVolume()
    {
        float volume = generalSlider.value;
        myMixer.SetFloat("general", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("generalVolume", volume);
        PlayerPrefs.Save(); // Adicionado
    }

    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        generalSlider.value = PlayerPrefs.GetFloat("generalVolume");

        SetMusicVolume();
        SetSFXVolume();
        SetGeneralVolume();
    }

    // ==========================================
    //� � � � � � TABS (SEPARADORES)
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
    //� � � � � � � �GR�FICOS
    // ==========================================

    private void SetupResolutions()
    {
        AllResolutions = Screen.resolutions;
        ResDropDown.ClearOptions();

        List<string> resolutionStringList = new List<string>();
        SelectedResolutionList = new List<Resolution>();
        // SOLU��O 1: Percorrer a lista de tr�s para a frente.
        // Assim garantimos que apanhamos sempre a vers�o com os Hertz (Hz) mais altos para cada resolu��o!
        for (int i = AllResolutions.Length - 1; i >= 0; i--)
        {
            string newRes = AllResolutions[i].width + " x " + AllResolutions[i].height;

            if (!resolutionStringList.Contains(newRes))
            {
                resolutionStringList.Add(newRes);
                SelectedResolutionList.Add(AllResolutions[i]);
            }
        }

    // Inverter as listas para que a apresenta��o no Dropdown fique natural (da menor resolu��o para a maior)
        resolutionStringList.Reverse();
        SelectedResolutionList.Reverse();

        ResDropDown.AddOptions(resolutionStringList);
    }

    public void SetFullscreenOn()
    {
        // SOLU��O 2: Usar ExclusiveFullScreen.�
        // D� controlo absoluto � GPU e evita que o Windows estrague as cores ao redimensionar.
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        PlayerPrefs.SetInt("isFullscreen", 1);
        PlayerPrefs.Save();
    }

    public void SetFullscreenOff()
    {
        Screen.fullScreenMode = FullScreenMode.Windowed;
        PlayerPrefs.SetInt("isFullscreen", 0);
        PlayerPrefs.Save(); // Adicionado
    }

    public void SetVSyncOn()
    {
        QualitySettings.vSyncCount = 1;
        PlayerPrefs.SetInt("isVSync", 1);
        PlayerPrefs.Save(); // Adicionado
    }

    public void SetVSyncOff()
    {
        QualitySettings.vSyncCount = 0;
        PlayerPrefs.SetInt("isVSync", 0);
        PlayerPrefs.Save(); // Adicionado
    }

    public void ChangeResolution()
    {
        SelectedResolution = ResDropDown.value;
        Resolution res = SelectedResolutionList[SelectedResolution];

        // Passamos o fullScreenMode atual diretamente
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);

        PlayerPrefs.SetInt("resolutionIndex", SelectedResolution);
        PlayerPrefs.Save();
    }

    // --- A MAGIA DO SAVE DOS GR�FICOS ---
    private void LoadGraphicsSettings()
    {
        bool isFullscreen = PlayerPrefs.GetInt("isFullscreen", 1) == 1;
        if (isFullscreen) SetFullscreenOn();
        else SetFullscreenOff();

        if (fullscreenToggle != null) fullscreenToggle.SetStateWithoutNotify(isFullscreen);

        bool isVSync = PlayerPrefs.GetInt("isVSync", 1) == 1;
        if (isVSync) SetVSyncOn();
        else SetVSyncOff();

        if (vsyncToggle != null) vsyncToggle.SetStateWithoutNotify(isVSync);

        if (PlayerPrefs.HasKey("resolutionIndex"))
        {
            int savedResIndex = PlayerPrefs.GetInt("resolutionIndex");

            if (savedResIndex >= 0 && savedResIndex < SelectedResolutionList.Count)
            {
                // Importante: Alterar o valor do dropdown invoca o evento OnValueChanged automaticamente se estiver associado no Inspector!
                ResDropDown.value = savedResIndex;
                ChangeResolution();
            }
        }
        else
        {
            for (int i = 0; i < SelectedResolutionList.Count; i++)
            {
                if (SelectedResolutionList[i].width == Screen.currentResolution.width &&
                  SelectedResolutionList[i].height == Screen.currentResolution.height)
                {
                    ResDropDown.value = i;
                    ChangeResolution();
                    break;
                }
            }
        }

        ResDropDown.RefreshShownValue();
    }
}