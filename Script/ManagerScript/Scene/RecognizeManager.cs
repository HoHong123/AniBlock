using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using Manager.Util;
using UnityEngine.SceneManagement;

public class RecognizeManager : MonoBehaviour, RecognizeMarker
{
    private Manager.Util.XmlManager _xmlManager = null;
    private Manager.Util.PackageManager _PackageManager = null;

    public static RecognizeManager S = null;
    [Header("==== 패키지 ====")]
    [Tooltip("주의 :: PACKAGE 순서대로 입력")]
    [SerializeField] private Sprite[] SPRITE_PackagePNG;    // 마커 리스트
    [SerializeField] private Button[] BTN_Pacakages = null; // 


    [Header("==== 오브젝트 ====")]
    [SerializeField] private Image IMG_Zoom = null;         // 중앙 마커 인식용 원형 이미지
    [SerializeField] private Image IMG_Marker = null;       // 중앙 원형 마커 이미지
    [SerializeField] private Image IMG_Package = null;      // 중앙 패키지 이미지
    [SerializeField] private Button BTN_Help = null;        // Help메뉴 버튼
    [SerializeField] private GameObject IMG_Check = null;   // 마커 인식 확인용 체크 이미지


    private bool b_ActOnce = false;
    private bool b_Transparent = true;                      // 마커 이미지 루프 설정
    private Color trim = new Color(0, 0, 0, 0.00333f);      // 마커 이미지 알파값 색상 변경 범위


    private void Awake()
    {
        S = this;
    }

    private void Start()
    {
        _xmlManager = Manager.Util.XmlManager.S;
        _PackageManager = PackageManager.Instance;

        CustomARCameraManager.Instance.InitializeTracker();

        // 패키지 변경
        Init_Package();
        // 마커 이미지 확인
        Set_MarkerSprite();


        b_ActOnce = false;

        if (LoadMaster.S.GetLoadPanelActive()) LoadMaster.S.SetLoadPanel(false);
    }

    private void Init_Package()
    {
        switch (_PackageManager.CURRENT_PACKAGE_INFO) 
        {
            case PackageManager.PACKAGE_INFO.FIRST:
                IMG_Package.sprite = SPRITE_PackagePNG[0];
                break;
            case PackageManager.PACKAGE_INFO.RENEWEL:
                IMG_Package.sprite = SPRITE_PackagePNG[1];
                break;
            case PackageManager.PACKAGE_INFO.PPORORO:
                IMG_Package.sprite = SPRITE_PackagePNG[2];
                break;
        }

            // 퍼스트 패키지 버튼 초기화
        BTN_Pacakages[0].onClick.AddListener(() => {

            if ((int)PackageManager.Instance.CURRENT_PACKAGE_INFO == 0) return;

            _PackageManager.Set_ChangePackage(PackageManager.Instance.Get_PackageKey(PackageManager.PACKAGE_INFO.FIRST));
            StartCoroutine(IE_TrackerInitializeDelay());
            IMG_Package.sprite = SPRITE_PackagePNG[0];
            Set_MarkerSprite();
        });

        // 패키지별 버튼 초기화
        for(int k = 1; k < (int)PackageManager.PACKAGE_INFO.NumOf; k++)
        {
                // 리뉴얼 패키지 정보가 true이면 버튼 활성화
            if (_PackageManager.Get_IsPackageActive((PackageManager.PACKAGE_INFO)k))
            {
                int num = k;
                BTN_Pacakages[k].onClick.AddListener(() => {

                    if ((int)PackageManager.Instance.CURRENT_PACKAGE_INFO == num) return;

                    _PackageManager.Set_ChangePackage(PackageManager.Instance.Get_PackageKey((PackageManager.PACKAGE_INFO)num));
                    StartCoroutine(IE_TrackerInitializeDelay());
                    IMG_Package.sprite = SPRITE_PackagePNG[num];
                    Set_MarkerSprite();
                });
            }
            // 리뉴얼 패키지 정보가 false이면 버튼 비활성화 및 락다운
            else
            {
                // 버튼 비활성화
                BTN_Pacakages[1].interactable = false;
                // 락 이미지 활성화
                BTN_Pacakages[1].transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
      /// 트래커 이니셜 함수를 딜레이하는 함수
      /// 실시간으로 트래커를 생성시키면 이전 트래커를 제거해도 같은 프레임에 오브젝트를 검색하여 다시 이니셜하기 때문에 프레임 딜레이를 주어야한다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IE_TrackerInitializeDelay()
    {
        yield return new WaitForEndOfFrame();

        CustomARCameraManager.Instance.InitializeTracker();
    }

    private void Set_MarkerSprite()
    {
        switch (_PackageManager.CURRENT_PACKAGE_INFO)
        {
            case PackageManager.PACKAGE_INFO.FIRST:
                AsyncAssetLoadManager.Instance.Get_LoadAssetSprite(_PackageManager.s_CircleMarkerAddress[UnityEngine.Random.Range(0, (int)PackageManager.FirstCollection.NumOf)], ref IMG_Marker);
                break;

            case PackageManager.PACKAGE_INFO.RENEWEL:
                AsyncAssetLoadManager.Instance.Get_LoadAssetSprite(_PackageManager.s_CircleMarkerAddress[UnityEngine.Random.Range(0, (int)PackageManager.RenewelCollection.NumOf)], ref IMG_Marker);
                break;

            case PackageManager.PACKAGE_INFO.PPORORO:
                AsyncAssetLoadManager.Instance.Get_LoadAssetSprite(_PackageManager.s_CircleMarkerAddress[UnityEngine.Random.Range(0, (int)PackageManager.PpororoCollection.NumOf)], ref IMG_Marker);
                break;

            default:
                Debug.LogError("RecognizeManager (MarkerSprite_Initialize) :: NO PACKAGE INFO");
                return;
        }
    }

    private void Update()
    {
        MarkerTransparentLoop();
    }

    /// <summary>
    /// 마커 이미지 투명화/불투명화 함수
    /// </summary>
    private void MarkerTransparentLoop()
    {
        // 마커 이미지 투명화 루프
        if (b_Transparent)
        {
            // 색상 서서히 투명화
            IMG_Marker.color -= trim;

            // 색상이 완전 투명이 되면 다른 이미지로 변경 및 target = false
            if (IMG_Marker.color.a < 0)
            {
                IMG_Marker.color = new Color(1, 1, 1, 0);
                b_Transparent = false;
                Set_MarkerSprite();
            }
        }
        // 마커 이미지 불투명화 루프
        else // target = 0.5f
        {
            // 색상을 서서히 불투명화
            IMG_Marker.color += trim;

            // 특정 알파값 색상 이상으로 가면 투명화 하도록 설정
            if (IMG_Marker.color.a > 0.4f)
            {
                IMG_Marker.color = new Color(1, 1, 1, 0.4f);
                b_Transparent = true;
            }
        }
    }

    /// <summary>
    /// 도안 인식 후 도안 이미지를 표현하고 대기시간을 가졌다 다음 씬으로 넘어가게 해주는 함수
    /// </summary>
    /// <param name="_moveScene">다음씬 명칭</param>
    /// <returns></returns>
    private IEnumerator RecognizingSuccess()
    {
        enabled = false;
        IMG_Marker.color = new Color(1, 1, 1, 0);

        // 1초동안 이미지 불투명화
        float time = 1.0f;
        while (time > 0)
        {
            IMG_Marker.color += new Color(0, 0, 0, 0.1f);

            time -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.UI_CLIP_LIST.P_RESCUE_ScanSuccess);
        IMG_Check.SetActive(true);

        // 1.5초 대기
        time = 1.5f;
        while (time > 0)
        {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
        }

        // 다음 씬 이동
        LoadMaster.S.LoadSceneFunc("Scene_Rescue");
    }

    /// <summary>
    /// 마커를 인식하면 동작하는 함수
    /// </summary>
    /// <param name="_data">인식한 도안 명칭</param>
    /// <param name="_moveScene">인식 후 이동할 다음 씬 정보</param>
    public void MarkerRecognize(string _data)
    {
        // 이미 작동하고 있으면 반환
        if (b_ActOnce) return;

        // 한번만 동작하도록 하는 bool문 true
        b_ActOnce = true;

        Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.UI_CLIP_LIST.P_RECOG_MarkerScan); // 사운드 실행
        BTN_Help.interactable = false; // 도움말 버튼 비활성화


        #region 도안 크기 확인
        // 받은 데이터 분활 (WB_Cup_0 = WB, Cup, 0)
        // 인식된 데이터는 구출씬에서 두 단위로(WB_Cup) 나온다.
        string[] splitData = _data.Split('_');

        // 도안 명칭 확인
        string name = splitData[0] + "_" + splitData[1];

        // 8x8이 아닌 도안이 인식되면 해당 정보를 XmlManager에 전달
        if (splitData.Length > 2)
        {
            splitData[2] = "_" + splitData[2];

            // 8x16부터 탐색
            for (int i = 1; i < (int)PackageManager.MARKER_SIZE.NumOf; i++)
            {
                if (splitData[2].Equals((PackageManager.MARKER_SIZE._8x8 + i).ToString()))
                {
                    _PackageManager.CURRENT_MARKER_SIZE = (PackageManager.MARKER_SIZE._8x8 + i);
                    break;
                }
            }
        }
        // 8x8도안이면 초기화
        else
        {
            _PackageManager.CURRENT_MARKER_SIZE = PackageManager.MARKER_SIZE._8x8;
        }
        #endregion

        #region 도안 이미지 설정
        // 인식된 도안에 대해 각 패키지별 이미지 및 마커 이미지 설정
        switch (_PackageManager.CURRENT_PACKAGE_INFO)
        {
            case PackageManager.PACKAGE_INFO.FIRST:
                for (int i = 0; i < (int)Manager.Util.PackageManager.FirstCollection.NumOf; i++)
                {
                    if (name.Equals((Manager.Util.PackageManager.FirstCollection.WB_Cup + i).ToString()))
                    {
                        AsyncAssetLoadManager.Instance.Get_LoadAssetSprite(_PackageManager.s_CircleMarkerAddress[(int)(PackageManager.FirstCollection.WB_Cup + i)], ref IMG_Marker);

                        AsyncAssetLoadManager.Instance.Set_LoadAssetMarkerSprites(
                            _PackageManager.s_PatternMarkerAddress[(int)(PackageManager.FirstCollection.WB_Cup + i)],
                            _PackageManager.s_CircleMarkerAddress[(int)(PackageManager.FirstCollection.WB_Cup + i)],
                            _PackageManager.s_3DMarkerAddress[(int)(PackageManager.FirstCollection.WB_Cup + i)]);

                        _PackageManager.CurrentImage = (int)(Manager.Util.PackageManager.FirstCollection.WB_Cup + i);
                        break;
                    }
                }

                _PackageManager.FC_PACKAGE = (Manager.Util.PackageManager.FirstCollection.WB_Cup + _PackageManager.CurrentImage);
                break;

            case PackageManager.PACKAGE_INFO.RENEWEL:
                for (int i = 0; i < (int)Manager.Util.PackageManager.RenewelCollection.NumOf; i++)
                {
                    if (name.Equals((Manager.Util.PackageManager.RenewelCollection.RE1_Larva + i).ToString()))
                    {
                        AsyncAssetLoadManager.Instance.Get_LoadAssetSprite(_PackageManager.s_CircleMarkerAddress[(int)(PackageManager.RenewelCollection.RE1_Larva + i)], ref IMG_Marker);

                        AsyncAssetLoadManager.Instance.Set_LoadAssetMarkerSprites(
                            _PackageManager.s_PatternMarkerAddress[(int)(PackageManager.RenewelCollection.RE1_Larva + i)],
                            _PackageManager.s_CircleMarkerAddress[(int)(PackageManager.RenewelCollection.RE1_Larva + i)],
                            _PackageManager.s_3DMarkerAddress[(int)(PackageManager.RenewelCollection.RE1_Larva + i)]);

                        _PackageManager.CurrentImage = (int)(Manager.Util.PackageManager.RenewelCollection.RE1_Larva + i);
                        break;
                    }
                }

                _PackageManager.RE_PACKAGE = (Manager.Util.PackageManager.RenewelCollection.RE1_Larva + _PackageManager.CurrentImage);
                break;

            case PackageManager.PACKAGE_INFO.PPORORO:
                for (int i = 0; i < (int)Manager.Util.PackageManager.PpororoCollection.NumOf; i++)
                {
                    if (name.Equals((Manager.Util.PackageManager.PpororoCollection.PR1_CarrotA + i).ToString()))
                    {
                        AsyncAssetLoadManager.Instance.Get_LoadAssetSprite(_PackageManager.s_CircleMarkerAddress[(int)(PackageManager.PpororoCollection.PR1_CarrotA + i)], ref IMG_Marker);

                        AsyncAssetLoadManager.Instance.Set_LoadAssetMarkerSprites(
                            _PackageManager.s_PatternMarkerAddress[(int)(PackageManager.PpororoCollection.PR1_CarrotA + i)],
                            _PackageManager.s_CircleMarkerAddress[(int)(PackageManager.PpororoCollection.PR1_CarrotA + i)],
                            _PackageManager.s_3DMarkerAddress[(int)(PackageManager.PpororoCollection.PR1_CarrotA + i)]);

                        _PackageManager.CurrentImage = (int)(Manager.Util.PackageManager.PpororoCollection.PR1_CarrotA + i);
                        break;
                    }
                }

                _PackageManager.PR_PACKAGE = (Manager.Util.PackageManager.PpororoCollection.PR1_CarrotA + _PackageManager.CurrentImage);

                break;

            default:
                Debug.LogError("No Package Info is set :: " + _PackageManager.CURRENT_PACKAGE_INFO);
                return;
        }
        #endregion

        _xmlManager.LoadMapData(name);
        _xmlManager.LoadToStageXML(name);

        StartCoroutine(RecognizingSuccess());
    }


    // ============================= 버튼 리스터 함수 ============================= //

    /// <summary>
    /// 메인메뉴 복귀 함수
    /// </summary>
    public void BTN_BackToMain()
    {
        _PackageManager.Set_TrackerSceneLoad(false);
        LoadMaster.S.LoadSceneFunc("Scene_Main");
    }

    public void BTN_PackageSelectTrackerDeactive(bool _active)
    {
        CustomARCameraManager.Instance.PauseTracker(_active);
    }
}
