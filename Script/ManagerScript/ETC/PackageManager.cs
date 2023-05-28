using System.Collections;
using System.Collections.Generic;
using Manager.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Manager.Util
{
    public class PackageManager : MonoBehaviour
    {
        // ======================= 정적 변수 ======================= //
        public static PackageManager Instance = null;


        #region 패키지 키
        // 서버가 있으면 좋을 값들
        private const string KEY_THEME      = "T-H1E3M?E^I=N&F$O";
        private const string KEY_PACKAGE    = "P=A!C@K%A&G*E@I%N$F^O";

        private const string KEY_ANIBLOCK   = "F$I#R!S&T)C*O_L!L(E^C*&T+I=O?N";
        private const string KEY_RENEWAL    = "R_E!N%E+W?A>L<C>L}L0E8C=T{`I~O^N";
        private const string KEY_PPORORO    = "P@P$O%R*O!R^O=C`O~L>L?E<C[T]IqOcN";

        private const string KEY_ACTIVE     = "P[A>C]K/A^G#E%A#C@T!I?V+E";
        private const string KEY_FALSE      = "P~A@C&K$A=G+E^F$A*L(S]E$@";

        private const string PKCODE_RENEWAL = "RENWPACKON";
        private const string PKCODE_PPORORO = "PPRRPACKON";
        #endregion


        /// <summary>
        /// 패키지 코드 입력 문자열을 확인하는 함수
        /// </summary>
        /// <param name="_code">입력한 코드</param>
        /// <returns>옳바른 코드인지 확인후 결과 반환</returns>
        public bool Set_ComparePackageCode(string _code)
        {
            if (_code.Equals(PKCODE_RENEWAL))
            {
                PlayerPrefs.SetString(Encrypto.Encrypt(KEY_RENEWAL), Encrypto.Encrypt(KEY_ACTIVE));
                return true;
            }
            else if (_code.Equals(PKCODE_PPORORO))
            {
                PlayerPrefs.SetString(Encrypto.Encrypt(KEY_PPORORO), Encrypto.Encrypt(KEY_ACTIVE));
                return true;
            }

            return false;
        }
        /// <summary>
        /// 패키지 키 반환 함수
        /// </summary>
        /// <param name="_INFO">가져올 키의 패키지</param>
        /// <returns>패키지 키</returns>
        public string Get_PackageKey(PACKAGE_INFO _INFO)
        {
            if (_INFO == Manager.Util.PackageManager.PACKAGE_INFO.PPORORO)
                return Encrypto.Encrypt(KEY_PPORORO);
            else if (_INFO == Manager.Util.PackageManager.PACKAGE_INFO.RENEWEL)
                return Encrypto.Encrypt(KEY_RENEWAL);
            else 
                return Encrypto.Encrypt(KEY_ANIBLOCK);
        }
        /// <summary>
        /// 선택된 패키지가 활성화 가능한지 확인 여부를 반환하는 함수
        /// </summary>
        /// <param name="_INFO">선택할 패키지</param>
        /// <returns>패키지 활성화 가능 여부</returns>
        public bool Get_IsPackageActive(PACKAGE_INFO _INFO)
        {
            switch (_INFO)
            {
                case PACKAGE_INFO.FIRST:
                    return true;

                case PACKAGE_INFO.RENEWEL:
                    {
                            // 키가 존재하지 않으면 false로 생성
                        if (!PlayerPrefs.HasKey(Encrypto.Encrypt(KEY_RENEWAL)))
                        {
                            PlayerPrefs.SetString(Encrypto.Encrypt(KEY_RENEWAL), Encrypto.Encrypt(KEY_FALSE));
                            return false;
                        }
                        else
                        {
                            // 허락 키가 있으면 true
                            if (PlayerPrefs.GetString(Encrypto.Encrypt(KEY_RENEWAL)) == Encrypto.Encrypt(KEY_ACTIVE))
                                return true;
                            // 허락 키가 없으면 false
                            else
                                return false;
                        }
                    }

                case PACKAGE_INFO.PPORORO:
                    {
                            // 키가 존재하지 않으면 false로 생성
                        if (!PlayerPrefs.HasKey(Encrypto.Encrypt(KEY_PPORORO)))
                        {
                            PlayerPrefs.SetString(Encrypto.Encrypt(KEY_PPORORO), Encrypto.Encrypt(KEY_FALSE));
                            return false;
                        }
                        else
                        {
                            // 허락 키가 있으면 true
                            if (PlayerPrefs.GetString(Encrypto.Encrypt(KEY_PPORORO)) == Encrypto.Encrypt(KEY_ACTIVE))
                                return true;
                            // 허락 키가 없으면 false
                            else
                                return false;
                        }
                    }

                case PACKAGE_INFO.NumOf:
                default:
                    return false;
            }
        }


        #region 패키지 변수
        //1. 흰검, 2. 빨갈, 3. 주초, 4. 노파
        public enum FirstCollection
        {
            //흑백
            WB_Cup,
            WB_Scissors,
            WB_Sheep,
            WB_Bag,
            WB_Pony,
            WB_Key,
            WB_Hat,
            WB_Cat,
            WB_Penguin,
            WB_Rabbit,
            WB_Spaceship,
            WB_Whale,

            //빨갈
            RB_Bird,
            RB_Camel,
            RB_Chameleon,
            RB_Cherry,
            RB_Deer,
            RB_Dog,
            RB_Eagle,
            RB_Giftbox,
            RB_Heart,
            RB_Kangaroo,
            RB_Ship,
            RB_Strawberry,

            //주초
            OG_Car,
            OG_Crocodile,
            OG_Dinosaur,
            OG_Tropicalfish,
            OG_Hippopotamus,
            OG_Lamp,
            OG_Mantis,
            OG_Scarecrow,
            OG_Seahorse,
            OG_Snake,
            OG_Flower,
            OG_Turtle,

            //노파
            YB_Airplane,
            YB_Banana,
            YB_Crab,
            YB_Elephant,
            YB_Mouse,
            YB_Octopus,
            YB_Rhinoceros,
            YB_Scorpion,
            YB_Smile,
            YB_Snail,
            YB_Stagbeetle,
            YB_Umbrella,

            //ECT
            Plus_Apple,
            Plus_Corn,
            Plus_Lion,
            Plus_Tree,

            NumOf,
        }
        public enum RenewelCollection
        {
            // 8x8
            RE1_Larva,
            RE2_Giraffe,
            RE3_Jet,
            RE4_Pteranodon,
            RE5_Plesiosaurus,
            RE6_Question,
            RE7_Bell,
            RE8_Triceratops,
            RE9_Whale1,
            RE10_Hat,
            RE11_Balloon,
            RE12_Duck,
            RE13_Brachiosaurus,
            RE14_Pineapple,
            RE15_Carrot,
            RE16_Turtle,
            RE17_Spinosaurus,
            RE18_Unicorn,
            RE19_Stegosaurus,
            RE20_Plane,
            RE21_Lambeosaurus,
            RE22_Parrot1,
            RE23_Cat,
            RE24_Cactus,
            RE25_Icecream,
            RE26_Parasaurolophus,
            RE27_Mammoth,
            RE28_Frog,
            RE29_Melon,
            RE30_Bee,
            RE31_Ankylosaurus,
            RE32_Chicken,
            RE33_Strawberry,
            RE34_Egg,
            RE35_Bouquet,
            RE36_Octopus,
            RE37_Bull,
            RE38_Bag,
            RE39_Trophy,
            RE40_Pinwheel,
            RE41_Cowboy,
            RE42_Squirrel,

            // 16x8
            RE43_Dinosaur_16x8,
            RE44_Crocodile_16x8,
            RE45_Turtle_16x8,
            RE46_Whale_16x8,
            RE47_Lion_16x8,
            RE48_Helicopter_16x8,
            RE49_Snail_16x8,
            RE50_Sunflower_8x16,

            // 16x16
            RE51_Submarine_16x16,
            RE52_Parrot_16x16,
            RE53_ABalloon_16x16,

            NumOf,
        }
        public enum PpororoCollection
        {
            PR1_CarrotA,
            PR2_BirdA,
            PR3_BoatA,
            PR4_DinosaurA,
            PR5_CupA,
            PR6_CornA,
            PR7_SpaceShipA,
            PR8_SquirtGunA,
            PR9_TurtleA,
            PR10_ClementineA,
            PR11_HatA,
            PR12_UmbrellaA,
            PR13_FishA,
            PR14_GlassesA,
            PR15_BlueBirdA,
            PR16_HorseA,
            PR17_CatA,
            PR18_ElephantA,
            PR19_MoonA,
            PR20_DogA,
            PR21_PersimmonA,
            PR22_HelicopterA,
            PR23_WhaleA,
            PR24_GoatA,
            PR25_LionA,
            PR26_ButterflyA,
            PR27_ClockA,
            PR28_TreeA,
            PR29_DuckA,
            PR30_RobotA,

            NumOf,
        }

        public enum PACKAGE_INFO
        {
            FIRST = 0,
            RENEWEL,
            PPORORO,

            NumOf
        }

        //#3. 도안 크기 정보 정의
        public enum MARKER_SIZE
        {
            _8x8,
            _8x16,
            _16x8,
            _16x16,

            NumOf
        }

        // ======================= 동적 변수 ======================= //
        [Header("====== Package Info ======")]
        [SerializeField] private PACKAGE_INFO PACKAGE;  // 현재 패키지 정보 변수
        [SerializeField] private PACKAGE_INFO THEME;    // 현재 테마 정보 변수
        public PACKAGE_INFO CURRENT_PACKAGE_INFO
        {
            get { return PACKAGE; }
        } // 패키지 Enum
        public PACKAGE_INFO CURRENT_THEME_INFO
        {
            get { return THEME; }
        } // 테마 Enum

        public FirstCollection FC_PACKAGE = FirstCollection.NumOf;      // 퍼스트 패키지에 현재 인식된 도안 정보
        public RenewelCollection RE_PACKAGE = RenewelCollection.NumOf;  // 리뉴얼 패키지에 현재 인식된 도안 정보
        public PpororoCollection PR_PACKAGE = PpororoCollection.NumOf;  // 뽀로로 패키지에 현재 인식된 도안 정보


        [SerializeField] private GameObject[] PackageArray; // 각 패키지별 트래커 오브젝트 배열
        private GameObject OBJ_CurrentPackage = null;       // 현재 패키지 트래커 오브젝트 링크 변수
        private bool b_isOnScene = false;                   // 트래커가 DontDestroy 상태인지/아닌지 정하는 변수

        #endregion

        #region 현재 인식된 도안 정보
        [Header("====== 현재 인식 도안 정보 ======")]
        public int CurrentImage = -1;
        private string[] s_3D = null;       // 3D 마커 이미지 에셋 주소를 가지는 string 배열
        private string[] s_Circle = null;   // 동그라미 마커 이미지 에셋 주소를 가지는 string 배열
        private string[] s_Pattern = null;  // 도안 마커 이미지 에셋 주소를 가지는 string 배열
        private string[] s_Character = null;// 캐릭터 모델 에셋 주소를 가지는 string 배열
        public string[] s_3DMarkerAddress { get { return s_3D; } }              // 3D 마커 이미지 에셋 주소를 반환하는 변수
        public string[] s_CircleMarkerAddress { get { return s_Circle; } }      // 동그란 마커 이미지 에셋 주소를 반환하는 변수
        public string[] s_PatternMarkerAddress { get { return s_Pattern; } }    // 도안 이미지 에셋 주소를 반환하는 변수


        public MARKER_SIZE CURRENT_MARKER_SIZE = MARKER_SIZE._8x8; // 현재 도안의 크기를 저장하는 변수


        public struct STRUCT_MarkerSprites
        {
            public Sprite SPT_3D;
            public Sprite SPT_Circle;
            public Sprite SPT_Pattern;
        }
        public STRUCT_MarkerSprites STRUCT_CurrentMarkerSpritesInfo = new STRUCT_MarkerSprites();


        #endregion

        #region 미니게임 변수
        private int i_MaxMarker = 0; // 미니게임에 성공한 도안의 개수를 저장하는 변수
        /// <summary>
        /// 미니게임을 성공한 도안의 개수를 확인하는 함수
        /// </summary>
        public void Set_AccessableMarkerCount()
        {
            i_MaxMarker = 0;

            switch (PACKAGE)
            {
                case PACKAGE_INFO.FIRST:
                    // 모든 도안의 도안답 데이터 확인
                    for (int i = 0; i < (int)FirstCollection.NumOf; i++)
                    {
                        string enc_name = (FirstCollection.WB_Cup + i) + "_Key";

                        if (PlayerPrefs.HasKey(enc_name) &&
                            Encrypto.Decrypt(PlayerPrefs.GetString(enc_name)) == "Yesdata_" + (FirstCollection.WB_Cup + i))
                        {
                            i_MaxMarker++;
                            continue;
                        }

                        PlayerPrefs.SetString(enc_name, Encrypto.Encrypt("Nodata_" + (FirstCollection.WB_Cup + i)));
                    }

                    break;

                case PACKAGE_INFO.RENEWEL:
                    // 모든 도안의 도안답 데이터 확인
                    for (int i = 0; i < (int)RenewelCollection.NumOf; i++)
                    {
                        string enc_name = (RenewelCollection.RE1_Larva + i) + "_Key";

                        if (PlayerPrefs.HasKey(enc_name) &&
                            Encrypto.Decrypt(PlayerPrefs.GetString(enc_name)) == "Yesdata_" + (RenewelCollection.RE1_Larva + i))
                        {
                            i_MaxMarker++;
                            continue;
                        }

                        PlayerPrefs.SetString(enc_name, Encrypto.Encrypt("Nodata_" + (RenewelCollection.RE1_Larva + i)));
                    }

                    break;

                case PACKAGE_INFO.PPORORO:
                    // 모든 도안의 도안답 데이터 확인
                    for (int i = 0; i < (int)PpororoCollection.NumOf; i++)
                    {
                        string enc_name = (PpororoCollection.PR1_CarrotA + i) + "_Key";

                        if (PlayerPrefs.HasKey(enc_name) &&
                            Encrypto.Decrypt(PlayerPrefs.GetString(enc_name)) == "Yesdata_" + (PpororoCollection.PR1_CarrotA + i))
                        {
                            i_MaxMarker++;
                            continue;
                        }

                        PlayerPrefs.SetString(enc_name, Encrypto.Encrypt("Nodata_" + (PpororoCollection.PR1_CarrotA + i)));
                    }

                    break;

                default:
                    Debug.LogError("PackageManager (AccessableMarkerCount) :: No Package Info found");
                    break;
            }
        }
        /// <summary>
        /// 미니게임을 성곤한 도안의 개수를 반환하는 함수
        /// </summary>
        /// <returns></returns>
        public int Get_AccessableMarkerCount()
        {
            return (i_MaxMarker > 0) ? i_MaxMarker : 1;
        }
        #endregion


        // ======================= 초기화 ======================= //

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
                FC_PACKAGE = FirstCollection.NumOf;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            // 테마 정보 초기화
            {
                // 테마 정보 확인 및 설정
                if (!PlayerPrefs.HasKey(Encrypto.Encrypt(KEY_THEME)))
                {
                    PlayerPrefs.SetString(Encrypto.Encrypt(KEY_THEME), Encrypto.Encrypt(KEY_ANIBLOCK));
                }
                // 테마 정보 초기화
                Set_ChangeTheme(PlayerPrefs.GetString(Encrypto.Encrypt(KEY_THEME)));
            }

            // 패키지 정보 초기화
            {
                // 패키지 정보 확인 및 설정
                if (!PlayerPrefs.HasKey(Encrypto.Encrypt(KEY_PACKAGE)))
                {
                    PlayerPrefs.SetString(Encrypto.Encrypt(KEY_PACKAGE), Encrypto.Encrypt(KEY_ANIBLOCK));
                }
                // 패키지 정보 초기화
                //Debug.LogError(PlayerPrefs.GetString(Encrypto.Encrypt(KEY_PACKAGE)));
                Set_ChangePackage(PlayerPrefs.GetString(Encrypto.Encrypt(KEY_PACKAGE)));
            }

            // 패키지 정보 확인
            {
                if (!PlayerPrefs.HasKey(Encrypto.Encrypt(KEY_RENEWAL)))
                {
                    PlayerPrefs.SetString(Encrypto.Encrypt(KEY_RENEWAL), Encrypto.Encrypt(KEY_FALSE));
                }

                if (!PlayerPrefs.HasKey(Encrypto.Encrypt(KEY_PPORORO)))
                {
                    PlayerPrefs.SetString(Encrypto.Encrypt(KEY_PPORORO), Encrypto.Encrypt(KEY_FALSE));
                }
            }
        }

        // ======================= 패키지 변경 함수 ======================= //
        /// <summary>
            /// 테마 설정하는 함수
        /// </summary>
        /// <param name="_INFO"></param>
        public void Set_ChangeTheme(string _input)
        {
            string pass = Encrypto.Decrypt(_input);
            switch (pass)
            {
                case KEY_ANIBLOCK:
                    THEME = PACKAGE_INFO.FIRST;
                    PlayerPrefs.SetString(Encrypto.Encrypt(KEY_THEME), Encrypto.Encrypt(KEY_ANIBLOCK));
                    break;
                case KEY_RENEWAL:
                    THEME = PACKAGE_INFO.RENEWEL;
                    PlayerPrefs.SetString(Encrypto.Encrypt(KEY_THEME), Encrypto.Encrypt(KEY_RENEWAL));
                    break;
                case KEY_PPORORO:
                    THEME = PACKAGE_INFO.PPORORO;
                    PlayerPrefs.SetString(Encrypto.Encrypt(KEY_THEME), Encrypto.Encrypt(KEY_PPORORO));
                    break;

                default:
                    Debug.LogError("PACKAGEMANAGER :: Wrong string input");
                    return;
            }
        }

        /// <summary>
        /// 패키지를 변경하기 위해 사용되는 함수
        /// </summary>
        public void Set_ChangePackage(string _input)
        {
            string pass = Encrypto.Decrypt(_input);
            switch (pass)
            {
                case KEY_ANIBLOCK:
                    Set_Package(PACKAGE_INFO.FIRST);
                    break;
                case KEY_RENEWAL:
                    Set_Package(PACKAGE_INFO.RENEWEL);
                    break;
                case KEY_PPORORO:
                    Set_Package(PACKAGE_INFO.PPORORO);
                    break;
                default:
                    Debug.LogError("PACKAGEMANAGER :: Wrong string input");
                    return;
            }
        }

        private void Set_Package(PACKAGE_INFO _INFO)
        {
            // 패키지 정보가 없으면 반환
            if (PackageArray.Length < 1)
            {
                Debug.LogError("There is no registered package info");
                return;
            }

            // 등록되어 있지 않은 정보 호출시 반환
            if ((int)_INFO > PackageArray.Length)
            {
                Debug.LogError("Input is out of range :: " + _INFO + ", " + (int)_INFO);
                return;
            }

            // 현재 패키지 오브젝트가 존재하면 제거
            if (OBJ_CurrentPackage != null)
            {
                SceneManager.MoveGameObjectToScene(OBJ_CurrentPackage, SceneManager.GetActiveScene());
                Destroy(OBJ_CurrentPackage);
            }

            // 트래커들 제거
            CustomARCameraManager.Instance.DisableTrackers();

            // 새 오브젝트 정보 저장 및 새 트래커 오브젝트 생성
            string password = "";
            switch (_INFO)
            {
                case PACKAGE_INFO.FIRST:
                    password = Encrypto.Encrypt(KEY_ANIBLOCK);
                    break;
                case PACKAGE_INFO.RENEWEL:
                    password = Encrypto.Encrypt(KEY_RENEWAL);
                    break;
                case PACKAGE_INFO.PPORORO:
                    password = Encrypto.Encrypt(KEY_PPORORO);
                    break;
            }
            PlayerPrefs.SetString(Encrypto.Encrypt(KEY_PACKAGE), password);
            OBJ_CurrentPackage = Instantiate(PackageArray[(int)_INFO]);
            DontDestroyOnLoad(OBJ_CurrentPackage);

            // 마커 이미지 정보 변경
            PACKAGE = _INFO;

            // 각 패키지마다 정해진 세팅으로 변경
            Set_Tracker();
            Set_AccessableMarkerCount();
        }

        /// <summary>
        /// 패키지를 변경하며 동시에 변경되어야하는 값들을 변경하는 함수
        /// 1. 마커 이미지 배열 변경
        /// 2. 도안 데이터 초기화
        /// </summary>
        private void Set_Tracker()
        {
            // 마커 이미지 리스트 제거
            s_3D = null;
            s_Circle = null;
            s_Pattern = null;

            switch (PACKAGE)
            {
                case PACKAGE_INFO.FIRST:
                    {
                        // 마커 이미지 리스트 새로 편성
                        s_3D = new string[(int)FirstCollection.NumOf];
                        s_Circle = new string[(int)FirstCollection.NumOf];
                        s_Pattern = new string[(int)FirstCollection.NumOf];
                        s_Character = new string[(int)FirstCollection.NumOf];

                        // 마커 이미지를 순서대로 입력
                        for (int i = 0; i < (int)FirstCollection.NumOf; i++)
                        {
                            s_3D[i] = "Assets/Unsign_Resources/Markers/3DModelMarker/3D_FirstCollection/3D_" + i + "_" + (FirstCollection.WB_Cup + i) + ".png";
                            s_Circle[i] = "Assets/Unsign_Resources/Markers/CircleMarkerPatterns/CircleMarker_First/CC_" + (FirstCollection.WB_Cup + i) + ".png";
                            s_Pattern[i] = "Assets/Unsign_Resources/Markers/MarkerPattern/Marker_FirstCollection/PT_" + (FirstCollection.WB_Cup + i) + ".png";
                        }

                        // 마지막으로 인식된 해당 패키지의 정보를 설정
                        if (FC_PACKAGE != FirstCollection.NumOf)
                            XmlManager.S.LoadMapData(FC_PACKAGE.ToString());

                        break;
                    }

                case PACKAGE_INFO.RENEWEL:
                    {
                        // 마커 이미지 리스트 새로 편성
                        s_3D = new string[(int)RenewelCollection.NumOf];
                        s_Circle = new string[(int)RenewelCollection.NumOf];
                        s_Pattern = new string[(int)RenewelCollection.NumOf];
                        s_Character = new string[(int)RenewelCollection.NumOf];

                        // 마커 이미지를 순서대로 입력
                        for (int i = 0; i < (int)RenewelCollection.NumOf; i++)
                        {
                            s_3D[i] = "Assets/Unsign_Resources/Markers/3DModelMarker/3D_Renewel/3D_" + (RenewelCollection.RE1_Larva + i) + ".png";
                            s_Circle[i] = "Assets/Unsign_Resources/Markers/CircleMarkerPatterns/CircleMarker_Renewal/CC_" + (RenewelCollection.RE1_Larva + i) + ".png";
                            s_Pattern[i] = "Assets/Unsign_Resources/Markers/MarkerPattern/Marker_RenewalCollection/PT_" + (RenewelCollection.RE1_Larva + i) + ".png";
                        }

                        // 마지막으로 인식된 해당 패키지의 정보를 설정
                        if (RE_PACKAGE != RenewelCollection.NumOf)
                            XmlManager.S.LoadMapData(RE_PACKAGE.ToString());

                        break;
                    }

                case PACKAGE_INFO.PPORORO:
                    {
                        // 마커 이미지 리스트 새로 편성
                        s_3D = new string[(int)PpororoCollection.NumOf];
                        s_Circle = new string[(int)PpororoCollection.NumOf];
                        s_Pattern = new string[(int)PpororoCollection.NumOf];
                        s_Character = new string[1];

                        // 마커 이미지를 순서대로 입력
                        for (int i = 0; i < (int)PpororoCollection.NumOf; i++)
                        {
                            s_3D[i] = "Assets/Unsign_Resources/Markers/3DModelMarker/3D_Ppororo/3D_" + (PpororoCollection.PR1_CarrotA + i) + ".png";
                            s_Circle[i] = "Assets/Unsign_Resources/Markers/CircleMarkerPatterns/CircleMarker_Ppororo/CC_" + (PpororoCollection.PR1_CarrotA + i) + ".png";
                            s_Pattern[i] = "Assets/Unsign_Resources/Markers/MarkerPattern/Marker_PpororoCollection/PT_" + (PpororoCollection.PR1_CarrotA + i) + ".png";
                        }

                        s_Character[0] = "Assets/Unsign_Resources/3D_Models/3D_LiveGame/LiveGameModels/Ppororo/Ppororo_Model.prefab";

                        // 마지막으로 인식된 해당 패키지의 정보를 설정
                        if (PR_PACKAGE != PpororoCollection.NumOf)
                            XmlManager.S.LoadMapData(PR_PACKAGE.ToString());

                        break;
                    }

                default:
                    Debug.LogError("No Package info found :: The CURRENT_PACKAGE_INFO is not set");
                    break;
            }
        }

        /// <summary>
        /// 트래킹 오브젝트를 해당 씬으로 넘기거나 DontDestroy로 변경하는 함수.
        ///  * 인식한 이미지 위에 오브젝트를 올리기 위해서는 트래킹 오브젝트를 해당 씬으로 옮겨야한다. *
        /// </summary>
        /// <param name="_setScene">True : 씬으로 옮기기, False : DontDestroy로 옮기기</param>
        /// <param name="_sceneName">지정된 씬으로 옮기기</param>
        public void Set_TrackerSceneLoad(bool _setScene, string _sceneName = "Scene_Guide")
        {
            // 동일한 작업을 하는 것을 방지하는 예외처리
            if (b_isOnScene == _setScene) return;

            // 씬으로 넣을 것인지 DontDestroy를 할 것인지 결정
            b_isOnScene = _setScene;

            if (b_isOnScene)
            {
                // 선택된 씬으로 트래커들을 이동
                SceneManager.MoveGameObjectToScene(OBJ_CurrentPackage, SceneManager.GetSceneByName(_sceneName));
            }
            else
            {
                // 트래커들을 DontDestroy로 설정
                DontDestroyOnLoad(OBJ_CurrentPackage);
            }
        }
        
        /// <summary>
        /// 도안 위 중앙에 위치할 오브젝트를 받아 배치하는 함수
        /// </summary>
        /// <param name="_ARBBlocks">오브젝트 트렌스폼</param>
        public void Set_ARObjectToTrackerObject(Transform _ARBBlocks)
        {
            //Debug.Log(CurrentImage);
            _ARBBlocks.SetParent(OBJ_CurrentPackage.transform.GetChild(CurrentImage).transform);
            _ARBBlocks.localPosition = Vector3.zero;
            _ARBBlocks.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }
}
