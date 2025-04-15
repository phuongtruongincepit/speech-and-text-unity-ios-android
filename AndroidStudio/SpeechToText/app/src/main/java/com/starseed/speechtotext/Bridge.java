package com.starseed.speechtotext;

import com.unity3d.player.UnityPlayer;

/**
 * Created by J1mmyTo9
 */
public class Bridge {

    // Speak To Text
    protected static String language = "en-US";
    protected static int completeSilenceLengthMs = 0;
    protected static int minimumLengthMs = 0;
    protected static int maximumLengthMs = 0;

    public static void SettingSpeechToTextLanguage(String language){
        Bridge.language = language;
        MainActivity activity = (MainActivity)UnityPlayer.currentActivity;
        activity.OnShowVersion();
    }
    public static void SettingSpeechToTextCompleteSilenceLengthMs(int ms){
        Bridge.completeSilenceLengthMs = ms;
    }
    public static void SettingSpeechToTextMinimumLengthMs(int ms){
        Bridge.minimumLengthMs = ms;
    }
    public static void SettingSpeechToTextMaximumLengthMs(int ms){
        Bridge.maximumLengthMs = ms;
    }
    public static void StartRecording() {
        MainActivity activity = (MainActivity)UnityPlayer.currentActivity;
        activity.OnStartRecording();
    }
    public static void StopRecording() {
        MainActivity activity = (MainActivity)UnityPlayer.currentActivity;
        activity.OnStopRecording();
    }
    // Text To Speech
    public static void OpenTextToSpeed(String text) {
        MainActivity activity = (MainActivity)UnityPlayer.currentActivity;
        activity.OnStartSpeak(text);
    }
    public static void SettingTextToSpeed(String language, float pitch, float rate) {
        MainActivity activity = (MainActivity)UnityPlayer.currentActivity;
        activity.OnSettingSpeak(language, pitch, rate);
    }
    public static void StopTextToSpeed(){
        MainActivity activity = (MainActivity)UnityPlayer.currentActivity;
        activity.OnStopSpeak();
    }
    public static int getMessage() {
        return 100;
    }
}
