using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Manager.Util;

public class AsyncAssetLoadManager : MonoBehaviour
{
    private static AsyncAssetLoadManager Singleton = null;
    public static AsyncAssetLoadManager Instance
    {
        get
        {
            if (Singleton == null) Singleton = new AsyncAssetLoadManager();

            return Singleton;
        }
    }

    [Header("====== 에셋 로드 매니저 변수 ======")]
    [SerializeField] private string s_Address = "";

    [SerializeField] private Object OBJ_Return = null;
    [SerializeField] private Component COM_Return = null;
    [SerializeField] private Sprite SPT_Return = null;
    [SerializeField] private Image IMG_Return = null;
    [SerializeField] private Image[] IMG_ArrayReturn = null;
    [SerializeField] private SpriteRenderer SR_Return = null;
    [SerializeField] private SpriteRenderer[] SR_ArrayReturn = null;



    private void Start()
    {
        Singleton = this;
        DontDestroyOnLoad(this);
    }

    /// <summary>
    /// 에셋 리소스 어드레스 번들 로드 함수
    /// </summary>
    /// <param name="_path">에셋 위치</param>
    /// <param name="_type">반환될 오브젝트 타입 (typeof사용)</param>
    /// <returns></returns>
    public void Get_LoadAssetSprite(string _path, ref Image _object)
    {
        IMG_Return = _object;

        Addressables.LoadAssetAsync<Sprite>(_path).Completed += Get_LoadAssetImage;
    }
    public void Get_LoadAssetSprite(string _path, ref SpriteRenderer _sprite)
    {
        SR_Return = _sprite;

        Addressables.LoadAssetAsync<Sprite>(_path).Completed += Get_LoadAssetSpriteRenderer;
    }
    public async void Set_LoadAssetMarkerSprites(string _pattern, string _circle, string _3d)
    {
        PackageManager.Instance.STRUCT_CurrentMarkerSpritesInfo.SPT_Pattern = await Addressables.LoadAssetAsync<Sprite>(_pattern).Task;
        PackageManager.Instance.STRUCT_CurrentMarkerSpritesInfo.SPT_Circle = await Addressables.LoadAssetAsync<Sprite>(_circle).Task;
        PackageManager.Instance.STRUCT_CurrentMarkerSpritesInfo.SPT_3D = await Addressables.LoadAssetAsync<Sprite>(_3d).Task;
    }


    private void Get_LoadComponent<T>(AsyncOperationHandle<T> obj) where T : UnityEngine.Component
    {
        switch (obj.Status)
        {
            case AsyncOperationStatus.Succeeded:
                OBJ_Return = obj.Result;
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
    private void Get_LoadAssetImage(AsyncOperationHandle<Sprite> obj)
    {
        switch (obj.Status)
        {
            case AsyncOperationStatus.Succeeded:
                IMG_Return.sprite = obj.Result;
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
    private void Get_LoadAssetSpriteRenderer(AsyncOperationHandle<Sprite> obj)
    {
        switch (obj.Status)
        {
            case AsyncOperationStatus.Succeeded:
                SR_Return.sprite = obj.Result;
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
}
