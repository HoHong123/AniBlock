using Manager.Sound;
using Manager.Util;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    public class MainManager : MonoBehaviour
    {
        private const float f_DEFAULT_WAIT = 1.0f;

        [Header("====== 테마 ======")]
        [SerializeField] private GameObject[] OBJ_Theme = new GameObject[2];
        [SerializeField] private GameObject[] OBJ_IMGCheckBox = new GameObject[4];
        [SerializeField, Tooltip("주의 : 패키지 매니저의 PACKAGE ENUM 순서대로 놓을 것")]
        private Button[] BTN_Package;
        [SerializeField] private GameObject[] OBJ_PackageLock;


        [Header("====== 퍼펫 ======")]
        [SerializeField] private GameObject[] OBJ_Puppet = new GameObject[5];

        private int i_ActivePuppet = 0;


        [Header("====== 리포터 ======")]
        [SerializeField] private GameObject OBJ_Reporter = null;
        private Reporter SCRIPT_Reporter = null;
        private int i_ReporterActive = 0;
        private float f_Wait = 0;


        [Space(10), Header("====== Frames ======")]
        [SerializeField] private Text[] TXT_Frames = new Text[3];
        [SerializeField] private Material[] Mat_Frames = new Material[3];

        private float f_Height;

        
        [Header("====== 코드 ======")]
        [SerializeField] private InputField[] IF_CodeFields = new InputField[3];
        [SerializeField] private GameObject OBJ_TXTCodeSuccess = null;
        [SerializeField] private GameObject OBJ_TXTCodeFail = null;



        //------------------------- Initialize -------------------------
        public void Awake()
        {
            f_Height = (Screen.height * (1100f / 2560f)); // (필요 높이 / 원본 높이) * 100 = 항상 필요한 높이 퍼센트 : 1440 x 2560 기준
            if (f_Height * 1.5f > Screen.width)
            {
                f_Height = Screen.width * 0.33f * 2;
            }
        }

        void Start()
        {
            // 패키지 버튼 초기화
            for(int k = 0; k < BTN_Package.Length; k++)
            {
                int num = k;
                BTN_Package[k].onClick.AddListener(() => {

                    if ((int)PackageManager.Instance.CURRENT_THEME_INFO == num) return;

                    SoundManager.S.Play_ButtonSound();
                    CustomARCameraManager.Instance.DisableTrackers();
                    PackageManager.Instance.Set_ChangeTheme(PackageManager.Instance.Get_PackageKey((PackageManager.PACKAGE_INFO)num));
                    PackageManager.Instance.Set_ChangePackage(PackageManager.Instance.Get_PackageKey((PackageManager.PACKAGE_INFO)num));
                    Set_ChangeTheme();

                });
            }

            // 촬영본 활성화
            StartCoroutine(InitFrame());

            // 테마 선택
            Set_ChangeTheme();
            // 테마 버튼 잠금 확인
            Set_DetermineLockedPackage();

            if (LoadMaster.S.GetLoadPanelActive()) LoadMaster.S.SetLoadPanel(false);
        }

        //------------------------- Update -------------------------
        private void Update()
        {
            if (f_Wait > 0)
            {
                f_Wait -= Time.deltaTime;
            }
        }

        //------------------------- Function -------------------------
        private IEnumerator InitFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                if (SceneManager.GetActiveScene().name == "Scene_Main")
                {
                    int k = PlayerPrefs.GetInt("SAVE_TEXTURE");

                    for (int i = 0; i < 3; i++)
                    {
                        int next = i + k;
                        if (next > 2) next -= 3;

                        byte[] savedata;

                        try
                        {
                            savedata = File.ReadAllBytes(PATH.GetOnResources(Screen.width + "x" + Screen.height + "_1x1_" + next + ".jpg"));
                            Texture2D tex = new Texture2D((int)f_Height, (int)f_Height);
                            tex.LoadImage(savedata);
                            tex.Apply();

                            Mat_Frames[i].mainTexture = tex;
                            TXT_Frames[i].text = PlayerPrefs.GetInt("SAVE_TEXTURE_RATE" + next) + "%";
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("MainManager(InitFrame) :: There is no texture");
                            break;
                        }
                    }

                    break;
                }
            }
        }

        private void Set_Puppet()
        {
            // 기존 퍼펫 제거
            OBJ_Puppet[i_ActivePuppet].SetActive(false);

            // 새 퍼펫 결정
            i_ActivePuppet = Random.Range(0, 5);

            // 새 퍼펫 활서오하
            OBJ_Puppet[i_ActivePuppet].SetActive(true);
        }

        private void Set_ChangeTheme()
        {
            int theme = 0;
            int check = 0;
            switch (PackageManager.Instance.CURRENT_THEME_INFO)
            {
                case PackageManager.PACKAGE_INFO.FIRST:
                    Set_Puppet();
                    theme = 0;
                    check = 0;
                    break;

                case PackageManager.PACKAGE_INFO.RENEWEL:
                    Set_Puppet();
                    theme = 0;
                    check = 1;
                    break;

                case PackageManager.PACKAGE_INFO.PPORORO:
                    theme = 1;
                    check = 2;
                    break;

                default:
                    Debug.LogError("MainManager(ChangeTheme) :: There is no theme info");
                    return;
            }

            // 테마 설정
            for (int k = 0; k < OBJ_Theme.Length; k++)
            {
                if (theme == k)
                {
                    OBJ_Theme[k].SetActive(true);
                    continue;
                }
                OBJ_Theme[k].SetActive(false);
            }

            // 체크박스 제거
            for (int k = 0; k < OBJ_IMGCheckBox.Length; k++)
            {
                if (check == k) 
                { 
                    OBJ_IMGCheckBox[k].SetActive(true);
                    continue;
                }
                OBJ_IMGCheckBox[k].SetActive(false);
            }
        }

        private void Set_DetermineLockedPackage()
        {
            for(int  k = 0; k < (int)PackageManager.PACKAGE_INFO.PPORORO; k++)
            {
                OBJ_PackageLock[k].SetActive(!(PackageManager.Instance.Get_IsPackageActive((PackageManager.PACKAGE_INFO)(k + 1))));
            }
            
            // 현재 리뉴얼이 포함되지 않아 무조건 실행하는 라인, 나중에 리뉴얼 패키지 등록시 지울 것
            OBJ_PackageLock[0].SetActive(false);
        }

        public void Cheat()
        {
            if (i_ReporterActive > 13)
            {
                if (OBJ_Reporter.active)
                {
                    OBJ_Reporter.SetActive(false);
                }
                else
                {
                    OBJ_Reporter.SetActive(true);
                }

                i_ReporterActive = 0;
                return;
            }

            i_ReporterActive++;
            f_Wait = f_DEFAULT_WAIT;
        }

        public void PackageCodeActive()
        {
            OBJ_TXTCodeFail.SetActive(false);
            OBJ_TXTCodeSuccess.SetActive(false);

            string codes = "";
            for (int i = 0; i < 3; ++i)
            {
                codes += IF_CodeFields[i].text;
                IF_CodeFields[i].text = "";
            }

            if (PackageManager.Instance.Set_ComparePackageCode(codes))
            {
                OBJ_TXTCodeSuccess.SetActive(true);
                Set_DetermineLockedPackage();
            }
            else
            {
                OBJ_TXTCodeFail.SetActive(true);
            }
        }
    }
}
