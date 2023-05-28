using Manager.Sound;
using Manager.Util;
using maxstAR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    /*
     * --- 정답 풀 방식 ---
     * 
     * 정답 풀은 8x8 바둑판 모형의 오브젝트의 집합이다.
     * 블록 정답에 사용되는 2D와 3D 블록 오브젝트들은 Start에서 미리 생성되고 해당 블록을 가져오는 방식으로 사용한다.
     * 블록 정답을 설정할때, 정답으로 표시될 블록들은 회전 및 색상을 배정받고 이동되어야할 좌표에 존재하는 정답 풀의 자식 오브젝트에 귀속된다.
     * 귀속된 블록은 로컬 좌표를 Vector.zero로 초기화하여 따로 좌표설정을 하지 않고 블록들을 움직인다.
     * 이는, 초기 기획 단계에서 자주 변화를 겪으며 해상도나 풀의 크기를 변경하는 등 여러 조건에서 Position값을 넘기는 것보다 안전하고 쉽기에 정해진 방식이다.
     * 
     * PS.  2020.06월에 추가된 AR도안의 3D 블록들도 똑같은 기법을 사용했다.
     * PS2. 풀의 64개 오브젝트를 보기 편하려고 콜라이더를 넣은 경우가 있는데, 빌드시 반드시 제거할 것을 권고 (사실 한번 올바르게 설정하면 콜라이더가 필요 없다)
     * 
     * ---------------------
     * 
     * --- 주의점 ---
     * DX와 DY라는 좌표는 블록이 도안에 놓여야할 블록 위치이며 왼쪽 상단에서부터 시작한다.
     * 단, 주의할 점은 이전에 작성하던 분이 DX를 y좌표로, DY를 x좌표로 설정해버렸다. 그래서 그 둘을 반대로 생각해야한다.
     * 그래서 DX(4), DY(5)면 5행(DX)에 4열(DY)부분이 목표 좌표인거다.......
     * 
     * PS. 아직 안고친것도 ㄹㅈㄷ
     */

    public class GuideManager : MonoBehaviour, RecognizeMarker
    {
        // =======================  정적 변수 ======================= //
        static private GuideManager S = null;
        static public GuideManager Instance
        {
            get {
                if(S == null)
                {
                    S = new GuideManager();
                }
                return S; 
            }
        }

        // =======================  동적 변수 ======================= //

        private Manager.Util.XmlManager _XmlManager = null;
        private Manager.Util.PackageManager _PackageManager = null;

        #region 게임씬 레퍼런스 변수
        [System.Serializable] public struct Pattern_Info
        {
            public string s_Name;
            public Image IMG_MakerImg;
            public Transform TRAN_Pattern;
        }
        [Header("====== Game Scene ======")]
        [Tooltip("GuideMap : 8x8, 8x16, 16x8, 16x16")]
        // 도안 이미지를 보여주는 2D 이미지 오브젝트 및 정답 도형을 출력할 피봇 위치 모음
        [SerializeField] private List<Pattern_Info> _Pattern_Info = new List<Pattern_Info>(4);
        [Tooltip("각 Pattern오브젝트들 속 GuideMap의 GridLayoutGroup 컴포먼트의 CellSize 순서대로 0.01을 곱하여 입력")]
        [SerializeField] private float[] i_PatternCellSize = new float[4]; // 크기가 다른 도안들에 2D 블록을 넣기 위해선 크기를 재조정해야하는데 이때 재조정되어야 하는 값은 각 도안의 
        [SerializeField] private Image IMG_SelectPattern = null;
        [SerializeField] private Text TXT_VirtualPanelText = null;
        [SerializeField] private Text TXT_ChallengeNumber = null;
        [SerializeField] private Sprite SPT_DefaultMakerImg = null;

        private int i_TargetPattern2DGuideMaps = -1;

        //2D GUIDE POOL INFO..
        private int overlappedBlockTypeCount = 6;   //한 도안에 들어갈수있는 같은종류의 최대 블록종류 갯수

        #endregion

        #region 정답 블록 오브젝트 변수
        [Header("====== Static Block Info ======")]
        [SerializeField] private Color[] BlockColors = null;
        [SerializeField] private Sprite[] BlockType2DOutlineSprites = null;         // 도안 라인 스프라이트
        [SerializeField] private Sprite[] BlockType2DBlockSprites = null;           // 도안 블록 스프라이트
        [SerializeField] private GameObject[] BlockType3DBlock = null;              // 도안 블록 오브젝트
        [SerializeField] private List<BlockTypePool> BlockTypePool2DList = new List<BlockTypePool>();
        [System.Serializable] public class BlockTypePool
        {
            public RectTransform _BlockTrans;
            public Image _BlockImage;

            public Vector3 oriRotValue = Vector3.zero;

            public Manager.Util.XmlManager.WAITBLOCK_NUM _BlockType;
        }

        private List<Transform> CurrBlockTypeList = new List<Transform>();

        [SerializeField] private Transform BlockPool = null; //생성된 블록 풀들을 대기시키는 장소
        [SerializeField] private GameObject BlockPoolPrefab = null;

        [SerializeField] private GridLayoutGroup BlockTypeViewGridLayoutGroup = null;
        private List<Image> BlockTypeViewGridLayoutGroupList = new List<Image>();

        private int i_PatternWidth = 8;
        private bool isGuideActive = false;
        #endregion

        // ======================= 초기화 ======================= //
        private void Awake()
        {
            if(S == null)
                S = this;
        }

        private void Start()
        {
            _XmlManager = Manager.Util.XmlManager.S;
            _PackageManager = PackageManager.Instance;

            GUIDE_InitAR();
            GUIDE_Init();
            GUIDE_InitAnswerPopup();


            if (LoadMaster.S.GetLoadPanelActive()) LoadMaster.S.SetLoadPanel(false);
        }

        // ======================= 도안 설정 ======================= //

        #region 정답 블록 설정 및 배치
        private void GUIDE_Init()
        {
            BlockTypeViewGridLayoutGroupList.Clear();

            for (int i = 0; i < BlockTypeViewGridLayoutGroup.transform.childCount; i++)
            {
                BlockTypeViewGridLayoutGroupList.Add(BlockTypeViewGridLayoutGroup.transform.GetChild(i).gameObject.GetComponent<Image>());
                BlockTypeViewGridLayoutGroup.transform.GetChild(i).gameObject.SetActive(false);
            }

            BlockTypePool2DList.Clear();

            // 도안 크기별 도안 목표 설정
            SetMapSize();

            // Create Pool List...
            for (int i = 0; i < overlappedBlockTypeCount; i++)
            {
                for (int j = 0; j < (int)Manager.Util.XmlManager.WAITBLOCK_NUM.TE7 + 1; j++)
                {
                    // 새 캔버스 이미지(블록) 오브젝트 생성
                    GameObject _block = Instantiate(BlockPoolPrefab, BlockPool);

                    // 새 블록 정보 선언
                    BlockTypePool _blockPoolInfo = new BlockTypePool();
                    _blockPoolInfo._BlockTrans = _block.gameObject.GetComponent<RectTransform>();
                    _blockPoolInfo._BlockImage = _block.gameObject.GetComponent<Image>();

                    // 블록 스프라이트 이미지 변경
                    _blockPoolInfo._BlockImage.sprite = BlockType2DOutlineSprites[(int)Manager.Util.XmlManager.WAITBLOCK_NUM.AM1 + j];
                    // 블록 타입 설정
                    _blockPoolInfo._BlockType = Manager.Util.XmlManager.WAITBLOCK_NUM.AM1 + j;

                    // 블록 타입별 피봇 및 회전 값 초기화
                    switch (_blockPoolInfo._BlockType)
                    {
                        case Manager.Util.XmlManager.WAITBLOCK_NUM.AM1:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.495f, 0.505f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.D1:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.6f, 0.5f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.TR1:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.3f, 0.5f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.TR2:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.6f, 0.39f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.TE1:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.8f, 0.5f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.TE2:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.285f, 0.6f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.TE3:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.715f, 0.6f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.TE4:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.6f, 0.59f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.TE5:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.7f, 0.6f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.TE6:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.5f, 0.6f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                            break;

                        case Manager.Util.XmlManager.WAITBLOCK_NUM.TE7:
                            _blockPoolInfo._BlockTrans.pivot = new Vector2(0.29f, 0.61f);
                            _blockPoolInfo._BlockTrans.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                            break;
                    }

                    _blockPoolInfo.oriRotValue = _blockPoolInfo._BlockTrans.rotation.eulerAngles;
                    _block.gameObject.SetActive(false);

                    BlockTypePool2DList.Add(_blockPoolInfo);
                }
            }

            // 인식된 도안의 여러 정답 중 첫번째 정답으로 초기화
            GuideUpdate(0);
        }

        /// <summary>
        /// 정답을 변경하면 선택된 정답 정보를 화면에 출력해 주는 함수
        /// </summary>
        /// <param name="_GuideIndex">인식된 도안의 정답 번호(저장된 정답 개수 범위내에서 값 전달할것)</param>
        private void GuideUpdate(int _GuideIndex)
        {
            CurrBlockTypeList.Clear();

            // 현재 인식된 도안이 없으면 실행
            switch (_PackageManager.CURRENT_PACKAGE_INFO)
            {
                case PackageManager.PACKAGE_INFO.FIRST:
                    {
                        // 인식된 마커가 없으면 실행
                        if (_PackageManager.FC_PACKAGE == PackageManager.FirstCollection.NumOf)
                        {
                            _Pattern_Info[i_TargetPattern2DGuideMaps].IMG_MakerImg.sprite = SPT_DefaultMakerImg;          // 중앙 마커이미지를 기본 이미지로 변경
                            TXT_VirtualPanelText.gameObject.SetActive(true);    // "도안을 인식해주세요" 텍스트 활성화

                            return;
                        }
                        break;
                    }

                case PackageManager.PACKAGE_INFO.RENEWEL:
                    {
                        // 인식된 마커가 없으면 실행
                        if (_PackageManager.RE_PACKAGE == PackageManager.RenewelCollection.NumOf)
                        {
                            _Pattern_Info[i_TargetPattern2DGuideMaps].IMG_MakerImg.sprite = SPT_DefaultMakerImg;          // 중앙 마커이미지를 기본 이미지로 변경
                            TXT_VirtualPanelText.gameObject.SetActive(true);    // "도안을 인식해주세요" 텍스트 활성화

                            return;
                        }
                        break;
                    }

                case PackageManager.PACKAGE_INFO.PPORORO:
                    {
                        // 인식된 마커가 없으면 실행
                        if (_PackageManager.PR_PACKAGE == PackageManager.PpororoCollection.NumOf)
                        {
                            _Pattern_Info[i_TargetPattern2DGuideMaps].IMG_MakerImg.sprite = SPT_DefaultMakerImg;          // 중앙 마커이미지를 기본 이미지로 변경
                            TXT_VirtualPanelText.gameObject.SetActive(true);    // "도안을 인식해주세요" 텍스트 활성화

                            return;
                        }
                        break;
                    }

                default:
                    Debug.LogError("GuideManager(GuideUpdate) :: There is No Package info");
                    return;
            }

            // 중단, 상단 마커이미지를 현재 인식된 도안 이미지로 변경
            _Pattern_Info[i_TargetPattern2DGuideMaps].IMG_MakerImg.sprite   = _PackageManager.STRUCT_CurrentMarkerSpritesInfo.SPT_Pattern;
            IMG_SelectPattern.sprite                                        = _PackageManager.STRUCT_CurrentMarkerSpritesInfo.SPT_Pattern;


            // 오류나 Xml파일 설정 실수로 해당 도안에 대한 정답이 없으면 모든 동작 취소
            if (_XmlManager.ALLGUIDE_LIST.Count == 0 || _XmlManager.ALLGUIDE_LIST.Count == 0)
            {
                Debug.LogWarning("There is No Answer for this pattern.");
                return;
            }

            // 중앙에 존재하는 정답블록 표시구역의 모든 블록 이미지 비활성화
            for (int i = 0; i < BlockTypeViewGridLayoutGroupList.Count; i++)
            {
                if (BlockTypeViewGridLayoutGroupList[i].gameObject.activeSelf)
                {
                    BlockTypeViewGridLayoutGroupList[i].gameObject.SetActive(false);
                }
            }

            // AR 블록 오브젝트 모두 비활성화
            for (int i = 0; i < OBJ_Blocks.Length; i++)
            {
                if (OBJ_Blocks[i].gameObject.activeSelf)
                {
                    OBJ_Blocks[i].gameObject.SetActive(false);
                }

                OBJ_Blocks[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
                OBJ_Blocks[i].transform.SetParent(TRAN_AR_Bloacks.transform);
            }

            // 중앙 도안 이미지에 들어갈 블록 객체들의 이미지 오브젝트들 초기화
            // * 게임 오브젝트 비활성화 / 부모객체를 풀(도안)로 설정 / 회전값 초기화
            for (int i = 0; i < BlockTypePool2DList.Count; i++)
            {
                BlockTypePool2DList[i]._BlockTrans.gameObject.SetActive(false);
                BlockTypePool2DList[i]._BlockTrans.gameObject.transform.SetParent(BlockPool);
                BlockTypePool2DList[i]._BlockTrans.gameObject.transform.rotation = Quaternion.Euler(BlockTypePool2DList[i].oriRotValue);
            }

            // 각 도안마다 존재하는 몇 개의 정답 중 _GuideIndex번째 정답에 필요로하는 블록 수만큼 반복
            for (int i = 0; i < _XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST.Count; i++)
            {
                // i번째 블록이 이미 사용되고 있는지 확인
                int _blockPoolIndex = ReturnUnActivePoolItem(_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].ID);

                // 블록이 이미 사용되고 있다면 예외처리 실행
                if (_blockPoolIndex == -1)
                {
                    Debug.LogError("2D::GuideUpdate(int _GuideIndex) : 풀이 문제발견, 확인바람");
                    return;
                }

                // i번째 AR블록 활성화
                OBJ_Blocks[_blockPoolIndex].SetActive(true);

                // 각 블록에 색상 입력, 도안의 반대되는 색상을 사용해서 시각적으로 잘보이도록 설정
                Color originalColor = BlockColors[(int)_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].COLOR];
                Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b);
                newColor.r -= 1.0f;
                newColor.g -= 1.0f;
                newColor.b -= 1.0f;
                newColor = new Color(Mathf.Abs(newColor.r), Mathf.Abs(newColor.g), Mathf.Abs(newColor.b));


                // --------------------------- //


                // 2D 블럭, 3D 블록 색상 설정
                BlockTypePool2DList[_blockPoolIndex]._BlockImage.color = newColor;
                OBJ_Blocks[_blockPoolIndex].GetComponent<GUID_ARBlockPrefabMeshCollector>().Set_BlockColor(originalColor, newColor);

                // 2D, 3D 블록 풀에 해당 블록이 이동해야하는 좌표에 오브젝트를 부모로 설정
                BlockTypePool2DList[_blockPoolIndex]._BlockTrans.gameObject.transform
                    .SetParent(_Pattern_Info[i_TargetPattern2DGuideMaps].TRAN_Pattern.GetChild(
                        ((int)_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].DX * i_PatternWidth) + 
                        (int)_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].DY));
                OBJ_Blocks[_blockPoolIndex].transform.SetParent(TRAN_GuideMap[i_TargetPattern2DGuideMaps].transform);


                // 부모의 크기에 따라 크기 재설정
                BlockTypePool2DList[_blockPoolIndex]._BlockTrans.localScale = Vector3.one * i_PatternCellSize[i_TargetPattern2DGuideMaps];
                //OBJ_Blocks[_blockPoolIndex].transform.localScale = Vector3.one * 4;

                // 2D블록, 3D블록 위치 초기화
                BlockTypePool2DList[_blockPoolIndex]._BlockTrans.localPosition = Vector3.zero;
                OBJ_Blocks[_blockPoolIndex].transform.localPosition = new Vector3(
                    (int)_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].DY,
                    0,
                    -((int)_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].DX));

                // 2D블록, 3D블록 회전값 * 90도의 값으로 블록 회전값 초기화
                BlockTypePool2DList[_blockPoolIndex]._BlockTrans.gameObject.transform.rotation = Quaternion.Euler(
                    BlockTypePool2DList[_blockPoolIndex]._BlockTrans.gameObject.transform.rotation.eulerAngles +
                    (new Vector3(0, 0, 90) * _XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].ROT));
                OBJ_Blocks[_blockPoolIndex].transform.localRotation = Quaternion.Euler(
                    (new Vector3(0, -90, 0) * _XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].ROT));

                // 추후에 동일한 블록을 또 호출하는지 확인하기 위해 활성화된 블록을 CurrBlockTypeList 리스트에 삽입
                CurrBlockTypeList.Add(BlockTypePool2DList[_blockPoolIndex]._BlockTrans);


                // --------------------------- //


                // 가이드 화면에 항상 표시되는 정답 블록 설정
                for (int j = 0; j < BlockTypeViewGridLayoutGroupList.Count; j++)
                {
                    if (BlockTypeViewGridLayoutGroupList[j].gameObject.activeSelf == false)
                    {
                        BlockTypeViewGridLayoutGroupList[j].sprite = BlockType2DBlockSprites[(int)_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].ID];
                        BlockTypeViewGridLayoutGroupList[j].color = BlockColors[(int)_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[i].COLOR];

                        // 색상이 너무 검은색이면 보기 좋지 않다는 피드백에 검은색상은 임의로 변경
                        if (_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[j].COLOR == Manager.Util.XmlManager.WAITBLOCK_MATCOLOR.BLACK) // BLACK
                            BlockTypeViewGridLayoutGroupList[j].color = new Color32(105, 105, 105, 255);
                        else
                            BlockTypeViewGridLayoutGroupList[j].color = BlockColors[(int)_XmlManager.ALLGUIDE_LIST[_GuideIndex].GUIDE_LIST[j].COLOR];

                        BlockTypeViewGridLayoutGroupList[j].gameObject.SetActive(true);
                        break;
                    }
                }
            }

            // 정답 표시 오브젝트 온/오프 결정
            // 정답을 킨 상태에서 다른 정답으로 바꿨을때, 바로 정답이 보이게 하라는 대표님 말씀때문에 바꿈
            if (isGuideActive)
            {
                for (int i = 0; i < CurrBlockTypeList.Count; i++)
                {
                    CurrBlockTypeList[i].gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// 특정 블록 타입에 대해 해당 블록이 이미 사용되었는지 확인하고, 해당 블록 데이터를 찾아 반환
        /// </summary>
        /// <param name="_blockType">현재 사용해야하는 블록 정보</param>
        /// <returns>블록 풀에 저장되어있는 해당 블록의 위치</returns>
        private int ReturnUnActivePoolItem(Manager.Util.XmlManager.WAITBLOCK_NUM _blockType)
        {
            int poolIndex = -1;

            // 블록 정보 모두 탐색
            for (int i = 0; i < BlockTypePool2DList.Count; i++)
            {
                // 해당 동일한 블록 타입을 찾고, 동일한 블록 타입이 이미 사용되어있지 않다면 실행
                if (BlockTypePool2DList[i]._BlockType == _blockType 
                    && !CurrBlockTypeList.Contains(BlockTypePool2DList[i]._BlockTrans))
                {
                    poolIndex = i; // 해당 블록의 위치 값 저장
                    break;
                }
            }

            return poolIndex; // 해당 블록 위치 값 반환
        }

        private void SetMapSize()
        {
            // 모든 피봇 및 도안 이미지 비활성화
            for(int k = 0; k < _Pattern_Info.Count; k++)
            {
                _Pattern_Info[k].TRAN_Pattern.parent.gameObject.SetActive(false);
            }

            // 도안 크기별 도안 목표 설정
            // 도안 맵 오브젝트 선정
            // 도안 맵 오브젝트 활성화
            switch (_PackageManager.CURRENT_MARKER_SIZE)
            {
                case Util.PackageManager.MARKER_SIZE._8x8:
                    i_PatternWidth = 8;
                    i_TargetPattern2DGuideMaps = 0;
                    _Pattern_Info[0].TRAN_Pattern.parent.gameObject.SetActive(true);
                    break;

                case Util.PackageManager.MARKER_SIZE._8x16:
                    i_PatternWidth = 8;
                    i_TargetPattern2DGuideMaps = 1;
                    _Pattern_Info[1].TRAN_Pattern.parent.gameObject.SetActive(true);
                    break;

                case Util.PackageManager.MARKER_SIZE._16x8:
                    i_PatternWidth = 16;
                    i_TargetPattern2DGuideMaps = 2;
                    _Pattern_Info[2].TRAN_Pattern.parent.gameObject.SetActive(true);
                    break;

                case Util.PackageManager.MARKER_SIZE._16x16:
                    i_PatternWidth = 16;
                    i_TargetPattern2DGuideMaps = 3;
                    _Pattern_Info[3].TRAN_Pattern.parent.gameObject.SetActive(true);
                    break;

                default:
                    i_PatternWidth = 8;
                    i_TargetPattern2DGuideMaps = 0;
                    _Pattern_Info[0].TRAN_Pattern.parent.gameObject.SetActive(true);
                    break;
            }
        }
        #endregion

        // ======================= 도안 정답 패널 ======================= //

        [Header("====== Guide PopUp ======")]
        [SerializeField] private GameObject PAN_SelectGuide                 = null;
        [SerializeField] private GameObject[] OBJ_SlecetGuideItemPrefabs    = null;

        #region 정답 패널에 필요한 함수, 변수 설정 구간
        /// <summary>
        /// 정답표시패널 활성화/비활성화
        /// </summary>
        public void ActiveSelectGuidePopUp()
        {
            // 현재 정답표시패널의 상태에 반대되는 상태로 전환
            PAN_SelectGuide.gameObject.SetActive(!PAN_SelectGuide.gameObject.activeSelf);
        }

        // 팝업 창 내부에 들어가있을 내부 버튼들 속에 정의될 블록들
        private void GUIDE_InitAnswerPopup()
        {
            for(int i = _XmlManager.ALLGUIDE_LIST.Count; i < 10; i++)
            {
                OBJ_SlecetGuideItemPrefabs[i].SetActive(false);
            }

            // 인식된 도안에 저장된 정답 수만큼 반복문 실행
            for (int i = 0; i < _XmlManager.ALLGUIDE_LIST.Count; i++)
            {
                OBJ_SlecetGuideItemPrefabs[i].SetActive(true);

                // 정답을 맞추기위해 필요한 블록들의 수만큼 반복
                for (int j = 0; j < _XmlManager.ALLGUIDE_LIST[i].GUIDE_LIST.Count; j++)
                {
                    // 한 세트당 블록은 11개가 최대이기에 11이 넘으면 반복 취소
                    if (j == 11) break;

                    // 블록 이미지 오브젝트의 이미지를 필요로하는 블록 모형으로 변형
                    OBJ_SlecetGuideItemPrefabs[i].transform.GetChild(1).transform.GetChild(j).gameObject.GetComponent<Image>().sprite = BlockType2DBlockSprites[(int)_XmlManager.ALLGUIDE_LIST[i].GUIDE_LIST[j].ID];

                    // 색상이 너무 검은색이면 보기 좋지 않다는 피드백에 검은색일때만 회색으로 변경
                    if (_XmlManager.ALLGUIDE_LIST[i].GUIDE_LIST[j].COLOR == Manager.Util.XmlManager.WAITBLOCK_MATCOLOR.BLACK) // BLACK
                        OBJ_SlecetGuideItemPrefabs[i].transform.GetChild(1).transform.GetChild(j).gameObject.GetComponent<Image>().color = new Color32(105, 105, 105, 255);
                    else // 다른 모든 색상은 그대로 입력
                        OBJ_SlecetGuideItemPrefabs[i].transform.GetChild(1).transform.GetChild(j).gameObject.GetComponent<Image>().color = BlockColors[(int)_XmlManager.ALLGUIDE_LIST[i].GUIDE_LIST[j].COLOR];

                    // 블록 이미지 오브젝트 활성화
                    OBJ_SlecetGuideItemPrefabs[i].transform.GetChild(1).transform.GetChild(j).gameObject.SetActive(true);
                }

                // 각 정답 버튼 리스너 람다식으로 설정 초기화
                int num = i + 1;
                OBJ_SlecetGuideItemPrefabs[i].gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
                OBJ_SlecetGuideItemPrefabs[i].gameObject.GetComponent<Button>().onClick.AddListener(() => {
                    TXT_ChallengeNumber.text = num.ToString();                  // 가이드씬의 청록색 현재 정답표기번호 설정
                    SoundManager.S.Play_ButtonSound();                          // 사운드 이팩트 설정

                    GuideUpdate(num - 1); // 가이드 업데이트
                    ActiveSelectGuidePopUp();
                });
            }
        }

        /// <summary>
        /// 정답 오브젝트들 활성화/비활성화 함수
        /// </summary>
        public void ActiveGuide()
        {
            // 상태 변경
            isGuideActive = !isGuideActive;

            // 가이드 활성화
            if (isGuideActive)
            {
                // 수정된 2D블록들 활성화
                for (int i = 0; i < CurrBlockTypeList.Count; i++)
                {
                    CurrBlockTypeList[i].gameObject.SetActive(true);
                }
            }
            // 가이드 비활성화
            else
            {
                // 모든 2D블록들 비활서오하
                for (int i = 0; i < CurrBlockTypeList.Count; i++)
                {
                    CurrBlockTypeList[i].gameObject.SetActive(false);
                }
            }
        }
        #endregion

        // ======================= AR 가이드 ======================= //

        [Header("====== AR Guide ======")]
        [SerializeField] private GameObject PAN_Bot;
        [SerializeField] private GameObject BTN_ARBack;
        [SerializeField] private GameObject TXT_ARHelpText;
        [SerializeField] private GameObject IMG_ARCircle;
        [SerializeField] private Transform TRAN_AR_Bloacks;
        [Tooltip("ARGuideMap")]
        [SerializeField] private Transform TRAN_ARGuideMap;
        [SerializeField] private Transform[] TRAN_GuideMap;
        [SerializeField] private GameObject[] OBJ_Blocks;    // 도안이 6색이기에 66개씩한다.

        private bool b_GuideAR = false;
        private string s_CurrentMarker = "";


        #region AR 도안 설정구간
        /// <summary>
        /// AR 세팅 초기화 설정 함수
        /// 마커 인식 후 마커샘플 오브젝트를 DontDestroy로 백업해놓았다면 해당 오브젝트를 가이드씬으로 옮긴다.
        /// </summary>
        private void GUIDE_InitAR()
        {
            int repeat = 0;
            switch (_PackageManager.CURRENT_PACKAGE_INFO)
            {
                case PackageManager.PACKAGE_INFO.FIRST:
                    repeat = 2;
                    break;

                case PackageManager.PACKAGE_INFO.RENEWEL:
                    repeat = 6;
                    break;

                case PackageManager.PACKAGE_INFO.PPORORO:
                    repeat = 2;
                    break;

                default:
                    Debug.LogError("INITAR :: There is No Package Info");
                    return;
            }

            OBJ_Blocks = new GameObject[repeat * 11];

            // 각 패키지별 필요한 세트만큼 블록 생성
            for (int i = 0; i < repeat; i++)
            {
                for(int k = 0; k < 11; k++)
                {
                    OBJ_Blocks[(i * 11) + k] = Instantiate(BlockType3DBlock[k], TRAN_AR_Bloacks);
                }
            }
        }

        /// <summary>
        /// 가이드씬의 AR On/Off버튼 레퍼런스 함수 (최초 버튼은 항상 off상태이다)
        /// </summary>
        public void BTN_ActivateARGuide()
        {
            // 버튼 소리 출력
            SoundManager.S.Play_ButtonSound();

            if (b_GuideAR) return;
            // On/Off 변경
            b_GuideAR = true;

            // AR 모드 종료 버튼 활성화
            BTN_ARBack.SetActive(true);

            // 도움말 켜기
            TXT_ARHelpText.SetActive(true);
            // AR 마커 인식용 동그라미 활성화
            IMG_ARCircle.SetActive(true);
            // 정답표시 패널 비활성화
            PAN_Bot.SetActive(false);
            // AR카메라 활성화
            CustomARCameraManager.Instance.EnableCamera();
        }
        
        /// <summary>
        /// 가이드씬의 AR 모드 Off 버튼
        /// </summary>
        public void BTN_DeactivateARGuide()
        {
            // 버튼 소리 출력
            SoundManager.S.Play_ButtonSound();

            if (!b_GuideAR) return;
            // On/Off 변경
            b_GuideAR = false;

            // AR모드 종료 버튼 비활성화
            BTN_ARBack.SetActive(false);

            // 도움말 끄기
            TXT_ARHelpText.SetActive(false);
            // AR 마커 인식용 동그라미 활성화
            IMG_ARCircle.SetActive(false);
            // 정답표시 패널 활성화
            PAN_Bot.SetActive(true);
            // AR카메라 비활성화
            CustomARCameraManager.Instance.DisableCamera();
        }

        /// <summary>
        /// 메인 메뉴로 돌아가는 함수
        /// </summary>
        public void BTN_BackToMain()
        {
            TRAN_ARGuideMap.SetParent(null);
            _PackageManager.Set_TrackerSceneLoad(false);
        }

        public void MarkerRecognize(string _MarkerName)
        {
            if (_MarkerName == s_CurrentMarker) return;


            Manager.Sound.SoundManager.S.Play_Sound(Manager.Sound.SoundManager.UI_CLIP_LIST.P_RECOG_MarkerScan); // 사운드 실행


            #region 도안 크기 확인
            /*
            // 받은 데이터 분활 (WB_Cup_0 = WB, Cup, 0)
            // 인식된 데이터는 구출씬에서 두 단위로(WB_Cup) 나온다.
            string[] splitData = _MarkerName.Split('_');

            // 도안 명칭 확인
            string name = splitData[0] + "_" + splitData[1];

            // 8x8이 아닌 도안이 인식되면 해당 정보를 XmlManager에 전달
            // 8x8 도안이 아닌 경우 파일 명칭을 "패키지순서_도안이름_도안크기"형식으로 지정하기 때문에 '_'를 기준으로 name변수 크기가 3개가 되면 다른 크기의 도안으로 간주한다.
            if (splitData.Length > 2)
            {
                splitData[2] = "_" + splitData[2];

                // 8x16부터 탐색
                for (int i = 1; i < (int)PackageManager.MARKER_SIZE.NumOf; i++)
                {
                    if (splitData[2].Equals((PackageManager.MARKER_SIZE._8x8 + i).ToString()))
                    {
                        _PackageManager.CURRENT_MARKER_SIZE = (PackageManager.MARKER_SIZE._8x8 + i);
                        break;
                    }
                }
            }
            // 8x8도안이면 초기화
            else
            {
                _PackageManager.CURRENT_MARKER_SIZE = PackageManager.MARKER_SIZE._8x8;
            }

            SetMapSize();
            */

            #endregion

            #region 도안 이미지 설정
            /*
            // 인식된 도안에 대해 각 패키지별 이미지 및 마커 이미지 설정
            switch (PackageManager.Instance.CURRENT_PACKAGE_INFO)
            {
                case PackageManager.PACKAGE_INFO.FIRST:
                    for (int i = 0; i < (int)PackageManager.FirstCollection.NumOf; i++)
                    {
                        if (name.Equals((PackageManager.FirstCollection.WB_Cup + i).ToString()))
                        {
                            _PackageManager.CurrentImage = (int)(PackageManager.FirstCollection.WB_Cup + i);
                            break;
                        }
                    }

                    _PackageManager.FC_PACKAGE = (PackageManager.FirstCollection.WB_Cup + _PackageManager.CurrentImage);
                    break;

                case PackageManager.PACKAGE_INFO.RENEWEL:
                    try
                    {
                        // Renewal은 "RExx_명칭" 포멧으로 splitData[0]에 저장된 "RExx"중 앞 두글자를 지워 남은 자리 수로 몇번째 도안인지 확인한다.
                        _PackageManager.CurrentImage = int.Parse(splitData[0].Substring(2)) - 1;
                    }catch(FormatException e)
                    {
                        Debug.LogError("ERROR (GuideManager) :: WRONG PACKAGE");
                        return;
                    }

                    _PackageManager.RE_PACKAGE = (PackageManager.RenewelCollection.RE1_Larva + _PackageManager.CurrentImage);
                    break;

                case PackageManager.PACKAGE_INFO.PPORORO:
                    try
                    {
                        // Ppororo은 "PRxx_명칭" 포멧으로 splitData[0]에 저장된 "PRxx"중 앞 두글자를 지워 남은 자리 수로 몇번째 도안인지 확인한다.
                        _PackageManager.CurrentImage = int.Parse(splitData[0].Substring(2)) - 1;
                    }
                    catch (FormatException e)
                    {
                        Debug.LogError("ERROR (GuideManager) :: WRONG PACKAGE");
                        return;
                    }

                    _PackageManager.PR_PACKAGE = (PackageManager.PpororoCollection.PR1_CarrotA + _PackageManager.CurrentImage);
                    break;

                default:
                    Debug.LogError("GuideManager(MarkerRecognize) :: No Package Info is set " + PackageManager.Instance.CURRENT_PACKAGE_INFO);
                    return;
            }

            s_CurrentMarker = _MarkerName;
            */

            // 트래킹 오브젝트를 가이드 씬으로 이동
            _PackageManager.Set_TrackerSceneLoad(true);

            // ARCircle 비활성화
            IMG_ARCircle.SetActive(false);
            // "도안을 인식해주세요" 텍스트 비활성화
            TXT_VirtualPanelText.gameObject.SetActive(false);
            // 상단 이미지 도안 변경
            //AsyncAssetLoadManager.Instance.Get_LoadAssetSprite(_PackageManager.s_MarkerAddress[_PackageManager.CurrentImage], ref IMG_SelectPattern);
            // 중단 이미지 도안 변경
            //_Pattern_Info[i_TargetPattern2DGuideMaps].IMG_MakerImg.sprite = IMG_SelectPattern.sprite;
            #endregion

            // 블록 인식 알고리즘을 위한 좌표값 설정 함수 호출
            //_PackageManager.LoadMapData(_MarkerName);
            // 도안 정답 정보 설정 함수 호출
            //_XmlManager.LoadToStageXML(_MarkerName);

            //GuideUpdate(0);
            GUIDE_InitAnswerPopup();

            // AR 오브젝트를 트래커 이미지로 넘기기
            _PackageManager.Set_ARObjectToTrackerObject(TRAN_ARGuideMap);
        }
        #endregion
    }
}
