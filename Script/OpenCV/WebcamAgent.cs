using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using OpenCVForUnityExample;
using FileIO;
using Manager.Util;
// using OpenCvSharp;


public class WebcamAgent : MonoBehaviour
{
    private string Answer;
    private int percentage;
    private string text;
    private string pathFile;
    private string serverFileName;
    private string id;

    private WebCamTexture wct = null;

    private CvLoader TfPredictor = null;
    private ModelingAgent modelingAgent = null;

    [Space(10), Header("Buttons--------------------------")]
    [SerializeField] private Button BtnShoot = null;
    [SerializeField] private Button BtnMiniGame = null;
    [SerializeField] private Button BtnBack = null;


    private float f_HeightPersent = (1100f / 2560f); // (필요 높이 / 원본 높이) * 100 = 항상 필요한 높이 퍼센트 : 1440 x 2560 기준
    private float f_StartHeight;
    private Vector2 V2_Size;

    private bool b_isRunning = false;

    public Transform zoom;
    ThresholdExample threshold;
    FileIO.OutputJPG.ImageOutput FIO_imageOutput;


    // ************* CV ************** 
    public void Awake()
    {
        FIO_imageOutput = new FileIO.OutputJPG.ImageOutput();
        FIO_imageOutput.Initiation();

        V2_Size = new Vector2();
        V2_Size.y = (Screen.height * f_HeightPersent);
        if (V2_Size.y * 1.5f > Screen.width)
        {
            V2_Size.x = Screen.width;
            V2_Size.y = V2_Size.x * 0.33f * 2;
            f_StartHeight = (Screen.height * 0.5f) - (V2_Size.y * 0.5f);
        }
        else
        {
            V2_Size.x = V2_Size.y * 1.5f;
            f_StartHeight = (Screen.height * 0.5f) - ((Screen.height * f_HeightPersent) * 0.5f); // 전체 높이의 절반 - 필요한 높이의 절반 = 시작해야하는 y포인트
        }
        V2_Size *= 1.1f;

        threshold = new ThresholdExample();
        TfPredictor = new CvLoader();

        if (!PlayerPrefs.HasKey("SAVE_TEXTURE"))    // 세이브 이미지 순서로 설정된 데이터가 없으면 실행
            PlayerPrefs.SetInt("SAVE_TEXTURE", 0);  // 세이브 이미지 순서 설정
    }

    // ------------------ Shoot ----------------------

    public void ShootAndInference()
    {
        if (modelingAgent == null)
        {
            modelingAgent = GetComponent<ModelingAgent>();
            modelingAgent.InitGrid();
        }

        // 이미 스캔이 진행 중이지 않고, 마커가 인식된 상태, 현재 스캔이 진행되지 않고있을때 실행
        if (Manager.RescueManager.S.Get_IsScanning() == false &&
            PackageManager.Instance.FC_PACKAGE != PackageManager.FirstCollection.NumOf &&
            !b_isRunning)
        {
            StartCoroutine("saveImage", true); // 스캐닝 시작
        }
        // 스캐닝을 실행했으면 실행
        else if(Manager.RescueManager.S.Get_IsScanning() == true)
        {
            LoadMaster.S.LoadSceneFunc("Scene_MiniGame"); // 미니게임 씬으로 로딩
        }
    }

    public void ShootAndInferenceColoring()
    {
        if(modelingAgent == null)
        {
            modelingAgent = GetComponent<ModelingAgent>();
            modelingAgent.InitDrawPanel();
        }

        StartCoroutine("saveImage", false);
    }

    // ------------------ Functions ----------------------
    /// <summary>
    /// 알고리즘으로 이미지 분석용 함수
    /// </summary>
    /// <param name="crop">캡쳐화면 중 분석을 해야하는 부분만 잘라낸 Texture2D변수</param>
    private void ActiveAltorighm(ref Texture2D crop)
    {
        // 구출씬에서 함수가 호출되면 실행
        if (PlayerPrefs.GetInt("W_Algorithm") == 0)
        {
            string name = Screen.width + "x" + Screen.height + "_1x1_" + PlayerPrefs.GetInt("SAVE_TEXTURE");
            _CallTensorflow(name + ".jpg", name + ".png");
            _CallModelBuilder(name + ".jpg", name + ".png", true);
        }

        // 컷씬에서 함수가 호출되면 실행
        else
        {
            switch (PackageManager.Instance.CURRENT_PACKAGE_INFO)
            {
                case PackageManager.PACKAGE_INFO.FIRST:
                    threshold.CheckFilled(crop, Manager.Util.XmlManager.S.LoadMarkerData(PackageManager.Instance.FC_PACKAGE.ToString()));
                    break;

                case PackageManager.PACKAGE_INFO.RENEWEL:
                    threshold.CheckFilled(crop, Manager.Util.XmlManager.S.LoadMarkerData(PackageManager.Instance.RE_PACKAGE.ToString()));
                    break;

                case PackageManager.PACKAGE_INFO.PPORORO:
                    threshold.CheckFilled(crop, Manager.Util.XmlManager.S.LoadMarkerData(PackageManager.Instance.PR_PACKAGE.ToString()));
                    break;

                default:
                    Debug.LogError("WebCamAgent(ActiveAltorighm) :: No Package Info is set :: " + PackageManager.Instance.CURRENT_PACKAGE_INFO);
                    return;
            }
        }

        // 텍스쳐를 최대 3개까지 저장해야하기 위해 0~2까지 저장파일 뒤에 이름을 설정하기 위해 해당 값에 대한 설정 변경 후 저장
        PlayerPrefs.SetInt("SAVE_TEXTURE", (PlayerPrefs.GetInt("SAVE_TEXTURE") == 2) ? 0 : PlayerPrefs.GetInt("SAVE_TEXTURE") + 1);
    }

    private void ActiveDrawPanel()
    {
        _CallTensorflow("CutSceneCapture.jpg", "CutSceneCapture.png");
        _CallModelBuilder("CutSceneCapture.jpg", "CutSceneCapture.png", false);
    }

    /// <summary>
    /// 이미지 저장을 위한 IEnumerator 함수
    /// </summary>
    /// <param name="_isAlgorithm">
    /// 알고리즘을 위해 실행하는지, 컷씬을 위해 실행하는지 확인하는 매개변수
    /// </param>
    /// <returns></returns>
    private IEnumerator saveImage(bool _isAlgorithm)
    {
        b_isRunning = true;
        zoom.gameObject.SetActive(false);

        
        yield return new WaitForEndOfFrame();

        string name = (_isAlgorithm) ? Screen.width + "x" + Screen.height + "_1x1_" + PlayerPrefs.GetInt("SAVE_TEXTURE") : "CutSceneCapture";
        Texture2D crop;
        FIO_imageOutput.Capture_1x1_JpgFile(out crop, V2_Size, name, f_StartHeight);

        yield return new WaitForEndOfFrame();


        // Rescue씬에서 사용
        if (_isAlgorithm)
        {
            ActiveAltorighm(ref crop);
        }
        else // Cut씬에서 사용
        {
            ActiveDrawPanel();
        }

        b_isRunning = false;

#if UNITY_ANDROID || UNITY_IOS
        if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
#else
        if (Application.internetReachability != NetworkReachability.NotReachable)
#endif
        {
            //----------------------get ID------------------------------------------

            id = "guest" + PlayerPrefs.GetInt("IDname").ToString();

            //----------------------FileName----------------------------------------

            percentage = PlayerPrefs.GetInt("SAVE_TEXTURE");

            for (int i = 0; i < 3; i++)
            {
                int next = i + percentage;
                if (next > 2) next -= 3;
                text = PlayerPrefs.GetInt("SAVE_TEXTURE_RATE" + next) + "%";
                if (PlayerPrefs.GetInt("SAVE_TEXTURE_RATE" + next) >= 70)
                {
                    Answer = "Correct";
                }
                else
                {
                    Answer = "Wrong";
                }
            }
            Manager.RescueManager TF = new Manager.RescueManager();

            if (!Application.isMobilePlatform)
            {
                serverFileName = " PC/ " + Answer + "_" + text + "_" + id + ".jpg";
            }
            else
            {
                serverFileName = SystemInfo.deviceModel + " _ " + text + "_" + Answer + "_" + id + ".jpg";//SystemInfo.deviceUniqueIdentifier
            }

            //----------------------Server to Image Upload--------------------------

            yield return new WaitForEndOfFrame();

            pathFile = PATH.GetOnResources(name + ".jpg");

            URL.Upload AWSUpload = new URL.Upload(); //upload 스크립트 가져오기(AWS 업로드 스크립트(URL : namespace 이름))
            
            AWSUpload.TestUploadFile(pathFile, serverFileName);
        }
    }
    
    // ------------------ Call ----------------------
    /// <summary>
    /// 이미지 분석을 위해 호출하는 함수
    /// </summary>
    /// <param name="input">분석될 이미지 파일 명칭(.jpg)</param>
    /// <param name="output">분석된 이미지 파일 저장 명칭(.png)</param>
    private void _CallTensorflow(string input, string output)
    {
        Hashtable args = new Hashtable();
        args.Add("inputImg", input);        // unitytest.jpg
        args.Add("outputDepth", output);    // unitytest.png

        if (TfPredictor == null) TfPredictor = new CvLoader();
        TfPredictor.DoInference(args);
    }

    /// <summary>
    /// 분석한 이미지 데이터로 게임내 블록을 생성하는 함수
    /// </summary>
    /// <param name="input">분석될 이미지 파일 명칭(.jpg)</param>
    /// <param name="output">분석된 이미지 파일 명칭(.png)</param>
    /// <param name="algorithm">알고리즘을 위해 실행하는지, 컷씬을 위해 실행하는지 확인하는 매개변수</param>
    private void _CallModelBuilder(string input, string output, bool algorithm)
    {
        Hashtable args = new Hashtable();
        args.Add("inputImg", input);        // unitytest.jpg
        args.Add("outputDepth", output);    // unitytest.png
        args.Add("height", V2_Size.y.ToString());
        args.Add("draw", algorithm);

        if (modelingAgent == null) modelingAgent = GetComponent<ModelingAgent>();
        modelingAgent.SendMessage("BuildModel", args);
    }
}
