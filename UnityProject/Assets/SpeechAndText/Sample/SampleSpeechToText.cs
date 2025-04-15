using UnityEngine;
using UnityEngine.UI;
using TextSpeech;
using UnityEngine.Android;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
using UnityEngine.Windows.Speech;
#endif
public class SampleSpeechToText : MonoBehaviour
{
    public InputField inputSilenceLength;
    public InputField inputMinimumLength;
    public InputField inputMaximumLength;
    public InputField resultText;

    public Text txtSettingsInfo;
/*#if UNITY_EDITOR || UNITY_STANDALONE_WIN

    private DictationRecognizer dictationRecognizer;
    private void StartDictation()
    {
        if (dictationRecognizer.Status != SpeechSystemStatus.Running)
        {
            dictationRecognizer.Start();
            Debug.Log("DictationRecognizer started.");
        }
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log($"Recognized text: {text} (Confidence: {confidence})");
    }

    private void OnDictationHypothesis(string text)
    {
        Debug.Log($"Hypothesis: {text}");
    }

    private void OnDictationComplete(DictationCompletionCause cause)
    {
        if (cause != DictationCompletionCause.Complete)
        {
            Debug.LogError($"Dictation completed unsuccessfully: {cause}");
        }
        else
        {
            Debug.Log("Dictation completed successfully.");
        }

        // Restart dictation to listen for new speech
        StartDictation();
    }

    private void OnDictationError(string error, int hresult)
    {
        Debug.LogError($"Dictation error: {error}; HResult = {hresult}");

        // Restart dictation in case of an error
        StartDictation();
    }
    void OnDestroy()
    {
        if (dictationRecognizer != null)
        {
            dictationRecognizer.Stop();
            dictationRecognizer.Dispose();
            Debug.Log("DictationRecognizer stopped and disposed.");
        }
    }
#endif*/
    void Start()
    {
/*#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        // Initialize the DictationRecognizer
        Debug.Log($"Speech recognition supported: {PhraseRecognitionSystem.isSupported}");
        dictationRecognizer = new DictationRecognizer();

        dictationRecognizer.DictationResult += OnDictationResult;
        dictationRecognizer.DictationHypothesis += OnDictationHypothesis;
        dictationRecognizer.DictationComplete += OnDictationComplete;
        dictationRecognizer.DictationError += OnDictationError;

        StartDictation();
        Debug.Log("Dictation recognizer started. Start speaking...");
#else*/
        Setting("en-US", 0, 0, 0);
        SpeechToText.Instance.onResultsCallback = OnResultsSpeech;
#if UNITY_ANDROID
        Permission.RequestUserPermission(Permission.Microphone);
#endif
//#endif
    }


    public void StartRecording()
    {
        resultText.text = "";
        SpeechToText.Instance.StartRecording("Speak any");
    }

    public void StopRecording()
    {

        SpeechToText.Instance.StopRecording();
    }
    void OnResultsSpeech(string _data)
    {
        Debug.Log("OnResultsSpeech: " + _data);
        resultText.text = _data;
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
