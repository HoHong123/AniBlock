using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchToShadowManager : MonoBehaviour
{
    public static MatchToShadowManager S = null;

    public Sprite[]     m_MarkerImageArrary = null;
    public GameObject   m_ShadowPrefab      = null;

    private int     p_CellMaxCount = 5;
    private float   p_LifeTime = 10.0f;
    private int     p_QuestionMaxCount = 10;
    private int     p_AnswerCount = 1;

    //스테이지정보제공텍스트
    public Text m_StageText = null;

    //풀
    public class Shadow
    {
        public RectTransform    m_RectTransfrom                 = null;
        public SpriteRenderer   m_MarkerSpriterenderer          = null;
        public ShadowPrefab     m_ShadowPrefabScript            = null;
    }
    private List<Shadow> p_Shadow_PoolList = new List<Shadow>();

    //셋팅
    private bool        p_IsGamePlay        = false;
    public int          m_QuestionCount     = 1;
    public float        m_LifeTime          = 0.0f;
    public Image        m_LifeTimeImage     = null;
    public TextMesh[]   m_OperText          = null;
    public int          m_CurrCellMaxCount  = 2;

    private void Awake()
    {
        S = this;

        CreatePoolList();
        SetGame();
    }

    private void CreatePoolList()
    {
        p_Shadow_PoolList.Clear();

        int _poolCount = p_CellMaxCount + p_AnswerCount;

        for (int i = 0; i < _poolCount; i++)
        {
            Shadow _Shadow = new Shadow();

            _Shadow.m_RectTransfrom = Instantiate(m_ShadowPrefab).gameObject.GetComponent<RectTransform>();
            _Shadow.m_MarkerSpriterenderer = _Shadow.m_RectTransfrom.gameObject.GetComponent<SpriteRenderer>();

            _Shadow.m_ShadowPrefabScript = _Shadow.m_RectTransfrom.gameObject.GetComponent<ShadowPrefab>();
            _Shadow.m_RectTransfrom.gameObject.SetActive(false);

            p_Shadow_PoolList.Add(_Shadow);
        }
    }

    private void SetGame()
    {
        p_IsGamePlay = true;

        m_CurrCellMaxCount = 2;
        m_QuestionCount = 1;
        m_StageText.text = "그림자보고맞추기 " + m_QuestionCount;
        m_LifeTime = p_LifeTime;

        m_LifeTimeImage.fillAmount = 1.0f;
        SetShadow();
    }

    private void SetShadow()
    {
        //초기화
        for (int i = 0; i < p_Shadow_PoolList.Count; i++)
        {
           p_Shadow_PoolList[i].m_RectTransfrom.gameObject.SetActive(false);
        }

        //문제생성 (정답추출 ~> 정답을 포함한 그림자추출)
        int _AnswerVal = Random.Range(0, m_MarkerImageArrary.Length);
        Debug.Log("문제정답 : " + _AnswerVal);

        p_Shadow_PoolList[0].m_RectTransfrom.anchoredPosition = new Vector2(0, -3.0f);
        p_Shadow_PoolList[0].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[_AnswerVal];
        p_Shadow_PoolList[0].m_MarkerSpriterenderer.sortingOrder = 1;
        p_Shadow_PoolList[0].m_MarkerSpriterenderer.color = Color.white;
        p_Shadow_PoolList[0].m_ShadowPrefabScript.m_IsAnswer = true;
        p_Shadow_PoolList[0].m_ShadowPrefabScript.m_Number = _AnswerVal;
        p_Shadow_PoolList[0].m_RectTransfrom.gameObject.SetActive(true);

        //어느 그림자에 정답을 표현할지 결정함
        int _AnswerShadowVal = Random.Range(1, m_CurrCellMaxCount + 1);
        Debug.Log("정답위치 : " + _AnswerShadowVal);

        Vector2 _SPos   = Vector2.zero;
        float _Offset = 0.1f;
        switch (m_CurrCellMaxCount)
        {
            case 2:
                _SPos = new Vector2(m_CurrCellMaxCount * -1.0f, 3.0f);
                _Offset = (m_CurrCellMaxCount * 2.0f);

                for (int i = 0; i < p_Shadow_PoolList.Count; i++)
                {
                    p_Shadow_PoolList[i].m_RectTransfrom.localScale = Vector3.one * 0.5f;
                }
                break;

            case 3:
                _SPos = new Vector2(m_CurrCellMaxCount * -0.8f, 3.0f);
                _Offset = (m_CurrCellMaxCount * 0.75f);

                for (int i = 0; i < p_Shadow_PoolList.Count; i++)
                {
                    p_Shadow_PoolList[i].m_RectTransfrom.localScale = Vector3.one * 0.3f;
                }
                break;

            case 4:
                _SPos = new Vector2(m_CurrCellMaxCount * -0.7f, 3.0f);
                _Offset = (m_CurrCellMaxCount * 0.45f);

                for (int i = 0; i < p_Shadow_PoolList.Count; i++)
                {
                    p_Shadow_PoolList[i].m_RectTransfrom.localScale = Vector3.one * 0.2f;
                }
                break;

            case 5:
                _SPos = new Vector2(m_CurrCellMaxCount * -0.6f, 3.0f);
                _Offset = (m_CurrCellMaxCount * 0.3f);

                for (int i = 0; i < p_Shadow_PoolList.Count; i++)
                {
                    p_Shadow_PoolList[i].m_RectTransfrom.localScale = Vector3.one * 0.15f;
                }
                break;
        }

        for (int i = 1; i < m_CurrCellMaxCount + 1; i++)
        {
            if (i == _AnswerShadowVal)
            {
                p_Shadow_PoolList[i].m_RectTransfrom.anchoredPosition = _SPos;
                p_Shadow_PoolList[i].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[_AnswerVal];
                p_Shadow_PoolList[i].m_MarkerSpriterenderer.color = Color.black;
                p_Shadow_PoolList[i].m_ShadowPrefabScript.m_IsAnswer = false;
                p_Shadow_PoolList[i].m_ShadowPrefabScript.m_Number = _AnswerVal;
                p_Shadow_PoolList[i].m_RectTransfrom.gameObject.SetActive(true);
            }
            else
            {
                int _p = -1;
                while (true)
                {
                    _p = Random.Range(0, m_MarkerImageArrary.Length);
                    if(_p != _AnswerVal)
                    {
                        break;
                    }
                }

                p_Shadow_PoolList[i].m_RectTransfrom.anchoredPosition = _SPos;
                p_Shadow_PoolList[i].m_MarkerSpriterenderer.sprite = m_MarkerImageArrary[_p];
                p_Shadow_PoolList[i].m_MarkerSpriterenderer.color = Color.black;
                p_Shadow_PoolList[i].m_ShadowPrefabScript.m_IsAnswer = false;
                p_Shadow_PoolList[i].m_ShadowPrefabScript.m_Number = _p;
                p_Shadow_PoolList[i].m_RectTransfrom.gameObject.SetActive(true);
            }

            _SPos.x += _Offset;
        }
    }

    public void NextGame()
    {
        if (m_QuestionCount + 1 > p_QuestionMaxCount)
        {
            Debug.Log("SYSTEM : 게임 승리!");
            p_IsGamePlay = false;
        }
        else
        {
            m_QuestionCount++;

            m_CurrCellMaxCount = 2 + (m_QuestionCount / 3);
            m_StageText.text = "그림자보고맞추기 " + m_QuestionCount;
            SetShadow();
            m_LifeTime = p_LifeTime;
            m_LifeTimeImage.fillAmount = 1.0f;
        }
    }

    public void AnswerCheck(bool _IsClear)
    {
        if (_IsClear)
        {
            NextGame();
        }
        else
        {
            p_IsGamePlay = false;
            Debug.Log("SYSTEM : 오답 / 게임 오버");
        }
    }

    private void Update()
    {
        if (p_IsGamePlay == false) return;

        if (m_LifeTime > 0.0f)
        {
            m_LifeTime -= Time.deltaTime;
            m_LifeTimeImage.fillAmount = m_LifeTime / p_LifeTime;
        }
        else
        {
            m_LifeTime = 0.0f;
            Debug.Log("SYSTEM : 시간 초과 / 게임 오버");
        }
    }
}
