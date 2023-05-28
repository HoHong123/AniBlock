using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class DivideLRManager : MonoBehaviour
{
    public static DivideLRManager S = null;

    public Sprite[]     m_MarkerImageArrary = null;
    public GameObject   m_DividePrefab      = null;

    //스테이지정보제공텍스트
    public Text m_StageText = null;

    public class DivideObject
    {
        public RectTransform    m_RectTransfrom = null;
        public SpriteRenderer   m_MarkerSpriterenderer = null;
        public DividePrefab     m_DividePrefabScript = null;
    }
    private List<DivideObject> p_DivideObject_PoolList = new List<DivideObject>();

    private float   p_LifeTime              = 60f;
    private int     p_ShowingToObjectCount  = 5;
    private int     p_ClearGoalCount        = 80;

    //셋팅
    public bool     m_IsGamePlay = false;
    public float    m_LifeTime = 0.0f;
    public int      m_CurrClearGoalCount = 0;

    //오브젝트큐
    public  List<int>           m_DivideObjectQueue     = new List<int>();
    private List<int>           p_DivideLeftAnswerList  = new List<int>();
    private List<int>           p_DivideRightAnswerList = new List<int>();

    //정답리스트
    public List<int> m_AnswerList = new List<int>();

    //범위확장데이터
    private int p_OpenRangeData = 2;

    //2개에서 3개로확장시 아래 변수변경
    private bool p_isThreeCount = false;

    //클리어조건
    private int p_ClearCount = 0;

    private void Awake()
    {
        S = this;

        CreatePoolList();
        SetGame();
    }

    private void SetGame()
    {
        m_IsGamePlay = false;
        m_LifeTime = p_LifeTime;
        m_CurrClearGoalCount = 0;
        SetDivideLR();
    }

    private void SetDivideLR()
    {
        //초기화
        for (int i = 0; i < p_DivideObject_PoolList.Count; i++)
        {
            p_DivideObject_PoolList[i].m_RectTransfrom.gameObject.SetActive(false);
        }

        m_DivideObjectQueue.Clear();

        //정답 3개를 고르기
        for (int i = 0; i < 3; i++)
        {
            while (true)
            {
                int _p = Random.Range(0, m_MarkerImageArrary.Length);
                if (m_AnswerList.Contains(_p) == false)
                {
                    m_AnswerList.Add(_p);
                    Debug.Log("정답 : " + _p);
                    break;
                }
            }
        }

        //왼쪽에 2개를 몰지, 오른쪽에 2개를 몰지 결정 [0. 1] && 버튼생성
        int _IsLeft = Random.Range(0, 2);
        float _HorizontalVal = 2.5f;

        for (int i = 0; i < 3; i++)
        {
            p_DivideObject_PoolList[i].m_DividePrefabScript.m_IsEnabled = true;
        }

        if (_IsLeft == 1)
        {
            p_DivideLeftAnswerList.Add(m_AnswerList[0]);
            p_DivideLeftAnswerList.Add(m_AnswerList[2]);

            p_DivideRightAnswerList.Add(m_AnswerList[1]);

            p_DivideObject_PoolList[0].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[m_AnswerList[0]];
            p_DivideObject_PoolList[0].m_RectTransfrom.anchoredPosition = new Vector2(-_HorizontalVal, -3.0f);
            p_DivideObject_PoolList[0].m_DividePrefabScript.m_Number = m_AnswerList[0];
            p_DivideObject_PoolList[0].m_DividePrefabScript.m_Dir = DividePrefab.DIR.LEFT;
            p_DivideObject_PoolList[0].m_RectTransfrom.gameObject.SetActive(true);

            p_DivideObject_PoolList[1].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[m_AnswerList[1]];
            p_DivideObject_PoolList[1].m_RectTransfrom.anchoredPosition = new Vector2(_HorizontalVal, -3.0f);
            p_DivideObject_PoolList[1].m_DividePrefabScript.m_Number = m_AnswerList[1];
            p_DivideObject_PoolList[1].m_DividePrefabScript.m_Dir = DividePrefab.DIR.RIGHT;
            p_DivideObject_PoolList[1].m_RectTransfrom.gameObject.SetActive(true);


            //2 == hide
            p_DivideObject_PoolList[2].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[m_AnswerList[2]];
            p_DivideObject_PoolList[2].m_RectTransfrom.anchoredPosition = new Vector2(-_HorizontalVal, -1.0f);
            p_DivideObject_PoolList[2].m_DividePrefabScript.m_Number = m_AnswerList[2];
            p_DivideObject_PoolList[2].m_DividePrefabScript.m_Dir = DividePrefab.DIR.LEFT;
            p_DivideObject_PoolList[2].m_RectTransfrom.gameObject.SetActive(false);
        }
        else
        {
            p_DivideLeftAnswerList.Add(m_AnswerList[0]);

            p_DivideRightAnswerList.Add(m_AnswerList[1]);
            p_DivideRightAnswerList.Add(m_AnswerList[2]);

            p_DivideObject_PoolList[0].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[m_AnswerList[0]];
            p_DivideObject_PoolList[0].m_RectTransfrom.anchoredPosition = new Vector2(-_HorizontalVal, -3.0f);
            p_DivideObject_PoolList[0].m_DividePrefabScript.m_Number = m_AnswerList[0];
            p_DivideObject_PoolList[0].m_DividePrefabScript.m_Dir = DividePrefab.DIR.LEFT;
            p_DivideObject_PoolList[0].m_RectTransfrom.gameObject.SetActive(true);

            p_DivideObject_PoolList[1].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[m_AnswerList[1]];
            p_DivideObject_PoolList[1].m_RectTransfrom.anchoredPosition = new Vector2(_HorizontalVal, -3.0f);
            p_DivideObject_PoolList[1].m_DividePrefabScript.m_Number = m_AnswerList[1];
            p_DivideObject_PoolList[1].m_DividePrefabScript.m_Dir = DividePrefab.DIR.RIGHT;
            p_DivideObject_PoolList[1].m_RectTransfrom.gameObject.SetActive(true);


            //2 == hide
            p_DivideObject_PoolList[2].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[m_AnswerList[2]];
            p_DivideObject_PoolList[2].m_RectTransfrom.anchoredPosition = new Vector2(_HorizontalVal, -1.0f);
            p_DivideObject_PoolList[2].m_DividePrefabScript.m_Number = m_AnswerList[2];
            p_DivideObject_PoolList[2].m_DividePrefabScript.m_Dir = DividePrefab.DIR.RIGHT;
            p_DivideObject_PoolList[2].m_RectTransfrom.gameObject.SetActive(false);
        }

        m_IsGamePlay = true;
        p_OpenRangeData = 2;

        //탑쌓기 (큐이용)
        for (int i = 3; i < p_DivideObject_PoolList.Count; i++)
        {
            if(p_DivideObject_PoolList[i].m_RectTransfrom.gameObject.activeSelf == false)
            {
                int rVal = Random.Range(0, m_AnswerList.Count - 1);

                p_DivideObject_PoolList[i].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[m_AnswerList[rVal]];
                p_DivideObject_PoolList[i].m_DividePrefabScript.m_Number = m_AnswerList[rVal];
                p_DivideObject_PoolList[i].m_RectTransfrom.gameObject.SetActive(true);

                m_DivideObjectQueue.Add(i);
            }

            if(m_DivideObjectQueue.Count == p_ShowingToObjectCount)
            {
                break;
            }
        }

        //탑정렬
        Vector2 _SPos = new Vector2(0.0f, -1.5f);
        float   _OffsetY = 1.5f;

        for (int i = 0; i < m_DivideObjectQueue.Count; i++)
        {
            p_DivideObject_PoolList[m_DivideObjectQueue[i]].m_RectTransfrom.anchoredPosition = _SPos;
            _SPos.y += _OffsetY;
        }

        //게임준비완료 시작
        m_IsGamePlay = true;
    }

    public void AnswerCheck(DividePrefab.DIR _Dir)
    {
        bool _isAvaliable = false;

        if(_Dir == DividePrefab.DIR.LEFT)
        {
            for (int i = 0; i < p_DivideLeftAnswerList.Count; i++)
            {
                if (p_DivideObject_PoolList[m_DivideObjectQueue[0]].m_DividePrefabScript.m_Number ==
                   p_DivideLeftAnswerList[i])
                {
                    _isAvaliable = true;
                }
            }
        }
        else
        {
            for (int i = 0; i < p_DivideRightAnswerList.Count; i++)
            {
                if (p_DivideObject_PoolList[m_DivideObjectQueue[0]].m_DividePrefabScript.m_Number ==
                   p_DivideRightAnswerList[i])
                {
                    _isAvaliable = true;
                }
            }
        }


        if(_isAvaliable)
        {
            m_IsGamePlay = false;
            //Debug.Log("정답");
            p_ClearCount++;
            m_StageText.text = "좌우로 나누기 " + p_ClearCount;
            if (p_ClearCount >= 40)
            {
                p_isThreeCount = true;
                p_DivideObject_PoolList[2].m_RectTransfrom.gameObject.SetActive(true);
            }

            if(p_ClearCount == 80)
            {
                Debug.Log("게임 승리!");
                m_IsGamePlay = false;
            }

            //현재 오브젝트 비활성화
            p_DivideObject_PoolList[m_DivideObjectQueue[0]].m_RectTransfrom.gameObject.SetActive(false);
            m_DivideObjectQueue.RemoveAt(0);


            //새로운놈 하나붙이기
            int rVal = -1;
            if (p_isThreeCount == false)
            {
                for (int i = 3; i < p_DivideObject_PoolList.Count; i++)
                {
                    if (p_DivideObject_PoolList[i].m_RectTransfrom.gameObject.activeSelf == false)
                    {
                        rVal = Random.Range(0, m_AnswerList.Count - 1);

                        p_DivideObject_PoolList[i].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[m_AnswerList[rVal]];
                        p_DivideObject_PoolList[i].m_DividePrefabScript.m_Number = m_AnswerList[rVal];
                        p_DivideObject_PoolList[i].m_RectTransfrom.gameObject.SetActive(true);

                        m_DivideObjectQueue.Add(i);
                    }

                    if (m_DivideObjectQueue.Count == p_ShowingToObjectCount)
                    {
                        break;
                    }
                }
            }
            else
            {
                for (int i = 3; i < p_DivideObject_PoolList.Count; i++)
                {
                    if (p_DivideObject_PoolList[i].m_RectTransfrom.gameObject.activeSelf == false)
                    {
                        rVal = Random.Range(0, m_AnswerList.Count);

                        p_DivideObject_PoolList[i].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[m_AnswerList[rVal]];
                        p_DivideObject_PoolList[i].m_DividePrefabScript.m_Number = m_AnswerList[rVal];
                        p_DivideObject_PoolList[i].m_RectTransfrom.gameObject.SetActive(true);

                        m_DivideObjectQueue.Add(i);
                    }

                    if (m_DivideObjectQueue.Count == p_ShowingToObjectCount)
                    {
                        break;
                    }
                }
            }

            Vector2 _SPos = new Vector2(0.0f, -1.5f);
            float _OffsetY = 1.5f;

            for (int i = 0; i < m_DivideObjectQueue.Count; i++)
            {
                p_DivideObject_PoolList[m_DivideObjectQueue[i]].m_RectTransfrom.anchoredPosition = _SPos;
                _SPos.y += _OffsetY;
            }

            //게임준비완료 시작
            m_IsGamePlay = true;
        }
        else
        {
            Debug.Log("오답 / 제한시간 5초 감소");
            m_LifeTime -= 5.0f;
            if(m_LifeTime <= 0.0f)
            {
                m_LifeTime = 0.01f;
            }
        }
    }

    private void CreatePoolList()
    {
        p_DivideObject_PoolList.Clear();

        int _poolCount = 10;

        for (int i = 0; i < _poolCount; i++)
        {
            DivideObject _DivideObj = new DivideObject();

            _DivideObj.m_RectTransfrom = Instantiate(m_DividePrefab).gameObject.GetComponent<RectTransform>();
            _DivideObj.m_MarkerSpriterenderer = _DivideObj.m_RectTransfrom.gameObject.GetComponent<SpriteRenderer>();
            _DivideObj.m_DividePrefabScript = _DivideObj.m_RectTransfrom.gameObject.GetComponent<DividePrefab>();
            _DivideObj.m_RectTransfrom.gameObject.SetActive(false);

            p_DivideObject_PoolList.Add(_DivideObj);
        }
    }

    private void Update()
    {
        if (m_IsGamePlay == false) return;

        if (m_LifeTime > 0.0f)
        {
            m_LifeTime -= Time.deltaTime;
        }
        else
        {
            m_LifeTime = 0.0f;
            Debug.Log("SYSTEM : 시간 초과 / 게임 오버");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            AnswerCheck(DividePrefab.DIR.LEFT);
         }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            AnswerCheck(DividePrefab.DIR.RIGHT);
        }
    }
}
