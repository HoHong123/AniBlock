using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Manager.Util;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LiveGameManager : MiniGameManager
{
    #region 상수 값 모음
    private const int DEFAULT_TIMER = 60;
    private string DIRECTORY = "Assets/Unsign_Resources/3D_Models/3D_LiveGame/LiveGameModels/";
    #endregion

    #region 자료형
    [System.Serializable] public class BubblePrefab
    {
        public bool b_IsAnswer = false;
        public SphereCollider SC_Collider;
        public Transform TRAN_Pivot;
        public SpriteRenderer SPT_Bubble;
    }
    [System.Serializable] public class LiveGamePrefab
    {
        public string s_Name = "";
        [Range(0,3)] public int i_Rot = 0;
        public GameObject p_Body = null;
    }
    [System.Serializable] public class LiveGameResources {
        public Sprite STP_Answer = null;
        public GameObject L_Prefab;
    }
    #endregion

    [Header("====프리팹====")]
    [SerializeField] private List<BubblePrefab>   L_BubblePrefabs = new List<BubblePrefab>(10); // 버블 프리팹 정보 리스트

    [Header("====버블 카테고리====")]
    private List<Transform> L_FaceCameraList = new List<Transform>(); // 버블 프리팹의 스프라이트 리스트
    private float f_RotationSpeed = 15.0f;                            // 버블 프리팹 내부 오브젝트의 회전속도



    [Header("====== 웹캠 ======")]
    // INFO - WebCame
    [SerializeField] private GameObject WebcamFrame = null;     // 웹캠 프레임 스크립트

    private WebCamTexture wct = null;                           // 바닥 표현을 위한 웹캠 텍스쳐



    [Header("====== 게임 오브젝트 ======")]
    // 미니게임 카메라
    [SerializeField] private Camera OBJ_GameCamera = null;      // 게임 카메라 (백그라운드 카메라와 다름)
    [SerializeField] private LayerMask LAYER_Plane;             // 배경을 나타낼 Plane의 레이어마스크

    [Space(5)]
    // 카메라를 벗어날때 나타나는 가이드 UI
    [SerializeField] private GameObject OBJ_LeftPointer = null; 
    [SerializeField] private GameObject OBJ_RightPointer = null;



    [Space(5),Header("---- 플레이어 캐릭터 ----")]
    [SerializeField] private Transform TRAN_StartPos;           // 캐릭터 초기 위치 값
    [SerializeField] private Transform TRAN_OBJCharacter;       // 캐릭터 위치 값
    [SerializeField] private Transform TRAN_Pivot;              // 캐릭터 모델의 피봇 위치
    [SerializeField] private SpriteRenderer SPT_Guide = null;   // 캐릭터 숨기 중 바닥에 표시될 도안 스프라이트
    [SerializeField] private GameObject OBJ_StunStar = null;    // 캐릭터 기절시 발생하는 효과 오브젝트
    [SerializeField] private BoxCollider BC_Character;          // 캐릭터 박스 콜라이더

    private bool b_IsRot = false;                               // 초기 로테이션이 필요한지 확인하는 bool값
    private Transform TRAN_Model;                               // 캐릭터 모델
    private Animator A_Animator = null;                         // 캐릭터 애니메이터
    private Vector3 V3_TargetPosition;                          // 캐릭터가 이동할 다음 위치

    private LiveGamePrefab PlayerInfo = null;                   // 캐릭터 정보
                                                                // 1. 이름, 2. 회전값, 3. 카테고리, 4. 프리팹


    [Space(5), Header("---- 정답 말풍선 ----")]
    // 플레이어가 목표로하는 오브젝트를 보여주는 정답 말풍선
    [SerializeField] private Transform TRAN_AnswerBubblePivot;  // 정답 말풍선 피봇 위치 값
    [SerializeField] private Transform TRAN_AnswerBubble;       // 정답 말풍선 위치
    [SerializeField] private SpriteRenderer SPT_AnswerPic;      // 정답 말풍선 스프라이트
    [SerializeField] private List<int> l_SelectedMarker = new List<int>();
    [SerializeField] private List<GameObject> l_CharacterModel = new List<GameObject>();



    [Header("====== 게임 세팅 ======")]
    [SerializeField] private Text TXT_Score;                    // 게임 스코어 텍스트
    [SerializeField] private Text TXT_Timer;                    // 게임 타이머 텍스트
    [SerializeField] private Text TXT_Goal;                     // 게임 목표 텍스트
    
    [SerializeField] private float f_Speed = 0;                 // 캐릭터 이동속도
    private float f_DefaultSpeed = 0;                           // 기본 이동 속도
    private int i_Score = 0;                                    // 현재 스코어 값
    private int i_Fail = 0;                                     // 현재 실패한 횟수 값
    private int DEFAULT_ANSWER_COUNT = 3;                       // 기본 완료를 위해 기본으로 필요한 정답 수
    private bool b_isPause = false;                             // 게임 정지 확인 bool값
    public void Set_Pause(bool pause) { b_isPause = pause; }    // 게임 실행/정지 함수



    [Header("====== 버튼 ======")]
    [SerializeField] private Button BTN_Guide;                  // 숨기 버튼
    [SerializeField] private Button BTN_Boost;                  // 부스트 버튼

    private bool b_isFreez = false;                             // 현재 기절 상태인지 확인하는 bool값
    private bool b_isGuide = false;                             // 현재 숨은 상태인지 확인하는 bool값
    private bool b_isBoost = false;                             // 현재 부스트 상태인지 확인하는 bool값



    [Header("====== 오브젝트 ======")]
    [SerializeField] private ParticleSystem PS_Pointer;         // 터치한 위치에 나타날 파티클 시스템


    // INFO - MiniGame Gyro Sensor
    private Gyroscope gyro = null;                              // 자이로센서
    private int initialOrientationX;                            // 자이로센서의 x축 회전값
    private int initialOrientationY;                            // 자이로센서의 y축 회전값


    // INFO - Game Setting
    private float f_Timer = DEFAULT_TIMER;                      // 게임 타이머
    private int i_PrevTime = 0;                                 // 게임에서 지나간 시간
    private int i_MaxPackageSize = 0;                           // 최대 패키지 개수

    private Live_ObsticleManager SCRIPT_ObsticleManager = null; // 장애물 스크립트
    public GameObject test = null;


    private void Start()
    {
        InitGame();
        StartCoroutine(InitGyro());
    }

    private IEnumerator InitGyro()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            if(SceneManager.GetActiveScene().name == "Scene_MiniGame")
            {
                // 자이로 센서 등록
                gyro = Input.gyro;
                gyro.enabled = true;
                gyro.updateInterval = 0.01f;
                initialOrientationX = (int)Input.gyro.rotationRateUnbiased.x;
                initialOrientationY = (int)Input.gyro.rotationRateUnbiased.y;

                break;
            }
        }
    }

    private void InitGame()
    {
        #region 캐릭터 프리팹
        V3_TargetPosition = TRAN_OBJCharacter.position; // 버블 생성 위치 설정을 위해 캐릭터 위치 파악

        switch (_PackageManager.CURRENT_PACKAGE_INFO)
        {
            case PackageManager.PACKAGE_INFO.FIRST:
                DIRECTORY += "FirstCollection/MINI_";
                i_MaxPackageSize = (int)PackageManager.FirstCollection.NumOf;
                PlayerInfo = _xmlManager.LoadLiveData(_PackageManager.FC_PACKAGE.ToString());       // 현재 인식된 도안의 이름으로 XML파일 LiveGameInfo에서 정보 확보
                break;

            case PackageManager.PACKAGE_INFO.RENEWEL:
                DIRECTORY += "Renewal/MINI_";
                i_MaxPackageSize = (int)PackageManager.RenewelCollection.NumOf;
                PlayerInfo = _xmlManager.LoadLiveData(_PackageManager.RE_PACKAGE.ToString());   // 현재 인식된 도안의 이름으로 XML파일 LiveGameInfo에서 정보 확보
                break;

            case PackageManager.PACKAGE_INFO.PPORORO:
                DIRECTORY += "Ppororo/MINI_";
                i_MaxPackageSize = (int)PackageManager.PpororoCollection.NumOf;
                PlayerInfo = _xmlManager.LoadLiveData("PPORORO");   // 현재 인식된 도안의 이름으로 XML파일 LiveGameInfo에서 정보 확보
                break;

            default:
                Debug.LogError("No Package Info is set :: " + _PackageManager.CURRENT_PACKAGE_INFO);
                break;
        }

        SPT_Guide.sprite = _PackageManager.STRUCT_CurrentMarkerSpritesInfo.SPT_Pattern;     // 바닥 도안 스프라이트 설정

        TRAN_Model = Instantiate(PlayerInfo.p_Body, TRAN_Pivot).transform;          // 모델 프리팹 생성
        TRAN_Model.transform.localPosition = Vector3.zero;                          // 모델 위치 초기화
        TRAN_Pivot.localRotation = Quaternion.Euler(0, -90 * PlayerInfo.i_Rot, 0);  // 회전 값만큼 모델 회전

        A_Animator = TRAN_Model.GetComponent<Animator>();             // 모델 애니메이터 링크
        BC_Character = TRAN_OBJCharacter.GetComponent<BoxCollider>(); // 모델 박스 콜라이더 링크

        OBJ_StunStar.SetActive(false);  // 모델 기절 이펙트 비활성화

        f_DefaultSpeed = f_Speed;       // 이동속도 초기화
        #endregion

        #region 버블 프리팹
        // 정답 버블 프리팹들 초기화
        GameObject prefab = null;
        for (int i = 0; i < L_BubblePrefabs.Count; i++)
        {

            // 버블 스프라이트를 항상 카메라로 바라보도록 Update해줄 리스트
            L_FaceCameraList.Add(L_BubblePrefabs[i].SPT_Bubble.transform);

            // 프리팹의 종류에 따라 버블 속 내용물을 변경하기 위한 버블 초기화
            while (true)
            {
                int rand = 0;
                rand = Random.Range(0, i_MaxPackageSize);

                if (!l_SelectedMarker.Contains(rand))
                {
                    l_SelectedMarker.Add(rand);
                    break;
                }
            }

            switch (_PackageManager.CURRENT_PACKAGE_INFO)
            {
                case PackageManager.PACKAGE_INFO.FIRST:
                    l_CharacterModel.Add(Addressables.Instantiate(DIRECTORY + _PackageManager.PR_PACKAGE.ToString() + ".prefab").Result);
                    //AsyncAssetLoadManager.Instance.Get_LoadAssetObject(DIRECTORY + _PackageManager.PR_PACKAGE.ToString() + ".prefab", test);
                    break;

                case PackageManager.PACKAGE_INFO.RENEWEL:
                    break;

                case PackageManager.PACKAGE_INFO.PPORORO:
                    break;

                default:
                    Debug.LogError("No Package Info is set :: " + _PackageManager.CURRENT_PACKAGE_INFO);
                    break;
            }

            prefab = Instantiate(l_CharacterModel[i], L_BubblePrefabs[i].TRAN_Pivot);
            prefab.transform.localPosition = new Vector3(0, -0.6f, 0);
            prefab.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }

        // 플레이어가 회전이 필요한지 확인
        b_IsRot = (PlayerInfo.i_Rot > 0) ? true : false;

        // 정답 버블 위치 설정
        AllocateBubblePosition();

        // 도안 정답을 확인한 리스트 초기화
        _PackageManager.Set_AccessableMarkerCount();

        // 미니게임 매니저 초기화
        Init_MinigameManager();
        int goal = (int)(Get_RescuedCount() / 13);  // 목표 점수를 도안 정답을 맞춘 수에 비례하여 증가
        DEFAULT_ANSWER_COUNT = 3 + goal;

        InitializeExmpleText(); // 게임 목표 텍스트 변경
        #endregion

        #region 버튼 초기화
        BTN_AnimationButtonSetting();
        #endregion


        if (LoadMaster.S.GetLoadPanelActive()) LoadMaster.S.SetLoadPanel(false);
    }

    private void ObjectLoad(AsyncOperationHandle<GameObject> obj)
    {
        switch (obj.Status)
        {
            case AsyncOperationStatus.Succeeded:
                l_CharacterModel.Add(obj.Result);
                break;

            case AsyncOperationStatus.Failed:
                Debug.LogError("ASALManager(LoadObject) :: Load Fail");
                break;

            case AsyncOperationStatus.None:
                Debug.LogError("ASALManager(LoadObject) :: Load None");
                break;
        }

        Addressables.Release(obj);
    }

    // ------ UPDATE ------

    private void Update()
    {
        // 카메라 업데이트 (카메라 회전, 배경 텍스쳐 업데이트)
        UPDATE_CameraManagement();

        if (b_isPause) return; // 일시 정지시 반환

        //f_Timer = 999; // Test
        //DEFAULT_ANSWER_COUNT = 9999; // Test
        // 타이머 계산
        if (f_Timer < 0 || i_Score == DEFAULT_ANSWER_COUNT)
        {
            // 승패 여부 확인
            StartCoroutine(SetResult());
            enabled = false;
            return;
        }

        f_Timer -= Time.deltaTime; // 타이머 감소
        if(f_Timer < 11 && i_PrevTime != (int)f_Timer) // 10초 이하부터 매초마다 사운드 플레이
        {
            Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.MINIGAME_CLIP_LIST.M_Timer);
            i_PrevTime = (int)f_Timer;
        }
        TXT_Timer.text = ((int)f_Timer).ToString(); // 타이머 텍스트 설정

        // 프리팹 설정 (모든 정답 버블이 카메라를 향하도록 설정, 정답 버블과 충돌처리 확인, 정답 버블 위치 재설정)
        UPDATE_PrefabManagement();

        // 숨기 혹은 기절 상태이면 반환
        if (b_isGuide || b_isFreez) return;

        // 캐릭터 움직임 함수 호출 (터치, 버튼, 현재 상태)
        UPDATE_InputManagement();
    }

    private void UPDATE_PrefabManagement()
    {
        // 정답 버블 카메라를 향하게 회전
        Vector3 dir = TRAN_AnswerBubble.position - OBJ_GameCamera.transform.position;
        TRAN_AnswerBubble.rotation = Quaternion.LookRotation(dir, OBJ_GameCamera.transform.rotation * Vector3.up);

        TRAN_AnswerBubblePivot.position = TRAN_OBJCharacter.position;

        // 버블 내부의 오브젝트 회전
        for (int i = 0; i < L_BubblePrefabs.Count; i++)
        {
            // 버블 스프라이트를 카메라를 바라보도록 회전
            dir = L_BubblePrefabs[i].SPT_Bubble.transform.position - OBJ_GameCamera.transform.position;
            L_BubblePrefabs[i].SPT_Bubble.transform.rotation = Quaternion.LookRotation(dir, OBJ_GameCamera.transform.rotation * Vector3.up);


            // 버블 내부 캐릭터 회전
            L_BubblePrefabs[i].TRAN_Pivot.Rotate(Vector3.up * Time.deltaTime * f_RotationSpeed);


            // 버블 콜라이더 충돌 확인
            if (L_BubblePrefabs[i].SC_Collider.bounds.Intersects(BC_Character.bounds))
            {
                L_BubblePrefabs[i].SC_Collider.enabled = false;

                //Debug.Log("COLLIDE");
                if (L_BubblePrefabs[i].b_IsAnswer)
                {
                    // 점수 증가
                    //Debug.LogError("성공");
                    Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.MINIGAME_CLIP_LIST.M_LIVE_Correct);
                    TXT_Score.text = (++i_Score).ToString();
                }
                else
                {
                    // 실패
                    //Debug.LogError("실패");
                    Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.MINIGAME_CLIP_LIST.M_LIVE_Wrong);
                    FreezCharacter();
                }

                AllocateBubblePosition(); // 버블 위치 및 정답 초기화
            }
        }
    }

    private void UPDATE_CameraManagement()
    {
        if (gyro == null) return;

        OBJ_GameCamera.transform.Rotate(initialOrientationX - (Input.gyro.rotationRateUnbiased.x * 0.8f), 
                                        initialOrientationY - (Input.gyro.rotationRateUnbiased.y * 1.0f),
                                        0.0f);
        OBJ_GameCamera.transform.rotation = Quaternion.Euler(new Vector3(OBJ_GameCamera.transform.eulerAngles.x, OBJ_GameCamera.transform.eulerAngles.y, 0.0f));

        SetCameraPointer();
    }

    private void UPDATE_InputManagement()
    {
        if (EventSystem.current.currentSelectedGameObject != null) return;
        
        #if UNITY_EDITOR
        // 터치가 발생되면
        if (Input.GetMouseButtonDown(0))
            {
                // 레이를 쏘아 Plane과 적중한 위치의 위치값으로 이동
                RaycastHit hit = new RaycastHit();
                Ray ray = OBJ_GameCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray.origin, ray.direction, out hit, 50f, LAYER_Plane))
                {
                    //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1.0f);

                    V3_TargetPosition = hit.point;
                    TRAN_OBJCharacter.LookAt(V3_TargetPosition);
                    
                    // 터치 위치에 파티클 이동 후 플레이
                    PS_Pointer.transform.position = V3_TargetPosition;
                    PS_Pointer.Play();
                }
            }
#elif UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0)
            {
                UnityEngine.Touch touch = Input.GetTouch(0);

                try { Debug.Log("Touch Mini : " + touch.phase.Equals(UnityEngine.TouchPhase.Began)); }
                catch (System.Exception e) { }

                if (touch.phase == UnityEngine.TouchPhase.Began)
                { 
                    // 레이를 쏘아 Plane과 적중한 위치의 위치값으로 이동
                    RaycastHit hit = new RaycastHit();
                    Ray ray = OBJ_GameCamera.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray.origin, ray.direction, out hit, 50f, LAYER_Plane))
                    {
                        //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.blue, 1.0f);

                        V3_TargetPosition = hit.point;
                        TRAN_OBJCharacter.LookAt(V3_TargetPosition);

                        // 터치 위치에 파티클 이동 후 플레이
                        PS_Pointer.transform.position = V3_TargetPosition;
                        PS_Pointer.Play();
                    }
                }
            }
#endif

        if (Vector3.Distance(TRAN_OBJCharacter.transform.position, V3_TargetPosition) > 0.1f)
        {
            TRAN_OBJCharacter.transform.position = Vector3.MoveTowards(TRAN_OBJCharacter.transform.position, V3_TargetPosition, Time.deltaTime * f_Speed);
            A_Animator.SetTrigger("M1");
        }
        else
        {
            A_Animator.ResetTrigger("M1");
            A_Animator.SetTrigger("I2");
        }
    }

    // ------ FUNCTION ------

    /// <summary>
    /// 캐릭터가 카메라 밖을 나가면 캐릭터의 위치를 표시해주는 화살표 활성화
    /// </summary>
    private void SetCameraPointer()
    {
        Vector3 targetPositionScreenPoint = OBJ_GameCamera.WorldToScreenPoint(BC_Character.transform.position);
        
        if(targetPositionScreenPoint.x <= 0)
        {
            OBJ_LeftPointer.gameObject.SetActive(true);
            OBJ_RightPointer.gameObject.SetActive(false);
        }
        else if(targetPositionScreenPoint.x >= Screen.width)
        {
            OBJ_LeftPointer.gameObject.SetActive(false);
            OBJ_RightPointer.gameObject.SetActive(true);
        }
        else
        {
            OBJ_LeftPointer.SetActive(false);
            OBJ_RightPointer.SetActive(false);
        }
    }
    
    /// <summary>
    /// 정답 버블 위치 변경 함수
    /// </summary>
    /// 캐릭터를 기준으로 서로 일정 거리만큼 간격을 가지고 배치
    private void AllocateBubblePosition()
    {
        int chance = 100; // 무한 반복을 예외처리하기 위한 체크 변수

        int i = 0;
        int answer = Random.Range(0, 7); // 정답 지정 변수
        while (i < L_BubblePrefabs.Count)
        {
            // 현재 버블이 정답 버블로 확인되면 정답으로 설정
            L_BubblePrefabs[i].b_IsAnswer =  (i == answer) ? true : false; 

            // 예외처리가 되면 버블 위치 초기화 및 다음 버블로 이동
            if (chance == 0)
            {
                // 예외처리된 버블이 정답일 경우 다른 버블을 정답으로 처리
                if (i > 0 && i < 7) answer--;
                else answer++;

                L_BubblePrefabs[i].b_IsAnswer = false; // 정답 초기화
                L_BubblePrefabs[i].SC_Collider.transform.position = Vector3.zero; // 위치 초기화
                chance = 100; // 예외 카운트 초기화
                i++;          // 다음 버블로 이동
                continue;
            }

            Vector3 newPos = new Vector3(
                Random.Range(TRAN_StartPos.position.x - 15f, TRAN_StartPos.position.x + 15f), 
                1.5f, 
                Random.Range(TRAN_StartPos.position.z, TRAN_StartPos.position.z + 20f));

            // 플레이어와 가까우면 다시 계산
            if (Vector3.Distance(newPos, TRAN_OBJCharacter.position) < 5.0f)
            {
                //Debug.Log("Close with character : " + Vector3.Distance(newPos, TRAN_OBJCharacter.position));
                chance--;
                continue;
            }

            // 이전에 배치된 버블들과 가까우면 다시 계산
            int current = 0;
            do
            {
                if (Vector3.Distance(newPos, L_BubblePrefabs[current].SC_Collider.transform.position) < 5.0f)
                {
                    //Debug.Log("Close with Bubble["+ current +"] : " + Vector3.Distance(newPos, TRAN_OBJCharacter.position));
                    chance--;
                    current = -1;
                    break;
                }

                current++;
            } while (current < i + 1);

            if (current == -1) continue;

            L_BubblePrefabs[i].SC_Collider.transform.position = newPos;
            i++;
        }

        // 정답 말풍선 변경
        AsyncAssetLoadManager.Instance.Get_LoadAssetSprite(_PackageManager.s_3DMarkerAddress[l_SelectedMarker[answer]], ref SPT_AnswerPic);
    }

    /// <summary>
    /// 게임 목표 텍스트 변경 함수
    /// </summary>
    private void InitializeExmpleText()
    {
        string goal = "";
        switch (I2.Loc.LocalizationManager.CurrentLanguage)
        {
            case Manager.Util.OptionManager.USER_LANG_KOR:
                goal += "캐릭터가 생각하고 있는 친구를 ";
                goal += DEFAULT_ANSWER_COUNT.ToString();
                goal += "번 찾으세요!";
                break;

            case Manager.Util.OptionManager.USER_LANG_CHS:
                TXT_Goal.font = FONT_Comic;
                goal += "请找";
                goal += DEFAULT_ANSWER_COUNT.ToString();
                goal += "次角色所认为的好友。";
                break;

            case Manager.Util.OptionManager.USER_LANG_CHT:
                TXT_Goal.font = FONT_Comic;
                goal = "請找"; 
                goal += DEFAULT_ANSWER_COUNT.ToString(); 
                goal += "次角色所認為的好友。";
                break;

            case Manager.Util.OptionManager.USER_LANG_JPN:
                TXT_Goal.font = FONT_Comic;
                goal = "キャラクターが考えている友達を" + DEFAULT_ANSWER_COUNT.ToString() + "回見つける";
                break;

            case Manager.Util.OptionManager.USER_LANG_ESP:
                goal = "Encuentra " + DEFAULT_ANSWER_COUNT.ToString() + " veces al amigo en el que está pensando el personaje";
                break;

            case Manager.Util.OptionManager.USER_LANG_USA:
                goal = "Collect " + DEFAULT_ANSWER_COUNT.ToString() + " items your character needs!";
                break;

            case Manager.Util.OptionManager.USER_LANG_RUS:
                goal = "Найдите друга " + DEFAULT_ANSWER_COUNT.ToString() + " раз, о котором думает персонаж";
                break;

            case Manager.Util.OptionManager.USER_LANG_DEU:
                goal = "Finde " + DEFAULT_ANSWER_COUNT.ToString() + "-mal den Freund, an den der Charakter denkt";
                break;

            case Manager.Util.OptionManager.USER_LANG_FRA:
                goal = "Retrouvez " + DEFAULT_ANSWER_COUNT.ToString() + " fois les amis qui sont passés dans la tête du personnage.";
                break;

            case Manager.Util.OptionManager.USER_LANG_NLD:
                goal = "Zoek uit hoevaak 'Karakter' " + DEFAULT_ANSWER_COUNT.ToString() + " 'Vriend' denkt.";
                break;

            case Manager.Util.OptionManager.USER_LANG_POL:
                goal = "Encontre o amigo " + DEFAULT_ANSWER_COUNT.ToString() + " vezes em quem o Personagem está a pensar";
                break;

            case Manager.Util.OptionManager.USER_LANG_ITA:
                goal = "Trova " + DEFAULT_ANSWER_COUNT.ToString() + " volte l'amico a cui il Personaggio sta pensando";
                break;

            default: // 영어
                goal = "Collect " + DEFAULT_ANSWER_COUNT.ToString() + " items your character needs!";
                break;
        }

        TXT_Goal.text = goal;
    }

    /// <summary>
    /// 결과 화면 표시 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetResult()
    {
        SCRIPT_ObsticleManager.Dispose();

        OBJ_LeftPointer.SetActive(false);
        OBJ_RightPointer.SetActive(false);

        bool Victory = (i_Score >= DEFAULT_ANSWER_COUNT) ? true : false; // 목표 이상이면 성공

        if (Victory)
        {
            // 승리
            SetVictory(i_Score, (int)f_Timer, i_Fail);
        }
        else
        {
            // 패배
            SetFail(i_Score, (int)f_Timer, i_Fail);
        }

        StopAllCoroutines();
        yield return null;
    }

    /// <summary>
    /// 캐릭터 기절 함수
    /// </summary>
    public void FreezCharacter()
    {
        StopCoroutine("SetFreez");
        StartCoroutine("SetFreez");
    }

    /// <summary>
    /// 캐릭터 기절상태 유지 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetFreez()
    {
        i_Fail++;
        b_isFreez = true;
        BC_Character.enabled = false;
        BTN_Boost.interactable = false;
        BTN_Guide.interactable = false;
        OBJ_StunStar.SetActive(true);

        f_Speed = f_DefaultSpeed;

        float timer = 2.0f;
        while(timer > 0)
        {
            if (!b_isPause)
            {
                timer -= Time.deltaTime;
            }

            yield return new WaitForEndOfFrame();
        }

        b_isFreez = false;
        BC_Character.enabled = true;

        if (!b_isBoost) BTN_Boost.interactable = true;

        BTN_Guide.interactable = true;
        OBJ_StunStar.SetActive(false);
    }

    /// <summary>
    /// 캐릭터 부스트 유지 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetBoost()
    {
        b_isBoost = true;
        f_Speed = f_DefaultSpeed * 2;
        BTN_Boost.interactable = false;

        float timer = 2.0f;
        while (timer > 0)
        {
            if (!b_isPause)
            {
                timer -= Time.deltaTime;
            }

            yield return new WaitForEndOfFrame();
        }

        f_Speed = f_DefaultSpeed;

        timer = 8.0f;
        while (timer > 0)
        {
            if (!b_isPause)
            {
                timer -= Time.deltaTime;
            }

            yield return new WaitForEndOfFrame();
        }

        b_isBoost = false;
        BTN_Boost.interactable = true;
    }

    /// <summary>
    /// 오브젝트를 납작하게 줄이는 함수
    /// </summary>
    /// <param name="goGuide">캐릭터가 숨을지 나올지 설정하는 매개변수</param>
    /// <returns></returns>
    private IEnumerator MakeitFlat(bool goGuide)
    {
        float lerp = -0.083f;
        float scale = TRAN_OBJCharacter.localScale.z;

        BC_Character.enabled = !goGuide;

        if (!goGuide)
        {
            lerp *= -1;
            scale = 0;
        }

        while (true)
        {
            if(goGuide && scale < 0)
            {
                TRAN_Model.localScale = Vector3.zero;
                // 애니메이션
                break;
            }
            else if(!goGuide && scale > 1)
            {
                TRAN_Model.localScale = Vector3.one;

                break;
            }

            scale += lerp;
            TRAN_Model.localScale = (b_IsRot) ? new Vector3(1, 1, scale) : new Vector3(1, scale, 1);

            yield return new WaitForEndOfFrame();
        }
    }

    // ------ BUTTON LISTENER ------

    private void BTN_AnimationButtonSetting()
    {
        BTN_Guide.onClick.AddListener(() => {
            if (!b_isGuide)
            {
                // 이동속도 초기화
                f_Speed = 0;

                // 미리 지정한 목적지를 현재위치로 초기화
                V3_TargetPosition = TRAN_OBJCharacter.position;

                // 눕는 애니메이션 실행
                A_Animator.SetTrigger("I3");

                // 부스트 버튼 정지
                BTN_Boost.interactable = false;

                // 가이드 이미지 출력
                SPT_Guide.gameObject.SetActive(true);

                // 납작하게 변경
                StopCoroutine("MakeitFlat");
                StartCoroutine("MakeitFlat", true);

                Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.MINIGAME_CLIP_LIST.M_LIVE_GuideDown);

                // 가이드 상태 입력
                b_isGuide = true;
            }
            else
            {
                // 이동속도 초기화
                f_Speed = 5f;

                // 일어서는 애니메이션 실행
                A_Animator.SetTrigger("I1");

                // 부스트 버튼 활성화
                if (!b_isBoost) BTN_Boost.interactable = true;

                // 가이드 이미지 비활성화
                SPT_Guide.gameObject.SetActive(false);

                // 원래 크기로 다시 변경
                StopCoroutine("MakeitFlat");
                StartCoroutine("MakeitFlat", false);

                Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.MINIGAME_CLIP_LIST.M_LIVE_GuideUp);

                // 가이드 상태 입력
                b_isGuide = false;
            }

            A_Animator.ResetTrigger("I2");
            A_Animator.ResetTrigger("M1");
        });

        BTN_Boost.onClick.AddListener(() => {
            StartCoroutine("SetBoost");
        });
    }
}
