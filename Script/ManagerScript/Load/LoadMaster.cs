using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadMaster : MonoBehaviour
{
    public static LoadMaster S = null;

    public string s_MoveSceneName = "";
    public string s_PrevSceneName = "";


    [SerializeField] private GameObject CAN_BusyIndicator = null;
    public void SetLoadPanel(bool isActive)
    {
        CAN_BusyIndicator.SetActive(isActive);
    }
    public bool GetLoadPanelActive()
    {
        return CAN_BusyIndicator.active;
    }

    public bool ResetData = false;

    private void Awake()
    {
        S = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(CAN_BusyIndicator);

        s_PrevSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (ResetData) PlayerPrefs.DeleteAll();
    }

    public void LoadSceneFunc(string _Scene)
    {
        s_MoveSceneName = _Scene;

        DeterminePackage();

        SetLoadPanel(true);
        StartCoroutine(Load());
    }

    private void DeterminePackage()
    {
        if (Manager.Util.PackageManager.Instance.CURRENT_THEME_INFO != Manager.Util.PackageManager.PACKAGE_INFO.PPORORO) return;

        switch (s_MoveSceneName)
        {
            case "Scene_Title":
            case "Scene_Guide":
            case "Scene_World":
            case "Scene_Rescue":
                s_MoveSceneName += "_PRR";
                break;
            case "Scene_World_First":
                s_MoveSceneName = "Scene_World_PRR";
                break;
        }
    }

    private IEnumerator Load()
    {
        yield return null;

        s_PrevSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;


        AsyncOperation oper;
        oper = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(s_MoveSceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        oper = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(s_MoveSceneName);
        oper.allowSceneActivation = false;

        bool b_SetManualLoadPanel = false;

        // 타이틀과 오프닝은 LoadManager에서 수행함
        //Debug.Log("SCENE LOAD : " + s_MoveSceneName);
        switch (s_MoveSceneName)
        {
            case "Scene_Title":
            case "Scene_Title_PRR":
            case "Scene_Opening":
                b_SetManualLoadPanel = true;
                CustomARCameraManager.Instance.DisableCamera();
                break;

            case "Scene_Main":
                Manager.Sound.SoundManager.S.Play_Music(Manager.Sound.SoundManager.BGM_CLIP_LIST.MAIN);
                CustomARCameraManager.Instance.DisableCamera();
                break;

            case "Scene_Recognize":
                CustomARCameraManager.Instance.Switch_Mode(CustomARCameraManager.CUSTOM_AR_LIST.IMAGE);
                CustomARCameraManager.Instance.EnableCamera();
                break;

            case "Scene_Rescue":
            case "Scene_Rescue_PRR":
                if (s_PrevSceneName != "Scene_Recognize" && s_PrevSceneName != "Scene_Guide") // 내가 이걸 왜했더라
                    Manager.Sound.SoundManager.S.Play_Music(Manager.Sound.SoundManager.BGM_CLIP_LIST.PUZZLE);
                CustomARCameraManager.Instance.EnableCamera();
                break;

            case "Scene_Guide":
            case "Scene_Guide_PRR":
                CustomARCameraManager.Instance.Switch_Mode(CustomARCameraManager.CUSTOM_AR_LIST.IMAGE);
                CustomARCameraManager.Instance.DisableCamera();
                break;

            case "Scene_World_First":
            case "Scene_World_Renewal":
            case "Scene_World_PRR":
                Manager.Sound.SoundManager.S.Play_Music(Manager.Sound.SoundManager.BGM_CLIP_LIST.WORLD);
                CustomARCameraManager.Instance.DisableCamera();
                break;

            case "Scene_MiniGame":
            case "Scene_Minigame_PRR":
                Manager.Sound.SoundManager.S.Play_Music(Manager.Sound.SoundManager.BGM_CLIP_LIST.LIVE_GAME);

                CustomARCameraManager.Instance.EnableCamera();
                break;

            default: // 컷씬 이동
                CustomARCameraManager.Instance.Switch_Mode(CustomARCameraManager.CUSTOM_AR_LIST.INSTANT);
                CustomARCameraManager.Instance.EnableCamera();
                break;
        }

        while (!oper.isDone)
        {
            //Debug.Log(oper.progress);
            if (oper.progress >= 0.9f)
            {
                oper.allowSceneActivation = true;
            }

            yield return null;
        }

        if (b_SetManualLoadPanel && GetLoadPanelActive()) SetLoadPanel(false);
    }
}
