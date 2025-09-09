using UnityEngine;

// This is a self-contained, plugin-free class for native Text-to-Speech on Android.
public static class NativeTTS
{
#if UNITY_ANDROID
    private static AndroidJavaObject tts;
    private static bool isInitialized = false;

    // This class is a proxy to receive the callback when the TTS engine is ready.
    private class TtsInitListener : AndroidJavaProxy
    {
        public TtsInitListener() : base("android.speech.tts.TextToSpeech$OnInitListener") { }

        // This function is called from the Android side when initialization is complete.
        public void onInit(int status)
        {
            if (status == 0) // 0 means SUCCESS
            {
                isInitialized = true;
                Debug.Log("Native TTS Initialized successfully.");
                // You can optionally set the language here once it's ready.
                // SetLanguage("en-US"); 
            }
            else
            {
                Debug.LogError("Native TTS Initialization failed.");
            }
        }
    }
#endif

    public static void Initialize()
    {
#if UNITY_ANDROID
        if (isInitialized) return;

        // Get the current Android activity and context
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

        // Create an instance of the Android TextToSpeech class
        tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", context, new TtsInitListener());
#endif
    }

    public static void Speak(string text)
    {
#if UNITY_ANDROID
        if (!isInitialized || tts == null)
        {
            Debug.LogWarning("Native TTS is not ready. Cannot speak.");
            return;
        }
        // Call the 'speak' method on the Android TTS object.
        tts.Call<int>("speak", text, 0, null, null);
#endif
    }

    public static void Stop()
    {
#if UNITY_ANDROID
        if (!isInitialized || tts == null) return;
        tts.Call<int>("stop");
#endif
    }

    public static void SetLanguage(string langCode) // e.g., "en-US", "hi-IN"
    {
#if UNITY_ANDROID
        if (!isInitialized || tts == null)
        {
            Debug.LogWarning("Native TTS is not ready. Cannot set language.");
            return;
        }

        // Create a Java Locale object
        AndroidJavaObject locale = new AndroidJavaObject("java.util.Locale", langCode);
        // Set the language
        tts.Call<int>("setLanguage", locale);
        Debug.Log("Native TTS language set to: " + langCode);
#endif
    }
}