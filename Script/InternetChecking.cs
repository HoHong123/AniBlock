using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class InternetChecking : MonoBehaviour
{
    int ClickCount;
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _ShowAlert(string message);
#elif UNITY_ANDROID
    private AndroidJavaObject UnityAvtivity;
    private AndroidJavaObject UnityInstance;
#endif

    bool isPaused = false;

    private void Awake()
    {
        //전체 한번만
        if (!PlayerPrefs.HasKey("FirstMenu"))
        {
            PlayerPrefs.SetInt("FirstMenu", 0);
#if UNITY_ANDROID
            AndroidJavaClass ajc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            UnityAvtivity = ajc.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaClass ajc2 = new AndroidJavaClass("com.example.unityplugin.Uplugin");
            UnityInstance = ajc2.CallStatic<AndroidJavaObject>("instance");

            UnityInstance.Call("setContext", UnityAvtivity);
#endif
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
#if UNITY_ANDROID
                if (Application.systemLanguage == SystemLanguage.Korean)
                {

                    UnityAvtivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        UnityInstance.Call("ShowToast", "인터넷 연결 장애", 0);
                    }));
                }
                else if (Application.systemLanguage == SystemLanguage.English)
                {
                    UnityAvtivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        UnityInstance.Call("ShowToast", "Internet connection failure", 0);
                    }));
                }
#elif UNITY_IOS
            if (Application.systemLanguage == SystemLanguage.Korean)
            {
                _ShowAlert("인터넷 연결 장애");
            }
            else if(Application.systemLanguage == SystemLanguage.English)
            {
                _ShowAlert("Internet connection failure");
            }
#endif
            }
        }        
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("FirstMenu");
    }   

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            isPaused = true;
            PlayerPrefs.DeleteKey("FirstMenu");
        }
        else {
            if (isPaused)
            {
                isPaused = false;
                PlayerPrefs.DeleteKey("FirstMenu");
            }
        }
    }
}
