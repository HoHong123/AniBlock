using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.UnityUtils.Helper;
using Manager.Util;

namespace Manager
{
    public class RescueManager : MonoBehaviour
    {
        private const int SUCCESS_RATE = 70;

        private string GetAccessPasswordToMinigame()
        {
            return Encrypto.Encrypt("ACCESS#@ COMPLETE&%" + _PackageManager.FC_PACKAGE.ToString() + "_Mini");
        }
        private string GetDenyPasswordToMinigame()
        {
            return Encrypto.Encrypt("ACCESS#@ DENY&%" + _PackageManager.FC_PACKAGE.ToString() + "_Mini");
        }

        private bool b_isScanning = false;
        public bool Get_IsScanning()
        {
            return b_isScanning;
        }


        public static RescueManager S = null;

        private Util.XmlManager _xmlManager = null;
        private Util.PackageManager _PackageManager = null;

        [SerializeField] private WebcamAgent SCRIPT_WebCamAgent = null;

        [Header("====정답 오브젝트====")]
        [SerializeField] private Slider SLID_Wave = null;
        [SerializeField] private Text TXT_AnswerText = null;
        [SerializeField] private Image IMG_Scan = null;
        [SerializeField] private Image IMG_Answer = null;
        [SerializeField] private Image IMG_Marker = null;
        [SerializeField] private Image IMG_Rescue = null;
        [SerializeField] private Image IMG_BTNRecognize = null;
        [SerializeField] private GameObject OBJ_Zoom = null;

        [Header("====리소스====")]
        [SerializeField] private Sprite PNG_Correct = null;
        [SerializeField] private Sprite PNG_Wrong = null;
        [SerializeField] private Sprite PNG_Ready = null;
        [SerializeField] private Sprite SPRITE_Scan = null;
        [SerializeField] private Sprite SPRITE_Skip = null;

        [Header("====버튼====")]
        [SerializeField] private Button BTN_Cheat = null;
        [SerializeField] private Button BTN_Rescue = null;
        private int i_Click = 0;
        private const float f_DEFAULT_WAIT = 1.0f;
        private float f_Wait = 0;

        private bool b_Transparent = true;                      // 마커 이미지 루프 설정
        private Color trim = new Color(0, 0, 0, 0.00333f);      // 마커 이미지 알파값 색상 변경 범위



        private void Awake()
        {
            S = this;
        }

        private void Start()
        {
            _xmlManager = Util.XmlManager.S;
            _PackageManager = Util.PackageManager.Instance;

            Image[] images = { IMG_Marker, IMG_Scan, IMG_BTNRecognize };
            // 구출씬에 기본 도안 이미지 설정
            switch (_PackageManager.CURRENT_PACKAGE_INFO)
            {
                case PackageManager.PACKAGE_INFO.FIRST:
                    // 이미지 세팅
                    _xmlManager.LoadMapData(_PackageManager.FC_PACKAGE.ToString());
                    break;

                case PackageManager.PACKAGE_INFO.RENEWEL:
                    // 이미지 세팅
                    _xmlManager.LoadMapData(_PackageManager.RE_PACKAGE.ToString());
                    break;

                case PackageManager.PACKAGE_INFO.PPORORO:
                    // 이미지 세팅
                    _xmlManager.LoadMapData(_PackageManager.PR_PACKAGE.ToString());
                    break;

                default:
                    Debug.LogError("No Package info found :: The CURRENT_PACKAGE_INFO is not set");
                    break;
            }

            IMG_Marker.sprite = IMG_Scan.sprite = IMG_BTNRecognize.sprite = _PackageManager.STRUCT_CurrentMarkerSpritesInfo.SPT_Pattern;

            BTN_Rescue.onClick.AddListener(() => { SCRIPT_WebCamAgent.ShootAndInference(); });

            WaitForCamera();

            if (LoadMaster.S.GetLoadPanelActive()) LoadMaster.S.SetLoadPanel(false);
        }

        private IEnumerator WaitForCamera()
        {
            while (!CustomARCameraManager.Instance.IsCameraReady())
            {
                yield return new WaitForEndOfFrame();
            }

            OBJ_Zoom.GetComponent<Image>().sprite = PNG_Ready;
        }
        

        private void Update()
        {
            if (f_Wait > 0)
            {
                f_Wait -= Time.deltaTime;
            }
            else
            {
                i_Click = 0;
            }

            // 마커 이미지 투명화 루프
            if (b_Transparent) // target = 0
            {
                IMG_Marker.color -= trim;

                if (IMG_Marker.color.a < 0)
                {
                    IMG_Marker.color = new Color(1, 1, 1, 0);
                    b_Transparent = false;
                }
            }
            else // target = 0.5f
            {
                IMG_Marker.color += trim;

                if (IMG_Marker.color.a > 0.4f)
                {
                    IMG_Marker.color = new Color(1, 1, 1, 0.4f);
                    b_Transparent = true;
                }
            }
        }


        private IEnumerator Scanning(float _rate)
        {
            enabled = false;

            b_isScanning = true;

            OBJ_Zoom.SetActive(false);

            CustomARCameraManager.Instance.DisableCamera();

            BTN_Rescue.interactable = false;
            BTN_Rescue.onClick.RemoveAllListeners();
            BTN_Rescue.onClick.AddListener(() => {
                LoadMaster.S.LoadSceneFunc("Scene_Minigame");
            });

            IMG_Rescue.sprite = SPRITE_Skip;
            IMG_Scan.gameObject.SetActive(true);

            SLID_Wave.gameObject.SetActive(true);

            Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.UI_CLIP_LIST.P_RESCUE_BlockScan);

            float timer = 0;
            // 2초 동안 % 증가
            while ((timer += Time.deltaTime) < 2.0f)
            {
                if (SLID_Wave.value > SLID_Wave.maxValue) break;

                yield return new WaitForEndOfFrame();

                SLID_Wave.value += (Time.deltaTime * 60f);
            }

            IMG_Answer.gameObject.SetActive(true);
            SLID_Wave.gameObject.SetActive(false);

            if (!(_rate < SUCCESS_RATE) ? false : true)
            {
                Sound.SoundManager.S.Play_Sound(Sound.SoundManager.UI_CLIP_LIST.P_RESCUE_ScanFail);
                IMG_Answer.sprite = PNG_Wrong;
            }
            else
            {
                Sound.SoundManager.S.Play_Sound(Sound.SoundManager.UI_CLIP_LIST.P_RESCUE_ScanSuccess);
                IMG_Answer.sprite = PNG_Correct;
            }

            yield return new WaitForSeconds(1.5f);

            //isScanning = false;
            BTN_Rescue.interactable = true;
        }

        public void MinigameCheck(float _rate)
        {
            if (b_isScanning) return;

            StartCoroutine("Scanning", _rate);
        }
    }
}
