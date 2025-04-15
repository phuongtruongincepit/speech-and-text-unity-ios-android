#if UNITY_EDITOR || UNITY_STANDALONE_WIN
using UnityEngine;
using UnityEngine.Windows.Speech;
using System;
using System.Collections;
using Unity.VisualScripting;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
public class SpeechToTextWindows
{
	private static SpeechToTextWindows instance;

	private DictationRecognizer dictationRecognizer;
	private int maxSpeechLength; // Maximum duration in ms
	private int minSpeechLength; // minimum duration in ms
	private int silenceLength;
	public Action<string> onRecognized = null; // Callback for recognized text
	private bool isStopProcessing;
    private DateTime timeStartListening; // Track the start time
	private DateTime lastTimeReturnResult;
    private TimerCallback callback;
    private System.Threading.Timer timer;
    private string partialResult;
    private string allResult;
    private SpeechToTextWindows()
	{
/*        if (!PhraseRecognitionSystem.isSupported)
        {
            Debug.LogError("Speech recognition is not supported on this system.");
            return;
        }*/

        dictationRecognizer = new DictationRecognizer
        {
            AutoSilenceTimeoutSeconds = 5f, // Stop recognition if 5 seconds of silence
            InitialSilenceTimeoutSeconds = 10f // Stop recognition if no speech starts in 10 seconds
        };
        dictationRecognizer.DictationResult += OnDictationResult;
        dictationRecognizer.DictationHypothesis += OnDictationHypothesis;
        dictationRecognizer.DictationComplete += OnDictationComplete;
        dictationRecognizer.DictationError += OnDictationError;
    }

    public static SpeechToTextWindows Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new SpeechToTextWindows();
			}
			return instance;
		}
	}

	public void SettingSpeechSilenceLength(int silenceDuration)
	{
		silenceLength = silenceDuration;
	}
    public void SettingSpeechMinimumLength(int minDuration)
    {
        minSpeechLength = minDuration;
    }
    public void SettingSpeechMaximumLength(int maxDuration)
    {
        maxSpeechLength = maxDuration;
    }
	private void StartTimer(int durationMs, Action onCompleted)
	{
        callback = _ => onCompleted?.Invoke();
        timer = new System.Threading.Timer(callback, null, durationMs, Timeout.Infinite);
    }
	private void StopTimer()
	{
		timer?.Dispose();
	}

	private void StartTempRecording()
	{
        dictationRecognizer.Start();
    }
    public void StartRecognition()
	{
        isStopProcessing = false;
        timeStartListening = DateTime.Now;
        lastTimeReturnResult = timeStartListening;
        allResult = "";
        partialResult = "";

		StartTempRecording();

        // Start timeout
        if (maxSpeechLength > 0)
		{
			Debug.Log("DictationRecognizer started, timeout start");
			StartTimer(maxSpeechLength, () =>
			{
				Debug.Log("Timeout Stop");
				StopRecognition();
			});
		}
		else
		{
            Debug.Log("DictationRecognizer started");
        }
		if (silenceLength > 0)
		{
			Task.Run(() =>
			{
				while (!isStopProcessing)
				{
					if (lastTimeReturnResult != timeStartListening)
					{
						if ((DateTime.Now - lastTimeReturnResult).TotalMilliseconds > silenceLength && (DateTime.Now - timeStartListening).TotalSeconds > minSpeechLength)
						{
							Debug.Log("Silence length exceed");
							StopRecognition();
							StopTimer();
						}
					}
					Thread.Sleep(100);
				}
			});
		}
    }

	public void StopRecognition()
	{
		if (!isStopProcessing)
		{
            Debug.Log("DictationRecognizer stopped, Time from start to stop recognition:" + (int)(DateTime.Now - timeStartListening).TotalMilliseconds);
			SetStopProcessAndShowResult();
            Debug.Log("Call dictationRecognizer.Stop()");
            dictationRecognizer.Stop();
        }
    }

	public void Dispose()
	{
		if (dictationRecognizer != null)
		{
			StopRecognition();
			dictationRecognizer.DictationResult -= OnDictationResult;
			dictationRecognizer.DictationHypothesis -= OnDictationHypothesis;
			dictationRecognizer.DictationComplete -= OnDictationComplete;
			dictationRecognizer.DictationError -= OnDictationError;
			dictationRecognizer.Dispose();
			dictationRecognizer = null;
		}

		instance = null;
	}

	private void OnDictationResult(string text, ConfidenceLevel confidence)
	{
        Debug.Log($"Recognized text: {text}");
        if (isStopProcessing)
			return;
		if (!string.IsNullOrEmpty(text))
		{
            
			allResult += " " + text;
			if (silenceLength > 0)
			{
				if ((DateTime.Now - lastTimeReturnResult).TotalMilliseconds > silenceLength && (DateTime.Now - timeStartListening).TotalMilliseconds > minSpeechLength)
				{
					StopTimer();
					SetStopProcessAndShowResult();
				}
			}
			else
			{
				StopTimer();
				SetStopProcessAndShowResult();
			}
        }
	}
    private void SetStopProcessAndShowResult()
    {
        isStopProcessing = true;
		ShowResult();
    }
	private void ShowResult()
	{
        Debug.Log("Show final result: " + (string.IsNullOrEmpty(allResult) ? partialResult : allResult));
        onRecognized?.Invoke(string.IsNullOrEmpty(allResult) ? partialResult : allResult);
    }

    private void OnDictationHypothesis(string text)
	{
        Debug.Log($"Hypothesis: {text}");
        if (isStopProcessing)
            return;
        
		if (!string.IsNullOrEmpty(text))
		{
			partialResult = text;
			if (silenceLength > 0 && lastTimeReturnResult != timeStartListening)
			{
				if ((DateTime.Now - lastTimeReturnResult).TotalMilliseconds > silenceLength && (DateTime.Now - timeStartListening).TotalMilliseconds > minSpeechLength)
				{
					StopRecognition();
					StopTimer();
					return;
				}
			}
			lastTimeReturnResult = DateTime.Now;
		}
    }

	private void OnDictationComplete(DictationCompletionCause cause)
	{
		Debug.Log($"Dictation completed: {cause}");
		//ShowResult();
	}

	private void OnDictationError(string error, int hresult)
	{
        if (isStopProcessing)
            return;
        Debug.LogError($"Dictation error: {error}");
        if (silenceLength > 0)
        {
            if (lastTimeReturnResult > timeStartListening)
			{
				if ((DateTime.Now - lastTimeReturnResult).TotalMilliseconds > silenceLength && (DateTime.Now - timeStartListening).TotalMilliseconds > minSpeechLength)
				{
					StopTimer();
					SetStopProcessAndShowResult();
				}
				else
				{
                    StartTempRecording();
                }
			}
			else
			{
                StartTempRecording();
            }
        }
    }
}
#endif