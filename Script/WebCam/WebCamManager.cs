using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using MarkerLessARExample;
using OpenCVForUnity.UnityUtils.Helper;

public class WebCamManager : MonoBehaviour
{
//    public static WebCamManager S = null;

//    [SerializeField] private WebCamTextureToMatHelper WC_Helper;
//    [SerializeField] private WebCamTextureMarkerLessARExample WC_Example;
//    public WebCamTextureMarkerLessARExample Get_Example()
//    {
//        return WC_Example;
//    }

//    [SerializeField] private MeshRenderer MR_CameraMesh = null;
//    public void Set_Mesh(bool _isActive)
//    {
//        MR_CameraMesh.enabled = _isActive;
//    }
//    public Texture Get_CameraTexture()
//    {
//        return GetComponent<MeshRenderer>().material.mainTexture;
//    }

//    private void Start()
//    {
//        if (S != null)
//        {
//            Destroy(this.gameObject);
//        }
//        else
//        {
//            S = this;

//            if (WC_Helper == null) WC_Helper = GetComponent<WebCamTextureToMatHelper>();
//            if (WC_Example == null) WC_Example = GetComponent<WebCamTextureMarkerLessARExample>();

//            MR_CameraMesh = GetComponent<MeshRenderer>();
//            //PauseCamera();

//            DontDestroyOnLoad(this.gameObject);
//        }
//    }

//    public void PlayCamera()
//    {
//        Debug.Log("Camera Play");
//        if (!gameObject.active) gameObject.SetActive(true);

//        WC_Example.Play();
//    }

//    public void StopCamera()
//    {
//        Debug.Log("Camera Stop");
//        WC_Example.Stop();

//        if (gameObject.active) gameObject.SetActive(false);
//    }

//    public void PauseCamera()
//    {
//        Debug.Log("Camera Pause");
//        WC_Example.Pause();

//        if (gameObject.active) gameObject.SetActive(false);
//    }
}
