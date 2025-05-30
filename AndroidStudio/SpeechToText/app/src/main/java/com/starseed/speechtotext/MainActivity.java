package com.starseed.speechtotext;

import android.content.Intent;

import android.os.Bundle;
import android.os.Handler;
import android.speech.RecognitionListener;
import android.speech.RecognizerIntent;
import android.speech.SpeechRecognizer;
import android.text.TextUtils;

import android.speech.tts.TextToSpeech;
import android.speech.tts.UtteranceProgressListener;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import java.util.ArrayList;
import java.util.Locale;

public class MainActivity extends UnityPlayerActivity
{
    private TextToSpeech tts;
    private SpeechRecognizer speech;
    private Intent intent;
    private Handler handler;
    private Long lastTimeReturnResult;
    private String allResult;

    private boolean isStopProcessing;
    private String partialResult;
    private Long timeStartListening;
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        handler = new Handler();
        tts = new TextToSpeech(this, initListener);
        speech = SpeechRecognizer.createSpeechRecognizer(this);
        speech.setRecognitionListener(recognitionListener);
    }
    @Override
    public void onDestroy() {
        if (tts != null) {
            tts.stop();
            tts.shutdown();
        }
        if (speech != null) {
            speech.destroy();
        }
        handler.removeCallbacksAndMessages(null);
        super.onDestroy();
    }

    public void OnShowVersion()
    {
        UnityPlayer.UnitySendMessage("SpeechToText", "onShowVersion", "1.0.1");
    }
    private void StartTempRecording() {
        intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, Bridge.language);
        intent.putExtra(RecognizerIntent.EXTRA_PARTIAL_RESULTS, true);
        this.runOnUiThread(new Runnable() {

            @Override
            public void run() {
                speech.startListening(intent);
            }
        });
    }
    // speech to text
    public void OnStartRecording() {
        lastTimeReturnResult = 0L;
        allResult = "";
        partialResult = "";
        isStopProcessing = false;
        timeStartListening = System.currentTimeMillis();
        StartTempRecording();
        if (Bridge.maximumLengthMs > 0) {
            handler.postDelayed(new Runnable() {
                @Override
                public void run() {
                    OnStopRecording();
                }
            }, Bridge.maximumLengthMs);
        }
        UnityPlayer.UnitySendMessage("SpeechToText", "onMessage", "CallStart, Language: " + Bridge.language + ", Silence length (ms): " + Bridge.completeSilenceLengthMs + ", Minimum length (ms): " + Bridge.minimumLengthMs + ", Maximum length (ms): " + Bridge.maximumLengthMs);
    }
    public void OnStopRecording() {
        if (!isStopProcessing) {
            this.runOnUiThread(new Runnable() {

                @Override
                public void run() {
                    speech.stopListening();
                    SetStopProcessAndShowResult();
                }
            });
            UnityPlayer.UnitySendMessage("SpeechToText", "onMessage", "CallStop");
        }
    }
    private void SetStopProcessAndShowResult() {
        isStopProcessing = true;
        UnityPlayer.UnitySendMessage("SpeechToText", "onResults", allResult.isEmpty() ? partialResult : allResult);
    }

    RecognitionListener recognitionListener = new RecognitionListener() {

        @Override
        public void onReadyForSpeech(Bundle params) {
            UnityPlayer.UnitySendMessage("SpeechToText", "onReadyForSpeech", params.toString());
        }
        @Override
        public void onBeginningOfSpeech() {
            UnityPlayer.UnitySendMessage("SpeechToText", "onBeginningOfSpeech", "");
        }
        @Override
        public void onRmsChanged(float rmsdB) {
            UnityPlayer.UnitySendMessage("SpeechToText", "onRmsChanged", "" + rmsdB);
            if (isStopProcessing)
                return;
            //Log.i("TAG_NATIVE", "onRmsChanged $rmsdB")
            if (Bridge.completeSilenceLengthMs > 0 && lastTimeReturnResult > 0L) {
                if (System.currentTimeMillis() - lastTimeReturnResult > Bridge.completeSilenceLengthMs && System.currentTimeMillis() - timeStartListening > Bridge.minimumLengthMs) {
                    OnStopRecording();
                    handler.removeCallbacksAndMessages(null);
                }
            }
        }
        @Override
        public void onBufferReceived(byte[] buffer) {
            UnityPlayer.UnitySendMessage("SpeechToText", "onMessage", "onBufferReceived: " + buffer.length);
        }
        @Override
        public void onEndOfSpeech() {
            UnityPlayer.UnitySendMessage("SpeechToText", "onEndOfSpeech", "");
        }
        @Override
        public void onError(int error) {
            if (isStopProcessing)
                return;

            if (error == SpeechRecognizer.ERROR_NO_MATCH || error == SpeechRecognizer.ERROR_SPEECH_TIMEOUT) {
                if (Bridge.completeSilenceLengthMs > 0) {
                    if (lastTimeReturnResult > 0L) {
                        if (System.currentTimeMillis() - lastTimeReturnResult > Bridge.completeSilenceLengthMs && System.currentTimeMillis() - timeStartListening > Bridge.minimumLengthMs) {
                            handler.removeCallbacksAndMessages(null);
                            SetStopProcessAndShowResult();
                        } else {
                            StartTempRecording();
                        }
                    } else {
                        StartTempRecording();
                    }
                }


            } else {
                UnityPlayer.UnitySendMessage("SpeechToText", "onError", "" + error);
            }

        }
        @Override
        public void onResults(Bundle results) {
            if (isStopProcessing)
                return;
            if (results != null)
            {
                ArrayList<String> text = results.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
                if (text != null && !text.isEmpty()) {
                    String currentResult = TextUtils.join(" ", text);
                    allResult += " " + currentResult;
                    if (Bridge.completeSilenceLengthMs > 0) {
                        if (System.currentTimeMillis() - lastTimeReturnResult > Bridge.completeSilenceLengthMs && System.currentTimeMillis() - timeStartListening > Bridge.minimumLengthMs) {
                            handler.removeCallbacksAndMessages(null);
                            SetStopProcessAndShowResult();
                        } else {
                            StartTempRecording();
                        }
                    } else {
                        handler.removeCallbacksAndMessages(null);
                        SetStopProcessAndShowResult();
                    }
                }
            }
        }
        @Override
        public void onPartialResults(Bundle partialResults) {
            if (isStopProcessing)
                return;
            if (partialResults != null)
            {
                ArrayList<String> text = partialResults.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
                if (text != null && !text.isEmpty()) {
                    partialResult = TextUtils.join(" ", text);
                    if (Bridge.completeSilenceLengthMs > 0 && lastTimeReturnResult > 0L) {
                        if (System.currentTimeMillis() - lastTimeReturnResult > Bridge.completeSilenceLengthMs && System.currentTimeMillis() - timeStartListening > Bridge.minimumLengthMs) {
                            OnStopRecording();
                            handler.removeCallbacksAndMessages(null);
                            return;
                        }
                    }
                    lastTimeReturnResult = System.currentTimeMillis();
                }
            }
        }
        @Override
        public void onEvent(int eventType, Bundle params) {
            UnityPlayer.UnitySendMessage("SpeechToText", "onMessage", "onEvent");
        }

    };


    ////
    public  void OnStartSpeak(String valueText)
    {
        tts.speak(valueText, TextToSpeech.QUEUE_FLUSH, null, valueText);
    }
    public void OnSettingSpeak(String language, float pitch, float rate) {
        tts.setPitch(pitch);
        tts.setSpeechRate(rate);
        int result = tts.setLanguage(getLocaleFromString(language));
        UnityPlayer.UnitySendMessage("TextToSpeech", "onSettingResult", "Pitch " + pitch + ", rate: " + rate + ". Success: " + result);
    }
    public void OnStopSpeak()
    {
        tts.stop();
    }

    TextToSpeech.OnInitListener initListener = new TextToSpeech.OnInitListener()
    {
        @Override
        public void onInit(int status) {
            if (status == TextToSpeech.SUCCESS)
            {
                OnSettingSpeak(Locale.US.toString(), 1.0f, 1.0f);
                tts.setOnUtteranceProgressListener(utteranceProgressListener);
            }
        }
    };

    UtteranceProgressListener utteranceProgressListener=new UtteranceProgressListener() {
        @Override
        public void onStart(String utteranceId) {
            UnityPlayer.UnitySendMessage("TextToSpeech", "onStart", utteranceId);
        }
        @Override
        public void onError(String utteranceId) {
            UnityPlayer.UnitySendMessage("TextToSpeech", "onError", utteranceId);
        }
        @Override
        public void onDone(String utteranceId) {
            UnityPlayer.UnitySendMessage("TextToSpeech", "onDone", utteranceId);
        }
    };
    /**
     * Convert a string based locale into a Locale Object.
     * Assumes the string has form "{language}_{country}_{variant}".
     * Examples: "en", "de_DE", "_GB", "en_US_WIN", "de__POSIX", "fr_MAC"
     *
     * @param localeString The String
     * @return the Locale
     */
    public static Locale getLocaleFromString(String localeString)
    {
        if (localeString == null)
        {
            return null;
        }
        localeString = localeString.trim();
        if (localeString.equalsIgnoreCase("default"))
        {
            return Locale.getDefault();
        }

        // Extract language
        int languageIndex = localeString.indexOf('_');
        String language;
        if (languageIndex == -1)
        {
            // No further "_" so is "{language}" only
            return new Locale(localeString, "");
        }
        else
        {
            language = localeString.substring(0, languageIndex);
        }

        // Extract country
        int countryIndex = localeString.indexOf('_', languageIndex + 1);
        String country;
        if (countryIndex == -1)
        {
            // No further "_" so is "{language}_{country}"
            country = localeString.substring(languageIndex+1);
            return new Locale(language, country);
        }
        else
        {
            // Assume all remaining is the variant so is "{language}_{country}_{variant}"
            country = localeString.substring(languageIndex+1, countryIndex);
            String variant = localeString.substring(countryIndex+1);
            return new Locale(language, country, variant);
        }
    }
}

/*
to build: Build\Rebuild Project
then file classes.jar will appear in AndroidStudio\SpeechToText\app\build\intermediates\aar_main_jar\release
Copy this file and paste to Plugin folder in Unity, when you update this class.jar in unity, please close unity editor and open again to make this new class.jar be loaded.
* */