using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

public class MicrophoneSelector : MonoBehaviour
{
    public TMP_Dropdown sourceDropdown;
    public int chosenDeviceIndex = 0;

    public static UnityAction<int> OnMicrophoneChoiceChanged;

    void Start()
    {
        PopulateSourceDropdown();
    }

    void PopulateSourceDropdown()
    {
        var options = new List<TMP_Dropdown.OptionData>();

        foreach (var microphone in Microphone.devices)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(microphone, null, Color.black);

            options.Add(optionData);
        }

        sourceDropdown.options = options;
    }

    public void ChooseMicrophone(int optionIndex)
    {
        chosenDeviceIndex = optionIndex;

        OnMicrophoneChoiceChanged?.Invoke(chosenDeviceIndex);
    }
}