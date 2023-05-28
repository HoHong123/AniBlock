using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class FlipCardManager : MonoBehaviour
{
    public static FlipCardManager S = null;

    public Sprite[]     m_MarkerImageArrary = null;
    public GameObject   m_FlipCardPrefab    = null;

    //스테이지정보제공텍스트
    public Text m_StageText = null;

    private int     p_HeartMaxCount     = 3;
    private float   p_LifeTime          = 10.0f;
    private int     p_QuestionMaxCount  = 10;

    //셋팅
    private int     p_PairCheckCount = 0;
    public int      m_CheckToPairCount = 0;
    public bool     m_IsGamePlay = false;
    public int      m_CurrHeartCount = 0;
    public int      m_QuestionCount = 1;
    public float    m_LifeTime = 0.0f;
    public Image    m_LifeTimeImage = null;
    public int      m_CurrCellMaxCount = 2;
    public float    m_FlipCardShowingWaitTime = 5.0f;

    //정답셋팅
    public List<FlipCardPrefab> m_PairCheckList = new List<FlipCardPrefab>();

    public class FlipCard
    {
        public RectTransform    m_RectTransfrom         = null;
        public SpriteRenderer   m_MarkerSpriterenderer  = null;
        public FlipCardPrefab   m_FlipCardPrefabScript  = null;
    }
    private List<FlipCard> p_FlipCard_PoolList = new List<FlipCard>();

    //하트
    public Transform[] m_HeartArrary = null;

    private void Awake()
    {
        S = this;

        CreatePoolList();
        SetGame();
    }

    private void NextGame()
    {
        if (m_QuestionCount + 1 > p_QuestionMaxCount)
        {
            Debug.Log("SYSTEM : 게임 승리!");
            m_IsGamePlay = false;
        }
        else
        {
            m_QuestionCount++;
            m_StageText.text = "카드뒤집기 " + m_QuestionCount;
            SetFlipCard();
            m_LifeTime = p_LifeTime;
            m_LifeTimeImage.fillAmount = 1.0f;
            m_IsGamePlay = false;
        }
    }

    public void PairCheck(FlipCardPrefab _FlipCardScript)
    {
        if(m_PairCheckList.Count == 0)
        {
            m_PairCheckList.Add(_FlipCardScript);
        }
        else
        {
            m_PairCheckList.Add(_FlipCardScript);

            if(m_PairCheckList[0].m_Number == m_PairCheckList[1].m_Number)
            {
                m_CheckToPairCount++;
                m_PairCheckList[0].m_IsClear = true;
                m_PairCheckList[1].m_IsClear = true;
                m_PairCheckList.Clear();

                if(m_CheckToPairCount == p_PairCheckCount)
                {
                    NextGame();
                }
            }
            else
            {
                m_IsGamePlay = false;
                StartCoroutine(DisCountHeart());
            }
        }
    }

    IEnumerator DisCountHeart()
    {
        m_CurrHeartCount--;
        if(m_CurrHeartCount == 0)
        {
            m_HeartArrary[m_CurrHeartCount].gameObject.SetActive(false);
            Debug.Log("하트모두 소진 / 게임 오버");
            m_IsGamePlay = false;
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
            m_HeartArrary[m_CurrHeartCount].gameObject.SetActive(false);
            m_PairCheckList[0].ActiveFlip(true);
            m_PairCheckList[1].ActiveFlip(true);
            m_PairCheckList.Clear();
            m_IsGamePlay = true;
        }

        yield return null;
    }

    private void SetGame()
    {
        m_IsGamePlay = false;
        m_CurrCellMaxCount = 2;
        m_StageText.text = "카드뒤집기 " + m_QuestionCount;
        m_LifeTime = p_LifeTime;
        m_LifeTimeImage.fillAmount = 1.0f;
        SetFlipCard();
    }

    public static void ShuffleList<T>(List<T> list)
    {
        int random1;
        int random2;

        T tmp;

        for (int index = 0; index < list.Count; ++index)
        {
            random1 = UnityEngine.Random.Range(0, list.Count);
            random2 = UnityEngine.Random.Range(0, list.Count);

            tmp = list[random1];
            list[random1] = list[random2];
            list[random2] = tmp;
        }
    }

    private void SetFlipCard()
    {
        //초기화
        for (int i = 0; i < p_FlipCard_PoolList.Count; i++)
        {
            p_FlipCard_PoolList[i].m_RectTransfrom.gameObject.SetActive(false);
        }

        //문제생성 (m_CurrCellMaxCount 갯수내, 쌍쌍바로배치)
        List<int>   _AnswerList = new List<int>();

        switch (m_QuestionCount)
        {
            case 1:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 2;
                m_FlipCardShowingWaitTime = 3.0f;
                break;

            case 2:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 2;
                m_FlipCardShowingWaitTime = 2.0f;
                break;

            case 3:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 3;
                m_FlipCardShowingWaitTime = 3.0f;
                break;

            case 4:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 3;
                m_FlipCardShowingWaitTime = 2.0f;
                break;

            case 5:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 4;
                m_FlipCardShowingWaitTime = 3.0f;
                break;

            case 6:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 4;
                m_FlipCardShowingWaitTime = 3.0f;
                break;

            case 7:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 4;
                m_FlipCardShowingWaitTime = 2.0f;
                break;

            case 8:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 4;
                m_FlipCardShowingWaitTime = 1.0f;
                break;

            case 9:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 5;
                m_FlipCardShowingWaitTime = 5.0f;
                break;

            case 10:
                m_CurrHeartCount = 3;
                p_PairCheckCount = 5;
                m_FlipCardShowingWaitTime = 4.0f;
                break;
        }

        m_CheckToPairCount = 0;
        //하트채우기
        for (int i = 0; i < m_CurrHeartCount; i++)
        {
            m_HeartArrary[i].gameObject.SetActive(true);
        }

        //페어 고르기(중복없게)
        for (int i = 0; i < p_PairCheckCount; i++)
        {
            while (true)
            {
                int _AnswerVal = Random.Range(0, m_MarkerImageArrary.Length);

                if (_AnswerList.Contains(_AnswerVal) == false)
                {
                    Debug.Log("문제정답 : " + _AnswerVal);
                    _AnswerList.Add(_AnswerVal);
                    break;
                }
            }
        }

        //카드 셋팅
        Vector2 _SPos = Vector2.zero;
        float _InitX = 0.1f;
        float _OffsetX = 0.1f;
        float _OffsetY = 0.1f;
        float _Scale = 0.3f;

        int fIndex = 0;
        for (int i = 0; i < _AnswerList.Count; i++)
        {
            int _index = fIndex;
            for (int j = _index; j < _index + 2; j++)
            {
                p_FlipCard_PoolList[j].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[_AnswerList[i]];
                p_FlipCard_PoolList[j].m_FlipCardPrefabScript.m_IsEnabled = false;
                p_FlipCard_PoolList[j].m_FlipCardPrefabScript.m_Number = _AnswerList[i];
                p_FlipCard_PoolList[j].m_FlipCardPrefabScript.m_IsFlip = true;
                p_FlipCard_PoolList[j].m_FlipCardPrefabScript.m_IsClear = false;
                p_FlipCard_PoolList[j].m_RectTransfrom.gameObject.SetActive(true);
                fIndex++;
            }
        }

        int _SpaceEnterCount = 0; //줄바꿈

        switch(p_PairCheckCount)
        {
            case 2:
                _SPos = new Vector2(-2.5f, 3.0f);
                _InitX = _SPos.x;
                _OffsetX = 1.65f;
                _OffsetY = 0.5f;
                _SpaceEnterCount = 4;
                _Scale = 0.4f;
                break;

            case 3:
                _SPos = new Vector2(-2.5f, 4.5f);
                _InitX = _SPos.x;
                _OffsetX = 2.5f;
                _OffsetY = 2.5f;
                _SpaceEnterCount = 3;
                _Scale = 0.4f;
                break;

            case 4:
                _SPos = new Vector2(-2.5f, 4.0f);
                _InitX = _SPos.x;
                _OffsetX = 1.65f;
                _OffsetY = 2.5f;
                _SpaceEnterCount = 4;
                _Scale = 0.4f;
                break;

            case 5:
                _SPos = new Vector2(-2.5f, 4.8f);
                _InitX = _SPos.x;
                _OffsetX = 1.65f;
                _OffsetY = 1.8f;
                _SpaceEnterCount = 4;
                _Scale = 0.3f;
                break;
        }

        fIndex = 0;

        List<RectTransform> SuffleList = new List<RectTransform>();
        for (int i = 0; i < p_PairCheckCount * 2; i++)
        {
            p_FlipCard_PoolList[i].m_RectTransfrom.localScale = Vector3.one * _Scale;
            SuffleList.Add(p_FlipCard_PoolList[i].m_RectTransfrom);
        }
        ShuffleList(SuffleList);

        for (int i = 0; i < SuffleList.Count; i++)
        {
            SuffleList[i].anchoredPosition = _SPos;
            fIndex++;
            if(fIndex == _SpaceEnterCount)
            {
                fIndex = 0;
                _SPos.x = _InitX;
                _SPos.y -= _OffsetY;
            }
            else
            {
                _SPos.x += _OffsetX;
            }
        }

        StartCoroutine(WaitForFlipCardShowingTime());
    }

    IEnumerator WaitForFlipCardShowingTime()
    {
        float _eTime = 0.0f;
        float _delay = m_FlipCardShowingWaitTime;

        while(_eTime < 1)
        {
            _eTime += Time.deltaTime / _delay;
            m_StageText.text = "카드뒤집기 " + m_QuestionCount + "\n" + (m_FlipCardShowingWaitTime - (m_FlipCardShowingWaitTime * _eTime)).ToString("N1");

            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < p_FlipCard_PoolList.Count; i++)
        {
            if (p_FlipCard_PoolList[i].m_RectTransfrom.gameObject.activeSelf)
            {
                p_FlipCard_PoolList[i].m_FlipCardPrefabScript.m_IsEnabled = true;
                p_FlipCard_PoolList[i].m_FlipCardPrefabScript.ActiveFlip(true);
            }
        }

        m_StageText.text = "카드뒤집기 " + m_QuestionCount;

        m_IsGamePlay = true;

        yield return null;
    }

    private void CreatePoolList()
    {
        p_FlipCard_PoolList.Clear();

        int _poolCount = 10;

        for (int i = 0; i < _poolCount; i++)
        {
            FlipCard _FlipCard = new FlipCard();

            _FlipCard.m_RectTransfrom = Instantiate(m_FlipCardPrefab).gameObject.GetComponent<RectTransform>();
            _FlipCard.m_MarkerSpriterenderer = _FlipCard.m_RectTransfrom.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
            _FlipCard.m_FlipCardPrefabScript = _FlipCard.m_RectTransfrom.gameObject.GetComponent<FlipCardPrefab>();
            _FlipCard.m_RectTransfrom.gameObject.SetActive(false);

            p_FlipCard_PoolList.Add(_FlipCard);
        }
    }

    private void Update()
    {
        if (m_IsGamePlay == false) return;

        if (m_LifeTime > 0.0f)
        {
            m_LifeTime -= Time.deltaTime;
            m_LifeTimeImage.fillAmount = m_LifeTime / p_LifeTime;
        }
        else
        {
            m_LifeTime = 0.0f;
            Debug.Log("SYSTEM : 시간 초과 / 게임 오버");
            m_IsGamePlay = false;
        }
    }
}
