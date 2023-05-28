using CutScene.AnimationController;
using System.IO;
using UnityEngine;

namespace CutScene.SceneManager
{
    public class CUT_SceneHandler : MonoBehaviour
    {
        [Header("======= AR =======")]
        [SerializeField] Transform TRAN_InstantTrack = null;


        [Header("======= Panels =======")]
        [SerializeField] GameObject CAN_Main        = null;
        [SerializeField] GameObject CAN_Scan        = null;
        [SerializeField] GameObject CAN_Paint       = null;
        [SerializeField] GameObject CAN_Animation   = null;


        [Header("======= Cameras =======")]
        [SerializeField] GameObject CAM_ScanResult  = null;
        [SerializeField] GameObject CAM_Main  = null;


        [Header("======= Buttons =======")]
        [SerializeField] GameObject BTN_Cacpture    = null;
        [SerializeField] GameObject BTN_Save        = null;

        
        [Header("======= GameObject =======")]
        [SerializeField] GameObject DrawPanel       = null;
        [SerializeField] GameObject CUT_Cup         = null;
        [SerializeField] GameObject Frame           = null;

        [Header("======= Script =======")]
        [SerializeField] CUT_AnimationHandler SCRIPT_AnimationHandler;



        // -------------- Button -----------------
        public void BTN_FindSurface()
        {
            CustomARCameraManager.Instance.FindSurface();
        }

        #region 실행 버튼
        public void BTN_ActivePaint()
        {
            CAN_Main.SetActive(false);
            CAN_Paint.SetActive(true);

            CAM_Main.SetActive(true);
            CustomARCameraManager.Instance.DisableCamera();
        }

        // 캐릭터 색칠 후 캐릭터 애니메이션 장면 호출
        public void BTN_ActivateAnimation()
        {
            CUT_Cup.transform.SetParent(TRAN_InstantTrack);

            CAN_Paint.SetActive(false);             // 페인트 붓 제거
            CAN_Main.SetActive(false);
            CAN_Scan.SetActive(false);              // 스캔 캔버스 제거

            CAN_Animation.SetActive(true);          // 애니메이션 캔버스 활성화
            CUT_Cup.SetActive(true);                // 애니메이션 캐릭터 활성화

            CAM_Main.SetActive(false);
            CustomARCameraManager.Instance.EnableCamera();   // AR 카메라 활성화
        }

        // 화면을 촬영하는 캔버스 호출, 스킵도 가능
        public void BTN_ActivatePhoto()
        {
            CUT_Cup.transform.SetParent(TRAN_InstantTrack);

            CAN_Animation.SetActive(false);         // 애니메이션 캔버스 제거
            CAN_Paint.SetActive(false);            // 페인트 붓 제거
            CUT_Cup.SetActive(false);               // 애니메이션 캐릭터 제거

            CAN_Scan.SetActive(true);               // 스캔 캔버스 활성화

            CAM_Main.SetActive(false);
            CustomARCameraManager.Instance.EnableCamera();   // AR 카메라 활성화
        }

        // 촬영 후 데이터 저장 화면 출력
        // 촬영이 시작되고 해당 프레임동안 화면의 이미지를 저장해서 분석하여 블록을 만들어야하기 때문에
        // CustomARCameraManager로 AR카메라를 해당 함수에서 끄지않는다.
        public void BTN_ActiveScan()
        {
            BTN_Cacpture.SetActive(false);
            BTN_Save.SetActive(true);

            Frame.SetActive(false);

            CAM_Main.SetActive(true);
        }
        #endregion

        #region 백 버튼
        public void BackToMain()
        {
            CAN_Main.SetActive(true);
            CAN_Scan.SetActive(false);
            CAN_Paint.SetActive(false);
            CAN_Animation.SetActive(false);
            
            CUT_Cup.SetActive(false);
            SCRIPT_AnimationHandler.StopAnimation();

            CAM_Main.SetActive(true);
            CustomARCameraManager.Instance.DisableCamera();

            BTN_Save.SetActive(false);
            BTN_Cacpture.SetActive(true);

            Frame.SetActive(true);
        }

        public void BackToAnimation()
        {
            CAN_Scan.SetActive(false);

            CUT_Cup.SetActive(true);
            SCRIPT_AnimationHandler.StopAnimation();

            CustomARCameraManager.Instance.EnableCamera();
        }

        public void BackToCapture()
        {
            BTN_Save.SetActive(false);
            BTN_Cacpture.SetActive(true);

            Frame.SetActive(true);
            CustomARCameraManager.Instance.EnableCamera();   // AR 카메라 활성화
        }

        public void BackToActivatePhoto()
        {
            BTN_ActivatePhoto();

            BTN_Cacpture.SetActive(true);
            BTN_Save.SetActive(false);

            CustomARCameraManager.Instance.EnableCamera();   // AR 카메라 활성화
        }
        #endregion
    }
}