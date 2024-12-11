using UnityEngine;
using UnityEngine.UI;
using TextSpeech;
using UnityEngine.Android;

public class SampleSpeechToText : MonoBehaviour
{
    public InputField inputSilenceLength;
    public InputField inputMinimumLength;
    public InputField inputMaximumLength;
    public InputField resultText;

    public Text txtSettingsInfo;

    void Start()
    {
        Setting("en-US", 0, 0, 0);
        SpeechToText.Instance.onResultsCallback = OnResultsSpeech;
#if UNITY_ANDROID
        Permission.RequestUserPermission(Permission.Microphone);
#endif

    }


    public void StartRecording()
    {
        resultText.text = "";
#if UNITY_EDITOR
#else
        SpeechToText.Instance.StartRecording("Speak any");
#endif
    }

    public void StopRecording()
    {
#if UNITY_EDITOR
        OnResultsSpeech("Not support in editor.");
#else
        SpeechToText.Instance.StopRecording();
#endif
    }
    void OnResultsSpeech(string _data)
    {
        resultText.text = _data;
    }
    public void OnClickSpeak()
    {
        TextToSpeech.Instance.StartSpeak(resultText.text);
    }

    /// <summary>
    /// </summary>
    public void  OnClickStopSpeak()
    {
        TextToSpeech.Instance.StopSpeak();
    }

    /// <summary>
    /// </summary>
    /// <param name="code"></param>
    public void Setting(string code, int silence, int minimum, int maximum)
    {
        txtSettingsInfo.text = "Silence length: " + silence + "\nMinimum length: " + minimum + "\nMaximum length: " + maximum;
        SpeechToText.Instance.Setting(code, silence, minimum, maximum);
    }

    /// <summary>
    /// Button Click
    /// </summary>
    public void OnClickApply()
    {
        Setting("en-US", int.Parse(inputSilenceLength.text), int.Parse(inputMinimumLength.text), int.Parse(inputMaximumLength.text));
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
}
