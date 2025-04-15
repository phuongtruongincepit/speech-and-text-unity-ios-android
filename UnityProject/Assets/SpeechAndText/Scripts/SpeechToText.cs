using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System;

namespace TextSpeech
{
    public class SpeechToText : MonoBehaviour
    {

        #region Init
        private static SpeechToText _instance;
        public static SpeechToText Instance
        {
            get
            {
                if (_instance == null)
                {
                    //Create if it doesn't exist
                    GameObject go = new GameObject("SpeechToText");
                    _instance = go.AddComponent<SpeechToText>();
                    
                }
                return _instance;
            }
        }


        void Awake()
        {
            _instance = this;
        }
        #endregion

        public Action<string> onResultsCallback;

        public void Setting(string _language, int _silenceLength, int _minimumLength, int _maximumLength)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            SpeechToTextWindows.Instance.SettingSpeechSilenceLength(_silenceLength);
            SpeechToTextWindows.Instance.SettingSpeechMinimumLength(_minimumLength);
            SpeechToTextWindows.Instance.SettingSpeechMaximumLength(_maximumLength);
            if (SpeechToTextWindows.Instance.onRecognized == null)
            {
                SpeechToTextWindows.Instance.onRecognized = onResults;
            }
            
#elif UNITY_IPHONE
        _TAG_SettingSpeech(_language);
        _TAG_SettingSpeechSilenceLength(_silenceLength);  
        _TAG_SettingSpeechMinimumLength(_minimumLength);
        _TAG_SettingSpeechMaximumLength(_maximumLength);
#elif UNITY_ANDROID
        AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.starseed.speechtotext.Bridge");
        javaUnityClass.CallStatic("SettingSpeechToTextLanguage", _language);
        javaUnityClass.CallStatic("SettingSpeechToTextCompleteSilenceLengthMs", _silenceLength);
        javaUnityClass.CallStatic("SettingSpeechToTextMinimumLengthMs", _minimumLength);
        javaUnityClass.CallStatic("SettingSpeechToTextMaximumLengthMs", _maximumLength);
#endif
        }
        public void StartRecording(string _message = "")
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            SpeechToTextWindows.Instance.StartRecognition();
#elif UNITY_IPHONE
        _TAG_startRecording();
#elif UNITY_ANDROID
        AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.starseed.speechtotext.Bridge");
        javaUnityClass.CallStatic("StartRecording");
#endif
        }
        public void StopRecording()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            SpeechToTextWindows.Instance.StopRecognition();
#elif UNITY_IPHONE
        _TAG_stopRecording();
#elif UNITY_ANDROID
        AndroidJavaClass javaUnityClass = new AndroidJavaClass("com.starseed.speechtotext.Bridge");
        javaUnityClass.CallStatic("StopRecording");
#endif
        }

#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern void _TAG_startRecording();

        [DllImport("__Internal")]
        private static extern void _TAG_stopRecording();

        [DllImport("__Internal")]
        private static extern void _TAG_SettingSpeech(string _language);
        
        [DllImport("__Internal")]
        private static extern void _TAG_SettingSpeechSilenceLength(int _silenceInMs);
        
        [DllImport("__Internal")]
        private static extern void _TAG_SettingSpeechMinimumLength(int _minimumInMs);
        
        [DllImport("__Internal")]
        private static extern void _TAG_SettingSpeechMaximumLength(int _maximumInMs);
      
       
#endif


        public void onErrorMessage(string _message)
        {
            Debug.Log(_message);
        }
        /** Called when recognition results are ready. */
        public void onResults(string _results)
        {
            Debug.Log("onResults: " + _results);
            onResultsCallback?.Invoke(_results);
        }

        #region Android STT custom
#if UNITY_ANDROID
        #region Error Code
        /** Network operation timed out. */
        public const int ERROR_NETWORK_TIMEOUT = 1;
        /** Other network related errors. */
        public const int ERROR_NETWORK = 2;
        /** Audio recording error. */
        public const int ERROR_AUDIO = 3;
        /** Server sends error status. */
        public const int ERROR_SERVER = 4;
        /** Other client side errors. */
        public const int ERROR_CLIENT = 5;
        /** No speech input */
        public const int ERROR_SPEECH_TIMEOUT = 6;
        /** No recognition result matched. */
        public const int ERROR_NO_MATCH = 7;
        /** RecognitionService busy. */
        public const int ERROR_RECOGNIZER_BUSY = 8;
        /** Insufficient permissions */
        public const int ERROR_INSUFFICIENT_PERMISSIONS = 9;
        /////////////////////
        String getErrorText(int errorCode)
        {
            String message;
            switch (errorCode)
            {
                case ERROR_AUDIO:
                    message = "Audio recording error";
                    break;
                case ERROR_CLIENT:
                    message = "Client side error";
                    break;
                case ERROR_INSUFFICIENT_PERMISSIONS:
                    message = "Insufficient permissions";
                    break;
                case ERROR_NETWORK:
                    message = "Network error";
                    break;
                case ERROR_NETWORK_TIMEOUT:
                    message = "Network timeout";
                    break;
                case ERROR_NO_MATCH:
                    message = "No match";
                    break;
                case ERROR_RECOGNIZER_BUSY:
                    message = "RecognitionService busy";
                    break;
                case ERROR_SERVER:
                    message = "error from server";
                    break;
                case ERROR_SPEECH_TIMEOUT:
                    message = "No speech input";
                    break;
                default:
                    message = "Didn't understand, please try again.";
                    break;
            }
            return message;
        }
        #endregion

        public Action<string> onReadyForSpeechCallback;
        public Action onEndOfSpeechCallback;
        public Action<float> onRmsChangedCallback;
        public Action onBeginningOfSpeechCallback;
        public Action<string> onErrorCallback;
        public Action<string> onMessageCallback;
        public Action<string> onPartialResultsCallback;
        public Action<string> onShowVersionCallback;
        public void onShowVersion(string _params)
        {
            onShowVersionCallback?.Invoke(_params);
        }
        /** Called when the endpointer is ready for the user to start speaking. */
        public void onReadyForSpeech(string _params)
        {
            onReadyForSpeechCallback?.Invoke(_params);
        }
        /** Called after the user stops speaking. */
        public void onEndOfSpeech(string _paramsNull)
        {

            onEndOfSpeechCallback?.Invoke();
        }
        /** The sound level in the audio stream has changed. */
        public void onRmsChanged(string _value)
        {
            float _rms = float.Parse(_value);
            onRmsChangedCallback?.Invoke(_rms);
        }

        /** The user has started to speak. */
        public void onBeginningOfSpeech(string _paramsNull)
        {
            onBeginningOfSpeechCallback?.Invoke();
        }

        /** A network or recognition error occurred. */
        public void onError(string _value)
        {
            int _error = int.Parse(_value);
            string _message = getErrorText(_error);
            Debug.Log(_message);
            onErrorCallback?.Invoke(_message);
        }
        /** Called when partial recognition results are available. */
        public void onPartialResults(string _params)
        {
            onPartialResultsCallback?.Invoke(_params);
        }
        public void onMessage(string _message)
        {
            onMessageCallback?.Invoke(_message);
        }
#endif
        #endregion
    }
}
