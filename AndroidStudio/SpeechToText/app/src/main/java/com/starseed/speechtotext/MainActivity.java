package com.starseed.speechtotext;

import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.speech.RecognitionListener;
import android.speech.RecognizerIntent;
import android.speech.SpeechRecognizer;
import android.text.TextUtils;

import androidx.annotation.RequiresApi;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import java.util.ArrayList;
import java.util.Locale;

public class MainActivity extends UnityPlayerActivity
{
    private SpeechRecognizer speech;
    private Intent intent;
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);


        speech = SpeechRecognizer.createSpeechRecognizer(this);
        speech.setRecognitionListener(recognitionListener);
    }
    @Override
    public void onDestroy() {
        if (speech != null) {
            speech.destroy();
        }
        super.onDestroy();
    }

    public void OnShowVersion()
    {
        UnityPlayer.UnitySendMessage("SpeechToText", "onShowVersion", "1.0.1");
    }
    // speech to text
    public void OnStartRecording() {
        /*intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_PREFERENCE, Bridge.languageSpeech);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, Bridge.languageSpeech);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, Bridge.languageSpeech);
        intent.putExtra(RecognizerIntent.EXTRA_PARTIAL_RESULTS, true);
        intent.putExtra(RecognizerIntent.EXTRA_SPEECH_INPUT_COMPLETE_SILENCE_LENGTH_MILLIS, 2000);
        intent.putExtra(RecognizerIntent.EXTRA_CALLING_PACKAGE, this.getPackageName());
        intent.putExtra(RecognizerIntent.EXTRA_MAX_RESULTS, 3);*/
        intent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
        intent.putExtra(RecognizerIntent.EXTRA_LANGUAGE, Bridge.language);
        if (Bridge.completeSilenceLengthMs > 0) {
/*            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                intent.putExtra(RecognizerIntent.EXTRA_SEGMENTED_SESSION, true);
            }*/
            intent.putExtra(RecognizerIntent.EXTRA_SPEECH_INPUT_COMPLETE_SILENCE_LENGTH_MILLIS, new Long(Bridge.completeSilenceLengthMs));
        }
        if (Bridge.minimumLengthMs > 0) {
/*            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
                intent.putExtra(RecognizerIntent.EXTRA_SEGMENTED_SESSION, true);
            }*/
            intent.putExtra(RecognizerIntent.EXTRA_SPEECH_INPUT_MINIMUM_LENGTH_MILLIS, new Long(Bridge.minimumLengthMs));
        }
        intent.putExtra(RecognizerIntent.EXTRA_PARTIAL_RESULTS, true);

        this.runOnUiThread(new Runnable() {

            @Override
            public void run() {
                speech.startListening(intent);
            }
        });
        UnityPlayer.UnitySendMessage("SpeechToText", "onMessage", "CallStart, Language: " + Bridge.language + ", Silence length (ms): " + Bridge.completeSilenceLengthMs + ", Minimum length (ms): " + Bridge.minimumLengthMs);
    }
    public void OnStopRecording() {
        this.runOnUiThread(new Runnable() {

            @Override
            public void run() {
                speech.stopListening();
            }
        });
        UnityPlayer.UnitySendMessage("SpeechToText", "onMessage", "CallStop");
    }

    public void onLog(String m)
    {
        UnityPlayer.UnitySendMessage("SpeechToText", "onLog", m.toString());
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
            UnityPlayer.UnitySendMessage("SpeechToText", "onError", "" + error);
        }
        @Override
        public void onResults(Bundle results) {
            if (results != null)
            {
                ArrayList<String> text = results.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
                if (text != null && !text.isEmpty()) {
                    UnityPlayer.UnitySendMessage("SpeechToText", "onResults", TextUtils.join(" ", text));
                }
            }
        }
        @Override
        public void onPartialResults(Bundle partialResults) {
            if (partialResults != null)
            {
                ArrayList<String> text = partialResults.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
                if (text != null && !text.isEmpty()) {
                    UnityPlayer.UnitySendMessage("SpeechToText", "onPartialResults", TextUtils.join(" ", text));
                }
            }
        }
        @Override
        public void onEvent(int eventType, Bundle params) {
            UnityPlayer.UnitySendMessage("SpeechToText", "onMessage", "onEvent");
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