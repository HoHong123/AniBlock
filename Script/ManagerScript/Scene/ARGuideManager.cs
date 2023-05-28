using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;

namespace Manager
{
    public class ARGuideManager : MonoBehaviour
    {
        //ARFoundation..
        public ObjectPrefab objectToPlace = null;
        public GameObject placementIndicator = null;
        //public ARSessionOrigin  arOrigin              = null;

        private Pose placementPose;
        private bool placementPoseIsValid = false;

        public GameObject objectPrefab = null;

        private Manager.Util.XmlManager _xmlManager = null;
        private Manager.Util.PackageManager _PackageManager = null;

        private void Start()
        {
            _xmlManager = Manager.Util.XmlManager.S;
            _PackageManager = Util.PackageManager.Instance;

            GuideInit();
            InitPopUp();

#if UNITY_EDITOR         //Test Code..
            objectToPlace = Instantiate(objectPrefab).gameObject.GetComponent<ObjectPrefab>();
            objectToPlace.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            GuideUpdate(0);
#endif
        }

        void Update()
        {
            if (objectToPlace == null)
            {
                UpdatePlacementPose();
                UpdatePlacementIndicator();

                if (placementPoseIsValid && Input.touchCount > 0 &&
                    Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    PlaceObject();
                }
            }
            else
            {
                placementIndicator.SetActive(false);
            }
        }

        private void UpdatePlacementPose()
        {
            var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            //var hits = new List<ARRaycastHit>();

            //arOrigin.Raycast(screenCenter, hits, TrackableType.Planes);

            //placementPoseIsValid = hits.Count > 0;

            if (placementPoseIsValid)
            {
                //placementPose = hits[0].pose;

                var cameraForward = Camera.main.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            }
        }

        private void UpdatePlacementIndicator()
        {
            if (placementPoseIsValid)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            }
            else
            {
                placementIndicator.SetActive(false);
            }
        }

        private void PlaceObject()
        {
            objectToPlace = Instantiate(objectPrefab, placementPose.position, Quaternion.Euler(new Vector3(90.0f, placementPose.rotation.eulerAngles.y, placementPose.rotation.eulerAngles.z))).gameObject.GetComponent<ObjectPrefab>();
            GuideUpdate(0);
        }


        //=========================================================
        //ARGuide

        //3D GUIDE POOL INFO..
        private int overlappedBlockTypeCount = 2; //한 도안에 들어갈수있는 같은종류의 최대 블록종류 갯수

        [System.Serializable]
        public class BlockTypePool
        {
            public Transform _BlockTrans;
            public Material _BlockMat;
            public Material _BlockOutMat;

            public Manager.Util.XmlManager.WAITBLOCK_NUM _BlockType;
        }
        public Color[] BlockColors = null;

        private List<BlockTypePool> BlockTypePool3DList = new List<BlockTypePool>();
        private List<Transform> CurrBlockTypeList = new List<Transform>();

        public GameObject[] BlockType3DPrefabs = null;

        public Transform BlockPool = null; //생성된 블록 풀들을 대기시키는 장소

        private bool isGuideActive = false;

        public void GuideInit()
        {
            BlockTypePool3DList.Clear();

            //Create Pool List...
            for (int i = 0; i < overlappedBlockTypeCount; i++)
            {
                for (int j = 0; j < (int)Manager.Util.XmlManager.WAITBLOCK_NUM.TE7 + 1; j++)
                {
                    GameObject _block = Instantiate(BlockType3DPrefabs[j], BlockPool);

                    BlockTypePool _blockPoolInfo = new BlockTypePool();
                    _blockPoolInfo._BlockTrans = _block.gameObject.GetComponent<Transform>();
                    //_blockPoolInfo._BlockMat = _block.gameObject.GetComponent<CollectRenderer>().myRendererList[0].gameObject.GetComponent<MeshRenderer>().material;
                    _blockPoolInfo._BlockOutMat = _block.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material;

                    _blockPoolInfo._BlockType = Manager.Util.XmlManager.WAITBLOCK_NUM.AM1 + j;
                    _block.gameObject.SetActive(false);

                    BlockTypePool3DList.Add(_blockPoolInfo);
                }
            }
        }

        public void GuideUpdate(int _GuideIndex)
        {
            CurrBlockTypeList.Clear();

            //풀 초기화..
            for (int i = 0; i < BlockTypePool3DList.Count; i++)
            {
                BlockTypePool3DList[i]._BlockTrans.gameObject.SetActive(false);
                BlockTypePool3DList[i]._BlockTrans.gameObject.transform.SetParent(BlockPool);
            }

            //가이드를 읽고 배치
            for (int i = 0; i < _xmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST.Count; i++)
            {
                int _blockPoolIndex = ReturnUnActivePoolItem(_xmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].ID);

                if (_blockPoolIndex == -1)
                {
                    Debug.LogError("3D::GuideUpdate(int _GuideIndex) : 풀이 문제있다. 확인해바라");
                }
                else
                {
                    //색상 쳐발쳐발
                    Color newColor = BlockColors[(int)_xmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].COLOR];
                    newColor.r -= 1.0f;
                    newColor.g -= 1.0f;
                    newColor.b -= 1.0f;
                    newColor = new Color(Mathf.Abs(newColor.r), Mathf.Abs(newColor.g), Mathf.Abs(newColor.b));

                    BlockTypePool3DList[_blockPoolIndex]._BlockMat.color = BlockColors[(int)_xmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].COLOR];
                    BlockTypePool3DList[_blockPoolIndex]._BlockOutMat.color = newColor;

                    //위치 쳐발쳐발 (중심 온)
                    BlockTypePool3DList[_blockPoolIndex]._BlockTrans.gameObject.transform
                        .SetParent(objectToPlace.TileArrary[(int)_xmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].DX,
                                                            (int)_xmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].DY]);

                    BlockTypePool3DList[_blockPoolIndex]._BlockTrans.localPosition = Vector3.zero;

                    //회전 쳐발쳐발
                    BlockTypePool3DList[_blockPoolIndex]._BlockTrans.gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, (-90 * _xmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].ROT) + objectToPlace.gameObject.transform.rotation.eulerAngles.y, 0));

                    //활성온
                    CurrBlockTypeList.Add(BlockTypePool3DList[_blockPoolIndex]._BlockTrans);
                }
            }

            //온오프결정
            if (isGuideActive)
            {
                for (int i = 0; i < CurrBlockTypeList.Count; i++)
                {
                    CurrBlockTypeList[i].gameObject.SetActive(true);
                }
            }
        }

        //비활성 풀을 땡겨줌
        public int ReturnUnActivePoolItem(Manager.Util.XmlManager.WAITBLOCK_NUM _blockType)
        {
            int poolIndex = -1;

            for (int i = 0; i < BlockTypePool3DList.Count; i++)
            {
                if (BlockTypePool3DList[i]._BlockType == _blockType)
                {
                    if (BlockTypePool3DList[i]._BlockTrans.gameObject.activeSelf == false)
                    {
                        poolIndex = i;
                        break;
                    }
                }
            }

            return poolIndex;
        }

        public void ActiveGuide()
        {
            isGuideActive = !isGuideActive;

            //온오프결정
            if (isGuideActive)
            {
                for (int i = 0; i < CurrBlockTypeList.Count; i++)
                {
                    CurrBlockTypeList[i].gameObject.SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < CurrBlockTypeList.Count; i++)
                {
                    CurrBlockTypeList[i].gameObject.SetActive(false);
                }
            }
        }

        //=================================================
        [Header("Guide PopUp")]
        public GameObject PAN_SelectGuide = null;
        public GameObject PAN_SelectGuideContent = null;
        public GameObject BTN_SelectGuideItemPrefab = null;

        public Sprite[] BlockType2DBlockSprites = null;

        public void ActiveSelectGuidePopUp()
        {
            PAN_SelectGuide.gameObject.SetActive(!PAN_SelectGuide.gameObject.activeSelf);
        }

        public void InitPopUp()
        {
            for (int i = 0; i < _xmlManager.ALLGUIDE_LIST.Count; i++)
            {
                GameObject _item = Instantiate(BTN_SelectGuideItemPrefab, PAN_SelectGuideContent.transform);
                _item.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = (i + 1).ToString();

                _item.name = i.ToString();

                for (int j = 0; j < _xmlManager.ALLGUIDE_LIST[i].GUIDE_LIST.Count; j++)
                {
                    if (j == 11) break;

                    _item.transform.GetChild(1).transform.GetChild(j).gameObject.GetComponent<Image>().sprite = BlockType2DBlockSprites[(int)_xmlManager.ALLGUIDE_LIST[i].GUIDE_LIST[j].ID];
                    _item.transform.GetChild(1).transform.GetChild(j).gameObject.GetComponent<Image>().color = BlockColors[(int)_xmlManager.ALLGUIDE_LIST[i].GUIDE_LIST[j].COLOR];
                    _item.transform.GetChild(1).transform.GetChild(j).gameObject.SetActive(true);
                }

                _item.gameObject.GetComponent<Button>().onClick.AddListener(() => {
                    GuideUpdate(int.Parse(_item.name));
                    ActiveSelectGuidePopUp();
                });
            }
        }
    }


}
