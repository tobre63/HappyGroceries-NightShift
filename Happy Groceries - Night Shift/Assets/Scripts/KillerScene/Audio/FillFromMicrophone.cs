using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

public class FillFromMicrophone : MonoBehaviour
{
    public Image audioBar;
    public Slider sensitivitySlider;
    public AudioLoudnessDetector detector;

    public float minimumSensibility = 100;
    public float maximumSensibility = 1000;
    public float currentLoudnessSensibility = 500;
    public float threshold = 0.1f;

    public GameObject screamText;

    public static UnityAction OnScreamDetected;

    private void Start()
    {
        if (sensitivitySlider == null) return;

        sensitivitySlider.value = .5f;
        SetLoudnessSensibility(sensitivitySlider.value);
    }

    private void Update()
    {
        float loudness = detector.GetLoudnessFromMicrophone() * currentLoudnessSensibility;
        if (loudness < threshold) loudness = 0.01f;

        audioBar.fillAmount = loudness;

        if (loudness > .5f) OnScreamDetected?.Invoke();

        if (loudness > .5f && !screamText.activeInHierarchy) screamText.SetActive(true);
        if (loudness <= .5f && screamText.activeInHierarchy) screamText.SetActive(false);
    }

    public void SetLoudnessSensibility(float t)
    {
        currentLoudnessSensibility = Mathf.Lerp(minimumSensibility, maximumSensibility, t);
    }
}
