using maxstAR;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomARCameraManager : MonoBehaviour
{
    public enum CUSTOM_AR_LIST
    {
        IMAGE = 0,
        INSTANT,
    }
    private static CustomARCameraManager s;
    public static CustomARCameraManager Instance 
    {
        private set { }
        get
        {
            if (s == null) s = new CustomARCameraManager();
            return s;
        }
    }

    [Header("====== Sample Scripts ======")]
    [SerializeField] ImageTrackerSample SCRIPT_ImageSample      = null;
    [SerializeField] InstantTrackerSample SCRIPT_InstantSample  = null;

    private bool b_CameraEnable = false;
    public bool IsCameraReady()
    {
        return b_CameraEnable;
    }

    [SerializeField] private Camera AR_Cam;
    private ARBehaviour ARBehaviour;



    private void Start()
    {
        s = this;


        // 카메라, 마커인식, 지면인식 오브젝트 DontDestroy 설정
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(SCRIPT_ImageSample.gameObject);
        DontDestroyOnLoad(SCRIPT_InstantSample.gameObject);


        // 카메라가 없으면 카메라 설정
        if(AR_Cam == null) AR_Cam = GetComponent<Camera>();


        ARBehaviour = SCRIPT_ImageSample;


        // For see through smart glass setting
        if (ConfigurationScriptableObject.GetInstance().WearableType == WearableCalibration.WearableType.OpticalSeeThrough)
        {
            WearableManager.GetInstance().GetDeviceController().SetStereoMode(true);

            CameraBackgroundBehaviour cameraBackground = FindObjectOfType<CameraBackgroundBehaviour>();
            cameraBackground.gameObject.SetActive(false);

            WearableManager.GetInstance().GetCalibration().CreateWearableEye(Camera.main.transform);

            // BT-300 screen is splited in half size, but R-7 screen is doubled.
            if (WearableManager.GetInstance().GetDeviceController().IsSideBySideType() == true)
            {
                // Do something here. For example resize gui to fit ratio
            }
        }
    }

    /// <summary>
    /// AR카메라를 지면인식 트래커으로 설정 변경
    /// </summary>
    public void Switch_Mode(CUSTOM_AR_LIST _LIST)
    {
        switch (_LIST)
        {
            case CUSTOM_AR_LIST.IMAGE:
                {
                    //Debug.Log("ImageTrackerSample Switch_Mode Activate");
                    // ImageTrackerSample 스크립트 호출시 업캐스팅 객체가 동일한 스크립트일때, 해당 스크립트 활성화
                    if (ARBehaviour == SCRIPT_ImageSample)
                    {
                        ((ImageTrackerSample)ARBehaviour).enabled = true;
                    }

                    // ImageTrackerSample 스크립트 호출시 업캐스팅 객체가 다른 스크립트일때
                    if (ARBehaviour == SCRIPT_InstantSample)
                    {
                        // 이전 객체 트래킹 제거 및 비동기화
                        ((InstantTrackerSample)ARBehaviour).StopTracker();
                        //((InstantTrackerSample)ARBehaviour).Destroy();
                        ((InstantTrackerSample)ARBehaviour).enabled = false;

                        // 목표 스크립트 삽입 및 활성화
                        ARBehaviour = SCRIPT_ImageSample;
                        ((ImageTrackerSample)ARBehaviour).enabled = true;
                    }

                    // 해당 스크립트 초기화 함수 호출
                    // 씬 이동시 다른 마커나 Behavior스크립트를 인식해야하는 경우,
                    // 다시 오브젝트를 탐색하고 해당 오브젝트 정보를 저장해야하기에 초기화 함수는 해당 함수 호출시 계속 이루어진다.
                    // * 구출씬과 가이드씬이 서로 다른 ImageTrackerBehavior 오브젝트가 존재하는 방식이었으나 딜레이를 줄이기 위해 인식한 오브젝트를 바로 가져오는 방식으로 바뀌어 의미가 줄어들었지만,
                    // * 앞으로 확장성을 생각하면 호출될때마다 초기화하는 것이 안전하다.
                    ((ImageTrackerSample)ARBehaviour).StartTracker();

                    break;
                }

            case CUSTOM_AR_LIST.INSTANT:
                {
                    //Debug.Log("InstantTrackerSample Switch_Mode Activate");
                    // InstantTrackerSample 스크립트 호출시 업캐스팅 객체가 동일한 스크립트일때, 해당 스크립트 활성화
                    if (ARBehaviour == SCRIPT_InstantSample)
                    {
                        ((InstantTrackerSample)ARBehaviour).enabled = true;
                    }

                    // InstantTrackerSample 스크립트 호출시 업캐스팅 객체가 다른 스크립트일때
                    if (ARBehaviour == SCRIPT_ImageSample)
                    {
                        // 이전 객체 트래킹 제거 및 비동기화
                        ((ImageTrackerSample)ARBehaviour).StopTracker();
                        //((ImageTrackerSample)ARBehaviour).Destroy();
                        ((ImageTrackerSample)ARBehaviour).enabled = false;

                        // 목표 스크립트 삽입 및 활성화
                        ARBehaviour = SCRIPT_InstantSample;
                        ((InstantTrackerSample)ARBehaviour).enabled = true;
                    }

                // 해당 스크립트 초기화 함수 호출
                // 씬 이동시 다른 마커나 Behavior스크립트를 인식해야하는 경우,
                // 다시 오브젝트를 탐색하고 해당 오브젝트 정보를 저장해야하기에 초기화 함수는 해당 함수 호출시 계속 이루어진다.
                // * 구출씬과 가이드씬이 서로 다른 InstantTrackerBehavior 오브젝트가 존재하는 방식이었으나 딜레이를 줄이기 위해 인식한 오브젝트를 바로 가져오는 방식으로 바뀌어 의미가 줄어들었지만,
                // * 앞으로 확장성을 생각하면 호출될때마다 초기화하는 것이 안전하다.
                ((InstantTrackerSample)ARBehaviour).StartTracker();

                    break;
                }
        }
    }

    // 카메라 on/off는 TrackerManager에서 카메라 머테리얼을 실행/정지가 가능하지만,
    // 해당 동작을 실행시 잠시나마 딜레이가 생겨서 카메라 컨포먼트를 On/Off하는 방식을 사용
    /// <summary>
    /// AR 카메라 실행
    /// </summary>
    public void EnableCamera()
    {
        //Debug.Log("Enable Camera");
        AR_Cam.enabled = true; // 카메라 활성화
        b_CameraEnable = true; // 카메라가 활성됬음을 알리는 bool값 설정
        if (ARBehaviour == SCRIPT_ImageSample) ((ImageTrackerSample)ARBehaviour).enabled = true;
        else if (ARBehaviour == SCRIPT_InstantSample) ((InstantTrackerSample)ARBehaviour).enabled = true;

        PauseTracker(false);
    }
    /// <summary>
    /// AR 카메라 정지
    /// </summary>
    public void DisableCamera()
    {
        //Debug.Log("Disable Camera");
        AR_Cam.enabled = false; // 카메라 정지
        b_CameraEnable = false; // 카메라가 정지됬음을 알리는 bool값 설정
        if (ARBehaviour == SCRIPT_ImageSample) ((ImageTrackerSample)ARBehaviour).enabled = false;
        else if (ARBehaviour == SCRIPT_InstantSample) ((InstantTrackerSample)ARBehaviour).enabled = false;

        PauseTracker(true);
    }

    /// <summary>
    /// 카메라는 돌아가고 트래킹만 제거할때 사용
    /// </summary>
    /// <param name="_active">트래킹 정지 여부</param>
    public void PauseTracker(bool _active)
    {
        if (ARBehaviour == SCRIPT_ImageSample) ((ImageTrackerSample)ARBehaviour).b_PauseTracker = _active;
        else if (ARBehaviour == SCRIPT_InstantSample) ((InstantTrackerSample)ARBehaviour).b_PauseTracker = _active;
    }


    /// <summary>
    /// 카메라 모드 별 초기화 변수 호출
    /// (IMAGE : 마커인식, Instant : 지면인식),
    /// </summary>
    /// <param name="_CUSTOM">카메라 인식 모드 </param>
    // 기본적으로 Switch_Mode를 통해 StartTracker함수 호출시 초기화되지만 런타임 중 초기화를 해야하는 경우 해당 함수 호출
    public void InitializeTracker()
    {
        if (ARBehaviour == SCRIPT_ImageSample)
        {
            //Debug.Log("ImageTrackerSample Setting Activate");
            ((ImageTrackerSample)ARBehaviour).SetTrackerInfo();
        }
        else if (ARBehaviour == SCRIPT_InstantSample)
        {
            //Debug.Log("InstantTrackerSample Setting Activate");
            ((InstantTrackerSample)ARBehaviour).SetTrackerInfo();
        }
    }

    /// <summary>
    /// 모든 트래킹 데이터 제거
    /// </summary>
    public void DisableTrackers()
    {
        if (SCRIPT_ImageSample == null) return;

        SCRIPT_ImageSample.DisableTracker();
    }

    /// <summary>
    /// 지면인식 기능 호출 함수
    /// </summary>
    public void FindSurface()
    {
        SCRIPT_InstantSample.OnClickFindSurface();
    }
}
