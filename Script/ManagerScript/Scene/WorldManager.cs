
using Manager.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace Manager
{
    public class WorldManager : MiniGameManager
    {
        private const float f_ZOOM_SPEED = 0.1f;

        private string[] CUT_SCENE = {
            //"CUT_00_Cup", "N", "N", "N", "N", "N", "N", "N", "N", "N",
            "N", "N", "N", "N", "N", "N", "N", "N", "N", "N",
            "N", "N", "N", "N", "N", "N", "N", "N", "N", "N",
            "N", "N", "N", "N", "N", "N", "N", "N", "N", "N",
            "N", "N", "N", "N", "N", "N", "N", "N", "N", "N",
            "N", "N", "N", "N", "N", "N", "N", "N", "N", "N",
            "N", "N"};

        //---------- VARIABLES ----------

        [Header("------스크롤 뷰------")]
        [SerializeField] private RectTransform RT_BottemPanel = null;
        [SerializeField] private RectTransform RT_CharacterContent = null;
        [SerializeField] private RectTransform RT_LockCharacterContent = null;
        [SerializeField] private GameObject PF_CharacterButtonPrefab;
        [SerializeField] private GameObject PF_LockCharacterButtonPrefab;
        [SerializeField] private Text TXT_RescueCount;

        [Header("------모델 리스트------")]
        [SerializeField] private List<GameObject> L_Model = new List<GameObject>();

        [Header("------게임 오브젝트------")]
        [SerializeField] private Transform TRAN_Target;
        [SerializeField] private Transform TRAN_WorldOriginal;
        [SerializeField] private GameObject IMG_NameTag = null;
        [SerializeField] private Text TXT_Name = null;

        private Vector3 V3_Previous;
        private float f_PrevDist = 0.0f;

        private int i_CurrentModel = -1;
        private float f_TargetRange = -70f;
        private const float f_WORLD_RANGE = -63f;
        private const float f_CHARACTER_RANGE = -10f;

        [Header("------ 리소스 -------")]
        public DigitalRubyShared.FingersCameraMove3DComponentScript fingers;

        private Outline OL_PrevOutline = null;


        //---------- INITIALIZING ----------

        void Start()
        {
            Init_MinigameManager();
            ResetMinigameList();

            Initialize();
            ButtonInitializer();


            if (LoadMaster.S.GetLoadPanelActive()) LoadMaster.S.SetLoadPanel(false);
        }

        private void Initialize()
        {
            TRAN_Target = TRAN_WorldOriginal;
            f_TargetRange = f_WORLD_RANGE;
        }

        //---------- UPDATE ----------

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Camera.main.fieldOfView = 10f;
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                Camera.main.fieldOfView = 60f;
            }


            if (f_TargetRange == f_CHARACTER_RANGE)
            {
                Camera.main.transform.position = TRAN_Target.position;
                Camera.main.transform.Translate(new Vector3(0, 0, f_TargetRange));
            }

            // 줌인/줌아웃
            if (Input.touchCount > 1)
            {
                Touch[] touch = new Touch[] { Input.GetTouch(0), Input.GetTouch(1) };

                Vector2 firstTouchPrev = touch[0].position - touch[0].deltaPosition;
                Vector2 secondTouchPrev = touch[1].position - touch[1].deltaPosition;

                float prevTouch = (firstTouchPrev - secondTouchPrev).magnitude;
                float touchDelta = (touch[0].position - touch[1].position).magnitude;

                float delta = prevTouch - touchDelta;

                Camera.main.fieldOfView += delta * f_ZOOM_SPEED;
                Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, 10f, 60f);
            }
            else
            {
#if UNITY_EDITOR// 터치시 오브젝트 확인 후 사운드와 애니메이션 실행
                if (Input.GetMouseButtonDown(0))
                {
                    V3_Previous = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                }

                // 이동, FingerCameraMoveComponentScript에서 실행
                // 특정 기준점을 잡아 카메라 회전
                if (Input.GetMouseButton(0) && !fingers.Get_IsWorld())
                {
                    Vector3 direction = V3_Previous - Camera.main.ScreenToViewportPoint(Input.mousePosition);

                    Camera.main.transform.position = TRAN_Target.position;

                    // x축(상하) 회전 = x회전값 변경, 5~90도 사이 제한
                    Vector3 currentRotation = Camera.main.transform.localRotation.eulerAngles;
                    currentRotation.x = Mathf.Clamp(currentRotation.x + (direction.y * 180), 5, 90);
                    Camera.main.transform.localRotation = Quaternion.Euler(currentRotation);

                    // y축(좌우) 회전 = x회전값 변경
                    Camera.main.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180, Space.World); // x축 회전
                    Camera.main.transform.Translate(new Vector3(0, 0, f_TargetRange));

                    V3_Previous = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                }
#else
                if (Input.touchCount > 0)
                {
                    UnityEngine.Touch touch = Input.GetTouch(0);

                    try { Debug.Log("Touch Began Prev : " + touch.phase.Equals(UnityEngine.TouchPhase.Began)); }
                    catch (System.Exception e) { }

                    if (touch.phase.Equals(UnityEngine.TouchPhase.Began))
                    {
                        V3_Previous = Camera.main.ScreenToViewportPoint(Input.touches[0].position);
                    }
                }

                // 이동, FingerCameraMoveComponentScript에서 실행
                // 특정 기준점을 잡아 카메라 회전
                //Debug.Log("Rotate : " + Input.touchCount);
                if (Input.touchCount > 0 && !fingers.Get_IsWorld())
                {
                    UnityEngine.Touch touch = Input.GetTouch(0);

                    try { Debug.Log("Touch Move Rotate : " + touch.phase.Equals(UnityEngine.TouchPhase.Moved)); }
                    catch (System.Exception e) { }

                    if (touch.phase.Equals(UnityEngine.TouchPhase.Moved))
                    {
                        Vector3 direction = V3_Previous - Camera.main.ScreenToViewportPoint(touch.position);

                        Camera.main.transform.position = TRAN_Target.position;

                        // x축(상하) 회전 = x회전값 변경, 5~90도 사이 제한
                        Vector3 currentRotation = Camera.main.transform.localRotation.eulerAngles;
                        currentRotation.x = Mathf.Clamp(currentRotation.x + (direction.y * 180), 5, 90);
                        Camera.main.transform.localRotation = Quaternion.Euler(currentRotation);

                        // y축(좌우) 회전 = x회전값 변경
                        Camera.main.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180, Space.World); // x축 회전
                        Camera.main.transform.Translate(new Vector3(0, 0, f_TargetRange));

                        V3_Previous = Camera.main.ScreenToViewportPoint(touch.position);
                    }
                }
#endif
            }
        }

        //---------- FUNCTION ----------

        //---------- BUTTON LISTENER ----------
        [Header("------ 버튼 ------")]
        [SerializeField] private Button BTN_InventoryElevator = null;
        [SerializeField] private Button BTN_PhotoShot = null;
        [SerializeField] private Button BTN_LockToSave = null;
        [SerializeField] private Button BTN_NameTagExit = null;
        [SerializeField] private Button BTN_Film = null;
        [Space(5)]
        [SerializeField] private GameObject PAN_TopPanel = null;
        [SerializeField] private Slider SLIDER_Toggle = null;
        [SerializeField] private GameObject SV_Save = null;
        [SerializeField] private GameObject SV_Lock = null;


        private RectTransform RT_InventoryElevator = null;

        private const int UP_POSY = 580;
        private const int DOWN_POSY = -450;

        private bool b_isLock = true;
        private bool b_InventoryUp = true;


        /// <summary>
        /// 인벤토리 올리기/내리기
        /// </summary>
        public void InventoryScroller()
        {
            if (b_InventoryUp) // 인벤토리가 올라와있으면 내려가기
            {
                StartCoroutine("InventorySlider", false);
            }
            else // 인벤토리가 내려가 있으면 올라오기
            {
                StartCoroutine("InventorySlider", true);
            }

            b_InventoryUp = !b_InventoryUp;
        }

        /// <summary>
        /// 인벤토리 초기화 함수.
        /// 1. 게임씬의 활성화될 애니블록 오브젝트 검사
        /// 2. 정답을 확인한 도안 확인
        /// </summary>
        private void ButtonInitializer()
        {
            // 인터페이스를 내리고 올리는 버튼 초기화
            RT_InventoryElevator = BTN_InventoryElevator.GetComponent<RectTransform>();

            //Debug.Log(Get_MinigameSuccessLength());
            // 구출에 성공한 버튼 리스트 생성
            int sucess = 0;
            for (int i = 0; i < Get_MinigameSuccessLength() - 1; i++)
            {
                if (Get_IsSuccess(i))
                //if (true) // Test
                {
                    sucess++;

                    // 특정 모델 활성화
                    L_Model[i].SetActive(true);

                    // 모델 오브젝트의 명칭 중 "_" 뒤에 오는 단어를 저장, 모델 선택시 이름 표시를 위해 저장
                    string name = L_Model[i].name.Split('_')[1];

                    GameObject game = Instantiate(PF_CharacterButtonPrefab, RT_CharacterContent); // 버튼 생성
                    Image gameImage = game.transform.GetChild(0).GetComponent<Image>();             // 버튼 이미지 초기화                    
                                                                                                    // 패키지별로 지정된 3D 이미지 호출

                    AsyncAssetLoadManager.Instance.Get_LoadAssetSprite(PackageManager.Instance.s_3DMarkerAddress[i], ref gameImage);


                    // 버튼 아웃라인 컨포넌트 링크
                    Outline outline = game.GetComponent<Outline>();

                    int num = i; // 새로운 정수 값을 메모리에 저장
                    // 버튼 오브젝트를 람다식으로 초기화
                    game.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (i_CurrentModel == num) // 같은 모델을 선택할 경우
                        {
                            if (f_TargetRange == f_WORLD_RANGE) // 현재 카메라 목표가 없을 경우
                            {
                                f_TargetRange = f_CHARACTER_RANGE; // 캐릭터를 줌인하는 값으로 카메라값 변경
                                TRAN_Target = L_Model[num].transform; // 해당 캐릭터의 위치를 기준으로 회전하도록 설정

                                outline.enabled = true; // 아웃라인 활성화                                

                                IMG_NameTag.gameObject.SetActive(true); // 이름표 오브젝트 활성화

                                fingers.Set_IsWorld(false); // 터치패드 모드를 모델뷰로 설정

                                Camera.main.fieldOfView = 60f; // 확대
                            }
                            else // 현재 카메라 목표가 캐릭터를 목표로할 경우
                            {
                                f_TargetRange = f_WORLD_RANGE;              // 원래 위치로 이동

                                outline.enabled = false;                    // 아웃라인 비활성화

                                IMG_NameTag.gameObject.SetActive(false);    // 이름표 오브젝트 비활성화

                                fingers.Set_IsWorld(true);                  // 터치패드 모드를 월드뷰로 설정
                                fingers.Set_Ease();                         // 터치 움직임 뎀핑 초기화

                                // 카메라를 초기 회전과 위치로 설정
                                Camera.main.transform.localRotation = Quaternion.Euler(32.884f, 0, 0);
                                Camera.main.fieldOfView = 60f;
                            }
                        }
                        else  // 다른 모델을 선택할 경우
                        {
                            i_CurrentModel = num; // 현재 선택된 모델 정보 변경

                            f_TargetRange = f_CHARACTER_RANGE;      // 캐릭터 줌인 값으로 카메라값 변경
                            TRAN_Target = L_Model[num].transform;   // 캐릭터 위치로 변경
                            
                            outline.enabled = true; // 버튼 아웃라인 활성화
                            
                            // 이전 아웃라인이 존재하면 해당 아웃라인 비활성화
                            if (OL_PrevOutline != null) OL_PrevOutline.enabled = false;
                            OL_PrevOutline = outline;

                            IMG_NameTag.gameObject.SetActive(true); // 이름표 오브젝트 활성화
                            TXT_Name.text = name;                   // 이름표 텍스트 변경

                            fingers.Set_IsWorld(false); // 터치패드 모드를 모델뷰로 설정
                        }

                        Camera.main.transform.position = TRAN_Target.position; // 카메라 이동
                        Camera.main.transform.Translate(new Vector3(0, 0, f_TargetRange)); // 카메라 기본 거리 초기화

                        V3_Previous = Camera.main.ScreenToViewportPoint(Input.mousePosition);


                        // 컷씬 정보 확인 및 컷씬 버튼 활성화 및 초기화
                        if (CUT_SCENE[num] == "N")
                        {
                            BTN_Film.gameObject.SetActive(false);
                        }
                        else
                        {
                            BTN_Film.gameObject.SetActive(true);
                            BTN_Film.onClick.RemoveAllListeners();
                            BTN_Film.onClick.AddListener(() => { LoadMaster.S.LoadSceneFunc(CUT_SCENE[num]); });
                        }


                        Manager.Sound.SoundManager.S.Play_ButtonSound();
                    });

                    // 구출 성공 여부 확인 버튼 이미지 초기화
                    RT_LockCharacterContent.transform.GetChild(i).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
                }
            }

            // 취소 활성화
            BTN_NameTagExit.onClick.AddListener(() =>
            {
                f_TargetRange = f_WORLD_RANGE; // 원래 위치로 이동
                TRAN_Target = TRAN_WorldOriginal; // 원래 위치에서 회전하도록 설정

                Camera.main.transform.localRotation = Quaternion.Euler(32.884f, 0, 0);
                Camera.main.fieldOfView = 60f;

                Camera.main.transform.position = TRAN_Target.position; // 카메라 이동
                Camera.main.transform.Translate(new Vector3(0, 0, f_TargetRange));

                V3_Previous = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                if (OL_PrevOutline != null) OL_PrevOutline.enabled = false; // 활성화 되어있는 아웃라인 제거

                IMG_NameTag.gameObject.SetActive(false); // 이름표 비활성화

                fingers.Set_IsWorld(true); // 터치모드를 월드 모드로 변경
                fingers.Set_Ease();        // 터치모드 뎀핑 초기화

                Manager.Sound.SoundManager.S.Play_ButtonSound();
            });

            // 세이브와 락 변경
            BTN_LockToSave.onClick.AddListener(() => {
                SV_Save.SetActive(!b_isLock);
                SV_Lock.SetActive(b_isLock);

                SLIDER_Toggle.value = (b_isLock) ? 1 : 0;

                b_isLock = !b_isLock;
                Manager.Sound.SoundManager.S.Play_ButtonSound();
            });
            SV_Lock.SetActive(false);

            // if()
            switch (SceneManager.GetActiveScene().name)
            {
                case "Scene_World_First":
                    TXT_RescueCount.text = sucess + "/" + (int)Manager.Util.PackageManager.FirstCollection.NumOf;
                    break;
                case "Scene_World_PRR":
                    TXT_RescueCount.text = sucess + "/" + (int)Manager.Util.PackageManager.PpororoCollection.NumOf;
                    break;
            }
        }

        /// <summary>
        /// 인벤토리 엘레베이터
        /// </summary>
        /// <param name="_InventoryUp">true : 올라가기, false : 내려가기</param>
        /// <returns></returns>
        private IEnumerator InventorySlider(bool _InventoryUp)
        {
            Manager.Sound.SoundManager.S.Play_ButtonSound();

            Vector3 rot = RT_InventoryElevator.eulerAngles;
            rot.z += 180;
            RT_InventoryElevator.eulerAngles = rot;

            //float lerp = (_InventoryUp) ? 40f : -40f;
            float lerp = (_InventoryUp) ? 80f : -80f; // 인벤토리 이동 속도
            Vector2 rct = RT_BottemPanel.anchoredPosition;

            BTN_InventoryElevator.interactable = false;
            BTN_PhotoShot.interactable = false;


            while (true)
            {
                if (b_InventoryUp && rct.y > UP_POSY)
                {
                    rct.y = UP_POSY;
                    RT_BottemPanel.anchoredPosition = rct;
                    break;
                }
                else if (!b_InventoryUp && rct.y < DOWN_POSY)
                {
                    rct.y = DOWN_POSY;
                    RT_BottemPanel.anchoredPosition = rct;
                    break;
                }

                rct.y += lerp;
                RT_BottemPanel.anchoredPosition = rct;

                yield return new WaitForEndOfFrame();
            }

            BTN_InventoryElevator.interactable = true;
            BTN_PhotoShot.interactable = true;
        }
    }
}