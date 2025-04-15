using UnityEngine;
using UnityEngine.UI;
using TextSpeech;
using System.Runtime.InteropServices;
using System;
using Unity.VisualScripting;
public class SampleTextToSpeech : MonoBehaviour
{
    [SerializeField]
    public Slider sliderPitch;
    [SerializeField]
    public Slider sliderRate;
    [SerializeField]
    public InputField inputText;
    [SerializeField]
    public Text txtSettingsInfo;
    void Start()
    {
        sliderPitch.minValue = 0.5f;
        sliderPitch.maxValue = 2.0f;
#if UNITY_ANDROID
        sliderRate.minValue = 0.5f;
        sliderRate.maxValue = 2.0f;
#elif UNITY_IOS
        sliderRate.minValue = 0.0f;
        sliderRate.maxValue = 1.0f;
#endif
        sliderPitch.value = 1.0f;
        sliderRate.value = 1.0f;
        Setting("en-US", 1.0f, 1.0f);
        //inputText.text = TextToSpeech.Instance.GetMessageFromJava().ToString();
    }
    void OnDestroy()
    {
    }

    public void Setting(string code, float pitch, float rate)
    {
        txtSettingsInfo.text = "Pitch: " + pitch + "\nRate: " + rate;
        TextToSpeech.Instance.Setting(code, pitch, rate);
    }
    public void OnClickSpeak()
    {
        TextToSpeech.Instance.StartSpeak(inputText.text);
    }

    /// <summary>
    /// </summary>
    public void OnClickStopSpeak()
    {
        TextToSpeech.Instance.StopSpeak();
    }
    public void OnClickApply()
    {
        Setting("en-US", sliderPitch.value, sliderRate.value);
    }
}