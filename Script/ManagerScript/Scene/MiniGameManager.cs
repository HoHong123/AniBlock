using Manager.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameManager : MonoBehaviour
{
    private const string MINIGAME_KEY = "MINIGAMEKEYCODE";
    private const string VICTORY_KEY = "C#$^@CCA";
    private int i_SuccessNumber = -1;
    protected int Get_RescuedCount()
    {
        if(i_SuccessNumber < 0)
        {
            ResetMinigameList();
        }
        return i_SuccessNumber;
    }

    private bool[] MINIGAME_SUCCESS_LIST;
    protected int Get_MinigameSuccessLength()
    {
        if(MINIGAME_SUCCESS_LIST != null)
        {
            return MINIGAME_SUCCESS_LIST.Length;
        }

        Debug.LogError("There is no list");
        return 0;
    }
    protected bool Get_IsSuccess(int position)
    {
        return MINIGAME_SUCCESS_LIST[position];
    }

    private string Get_MinigameKey(string name)
    {
        return Encrypto.Encrypt(MINIGAME_KEY + name);
    }
    private string Get_VictoryKey()
    {
        return Encrypto.Encrypt(VICTORY_KEY);
    }

    protected Manager.Util.XmlManager _xmlManager = null;
    protected Manager.Util.PackageManager _PackageManager = null;

    [Header("====게임 패널====")]
    [SerializeField] private GameObject PAN_Result  = null;
    [SerializeField] private GameObject PAN_Victory = null;
    [SerializeField] private GameObject PAN_Fail    = null;

    [Space(5)]
    [SerializeField] private Text TXT_TimeResult    = null;
    [SerializeField] private Text TXT_AnswerResult  = null;
    [SerializeField] private Text TXT_MistakeResult = null;
    [SerializeField] private Text TXT_ScoreResult   = null;
    [SerializeField] protected Font FONT_Comic      = null;

    protected void SetVictory(int _score, int _time, int _fail)
    {
        Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.MINIGAME_CLIP_LIST.M_CLEAR_Success);

        PAN_Result.SetActive(true);
        PAN_Victory.SetActive(true);

        TXT_MistakeResult.text = _fail.ToString();
        TXT_AnswerResult.text = _score.ToString();
        TXT_TimeResult.text = _time.ToString();
        TXT_ScoreResult.text = (_time + (_score * 10) - (_fail * 10)).ToString();

        string name = "";
        switch (PackageManager.Instance.CURRENT_PACKAGE_INFO)
        {
            case PackageManager.PACKAGE_INFO.FIRST:
                name = _PackageManager.FC_PACKAGE.ToString();
                break;
            case PackageManager.PACKAGE_INFO.RENEWEL:
                name = _PackageManager.RE_PACKAGE.ToString();
                break;
            case PackageManager.PACKAGE_INFO.PPORORO:
                name = _PackageManager.PR_PACKAGE.ToString();
                break;
        }

        PlayerPrefs.SetString(Get_MinigameKey(name), Get_VictoryKey());
    }
    protected void SetFail(int _score, int _time, int _fail)
    {
        Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.MINIGAME_CLIP_LIST.M_CLEAR_Fail);

        PAN_Result.SetActive(true);
        PAN_Fail.SetActive(true);

        TXT_MistakeResult.text = _fail.ToString();
        TXT_AnswerResult.text = _score.ToString();
        TXT_TimeResult.text = _time.ToString();


    }

    private void Awake()
    {
        _xmlManager = Manager.Util.XmlManager.S;
        _PackageManager = Manager.Util.PackageManager.Instance;
    }


    protected void Init_MinigameManager()
    {
        switch (PackageManager.Instance.CURRENT_PACKAGE_INFO)
        {
            case PackageManager.PACKAGE_INFO.FIRST:
                MINIGAME_SUCCESS_LIST = new bool[(int)(Manager.Util.PackageManager.FirstCollection.NumOf) + 1];
                break;

            case PackageManager.PACKAGE_INFO.RENEWEL:
                MINIGAME_SUCCESS_LIST = new bool[(int)(Manager.Util.PackageManager.RenewelCollection.NumOf) + 1];
                break;

            case PackageManager.PACKAGE_INFO.PPORORO:
                MINIGAME_SUCCESS_LIST = new bool[(int)(Manager.Util.PackageManager.PpororoCollection.NumOf) + 1];
                break;

            default:
                Debug.LogError("MiniGameManager(Init_MinigameManager) :: Package Info not found");
                break;
        }

        ResetMinigameList();
    }

    protected void ResetMinigameList()
    {
        for (int i = 0; i < MINIGAME_SUCCESS_LIST.Length-1; i++)
        {
            string name = "";
            switch (PackageManager.Instance.CURRENT_PACKAGE_INFO)
            {
                case PackageManager.PACKAGE_INFO.FIRST:
                    name = (Manager.Util.PackageManager.FirstCollection.WB_Cup + i).ToString();
                    break;

                case PackageManager.PACKAGE_INFO.RENEWEL:
                    name = (Manager.Util.PackageManager.RenewelCollection.RE1_Larva + i).ToString();
                    break;

                case PackageManager.PACKAGE_INFO.PPORORO:
                    name = (Manager.Util.PackageManager.PpororoCollection.PR1_CarrotA + i).ToString();
                    break;

                default:
                    Debug.LogError("MiniGameManager(ResetMinigameList) :: Package Info not found");
                    break;
            }

            // 키가 없으면 false값 초기화로 새로 생성
            if (!PlayerPrefs.HasKey(Get_MinigameKey(name)))
            {
                PlayerPrefs.SetString(Get_MinigameKey(name), "CONTINUE");

                MINIGAME_SUCCESS_LIST[i] = false;
                continue;
            }

            // 키가 존재하면 미니게임 클리어 여부 확인
            if (PlayerPrefs.GetString(Get_MinigameKey(name)) == Get_VictoryKey())
            {
                i_SuccessNumber++;
                MINIGAME_SUCCESS_LIST[i] = true;
            }
            else
            {
                MINIGAME_SUCCESS_LIST[i] = false;
            }
            //Debug.Log(name + " : " + MINIGAME_SUCCESS_LIST[i]);.
        }
    }
}
