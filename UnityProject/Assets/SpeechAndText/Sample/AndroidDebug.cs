using UnityEngine;
using UnityEngine.UI;
using TextSpeech;

public class AndroidDebug : MonoBehaviour
{
    public Text txtLog;
    public Text txtNewLog;
    void Start()
    {
#if UNITY_ANDROID
        SpeechToText.Instance.onReadyForSpeechCallback = onReadyForSpeechCallback;
        SpeechToText.Instance.onEndOfSpeechCallback = onEndOfSpeechCallback;
        SpeechToText.Instance.onBeginningOfSpeechCallback = onBeginningOfSpeechCallback;
        SpeechToText.Instance.onErrorCallback = onErrorCallback;
        SpeechToText.Instance.onPartialResultsCallback = onPartialResultsCallback;
        SpeechToText.Instance.onShowVersionCallback = onShowVersionCallback;
        SpeechToText.Instance.onMessageCallback = onMessageCallback;
#else
        gameObject.SetActive(false);
#endif
    }

    void AddLog(string log)
    {
        txtLog.text += "\n" + log;
        txtNewLog.text = log;
        Debug.Log(log);
    }

    void onShowVersionCallback(string _data)
    {
        AddLog("Version: " + _data);
    }
    void onMessageCallback(string _data)
    {
        AddLog("Message: " + _data);
    }

    void onReadyForSpeechCallback(string _params)
    {
        AddLog("Ready for the user to start speaking");
    }
    void onEndOfSpeechCallback()
    {
        AddLog("User stops speaking");
    }
    void onBeginningOfSpeechCallback()
    {
        AddLog("User has started to speak");
    }
    void onErrorCallback(string _params)
    {
        AddLog("Error: " + _params);
    }
    void onPartialResultsCallback(string _params)
    {
        AddLog("Partial recognition results are available " + _params);
    }
}
