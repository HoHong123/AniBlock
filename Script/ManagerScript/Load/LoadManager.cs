using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//=========================================
//일자 : 2019 - 10 - 10
//성명 : 전 수용
//=========================================

public class LoadManager : MonoBehaviour
{
    [Header("BACKGROUND MOVE")]
    public Material MAT_Background = null;
    public Vector2 SavedOffset = Vector2.zero;
    public GameObject OBJ_Privacy = null;

    [Space(10)]

    public GameObject Warning;
    public GameObject ENG_Privacy;
    public GameObject KOR_Privacy;

    [Space(10)]
    public Image _slider = null;

    private bool    b_isOpening     = false;
    private bool    _isDone         = false;
    private bool    _isWarning      = false;
    private float   _fTime          = 0f;
    private AsyncOperation  _async_operation;

    public Button _SceneMoveButton = null;


    //# 시스템 언어에 따라, 기본 언어를 설정합니다.
    private void Awake()
    {
        SystemLanguage sl = Application.systemLanguage;

        switch (sl)
        {
            case SystemLanguage.Korean:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_KOR);
                break;

            case SystemLanguage.ChineseSimplified:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_CHS);
                break;

            case SystemLanguage.ChineseTraditional:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_CHT);
                break;

            case SystemLanguage.Japanese:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_JPN);
                break;

            case SystemLanguage.Spanish:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_ESP);
                break;

            case SystemLanguage.English:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_USA);
                break;

            case SystemLanguage.Russian:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_RUS);
                break;

            case SystemLanguage.German:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_DEU);
                break;

            case SystemLanguage.French:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_FRA);
                break;

            case SystemLanguage.Dutch:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_NLD);
                break;

            case SystemLanguage.Polish:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_POL);
                break;

            case SystemLanguage.Italian:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_ITA);
                break;

            default:
                I2.Loc.LocalizationManager.CurrentLanguage = PlayerPrefs.GetString(Manager.Util.OptionManager.USER_LANG_KEY, Manager.Util.OptionManager.USER_LANG_USA);
                break;
        }

        //Debug.Log("Current Language : " + I2.Loc.LocalizationManager.CurrentLanguage);

        PlayerPrefs.SetString(Manager.Util.OptionManager.USER_LANG_KEY, I2.Loc.LocalizationManager.CurrentLanguage);

        if (!PlayerPrefs.HasKey("FirstOpen"))
        {
            PlayerPrefs.SetInt("FirstOpen", 0);
            //I2.Loc.LocalizationManager.CurrentLanguage = Manager.Util.OptionManager.USER_LANG_USA;

            b_isOpening = true;
            _isWarning = true;

            if (Application.systemLanguage == SystemLanguage.Korean)
            {
                KOR_Privacy.SetActive(true);
            }
            else
            {
                ENG_Privacy.SetActive(true);
            }
        }
    }

    //# 다음 씬을 비동기로드합니다. 
    private void Update()
    {
        _fTime += Time.deltaTime / 2.0f;
        _slider.fillAmount = _fTime;

        if (_fTime >= 1)
        {
            //슬라이더 비활성화, 버튼 등장.!
            _slider.transform.parent.gameObject.SetActive(false);
            _SceneMoveButton.gameObject.SetActive(true);

            if (_isWarning)
            {
                _isWarning = false;
                Warning.SetActive(true);
            }
        }

        SavedOffset.x += 0.1f * Time.deltaTime;
        Vector2 offset = new Vector2(-SavedOffset.x, SavedOffset.x);
        MAT_Background.mainTextureOffset = offset;
    }

    // 다음씬으로 넘어갑니다.
    public void StartLoad()
    {
            //b_isOpening = true; // 무조건 오프닝씬 가기

        CustomARCameraManager.Instance.DisableCamera();

        string next_Scene = "";
        if (b_isOpening)
        {
            next_Scene = "Scene_Opening";
        }
        else
        {
            if (Manager.Util.PackageManager.Instance.CURRENT_THEME_INFO == Manager.Util.PackageManager.PACKAGE_INFO.PPORORO)
            {
                next_Scene = "Scene_Title_PRR";
                Manager.Sound.SoundManager.S.Play_Music(Manager.Sound.SoundManager.BGM_CLIP_LIST.PPR_TITLE);
            }
            else
            {
                next_Scene = "Scene_Title";
                Manager.Sound.SoundManager.S.Play_Music(Manager.Sound.SoundManager.BGM_CLIP_LIST.TITLE);
            }
        }

        LoadMaster.S.LoadSceneFunc(next_Scene);
    }
}