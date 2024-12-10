using UnityEngine;
using UnityEngine.UI;
using TextSpeech;
using UnityEngine.Android;

public class SampleSpeechToText : MonoBehaviour
{
    public GameObject loading;
    public InputField inputSilenceLength;
    public InputField inputMinimumLength;
    public InputField resultText;
    public Button applyButton;
    public Text txtSettingsInfo;

    void Start()
    {
        Setting("en-US", 0, 0);
        loading.SetActive(false);
        SpeechToText.Instance.onResultsCallback = OnResultsSpeech;

#if UNITY_ANDROID
        Permission.RequestUserPermission(Permission.Microphone);
#else
        inputSilenceLength.gameObject.SetActive(false);
        inputMinimumLength.gameObject.SetActive(false);
        applyButton.gameObject.SetActive(false);
        txtSettingsInfo.gameObject.SetActive(false);
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
#if UNITY_IOS
        loading.SetActive(true);
#endif
    }
    void OnResultsSpeech(string _data)
    {
        resultText.text = _data;
#if UNITY_IOS
        loading.SetActive(false);
#endif
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
    public void Setting(string code, int silence, int minimum)
    {
        txtSettingsInfo.text = "Silence length: " + silence + "\nMinimum length: " + minimum;
        SpeechToText.Instance.Setting(code, silence, minimum);
    }

    /// <summary>
    /// Button Click
    /// </summary>
    public void OnClickApply()
    {
        Setting("en-US", int.Parse(inputSilenceLength.text), int.Parse(inputMinimumLength.text));
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
}
