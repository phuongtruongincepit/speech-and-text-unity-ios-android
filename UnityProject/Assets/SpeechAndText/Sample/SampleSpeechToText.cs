using UnityEngine;
using UnityEngine.UI;
using TextSpeech;
using UnityEngine.Android;

public class SampleSpeechToText : MonoBehaviour
{
    public GameObject loading;
    public InputField inputSilenceLength;
    public InputField resultText;

    public Text txtLocale;

    void Start()
    {
        Setting("en-US", 0);
        loading.SetActive(false);
        SpeechToText.Instance.onResultsCallback = OnResultsSpeech;
#if UNITY_ANDROID
        Permission.RequestUserPermission(Permission.Microphone);
#else
        toggleShowPopupAndroid.gameObject.SetActive(false);
#endif

    }


    public void StartRecording()
    {
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
    public void Setting(string code, int silence)
    {
        txtLocale.text = "Locale: " + code + "\nSilence length: " + silence;
        SpeechToText.Instance.Setting(code, silence);
    }

    /// <summary>
    /// Button Click
    /// </summary>
    public void OnClickApply()
    {
        Setting("en-US", int.Parse(inputSilenceLength.text));
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
}
