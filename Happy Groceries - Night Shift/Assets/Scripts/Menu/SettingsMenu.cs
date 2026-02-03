using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    // Ativa ou desativa o fullscreen
    public void SetFullScreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen;
    }

}