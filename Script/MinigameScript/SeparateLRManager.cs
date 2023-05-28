using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class SeparateLRManager : MonoBehaviour
{
    public static SeparateLRManager S = null;

    public enum CHAR
    {
        StagBeetle,
        Kangaroo,
        Deer,
    }

    public enum ENEMY
    {
        Scorpion,
        Bird,
        Mantis,

        Snake,
        Dinosaur,
        Rhinoceros,

        Crocodile,
        Hippopotamus,
        Eagle,
    }

    //스테이지정보제공텍스트
    public Text m_StageText = null;

    public CHAR m_MyChar = CHAR.StagBeetle;

    public Sprite[] m_CharImageArrary = null;

    public Sprite[] m_EnemyImageArrary  = null;
    private int     p_EnemyRangeCount   = 3;

    //풀
    public class CharObject
    {
        public RectTransform    m_RectTransfrom = null;
        public SpriteRenderer   m_MarkerSpriterenderer = null;
        public CharPrefab       m_CharPrefabScript = null;
    }
    private List<CharObject>    p_CharObject_PoolList = new List<CharObject>();
    public GameObject           m_CharObjectPrefab = null;

    //셋팅
    public bool     m_IsGamePlay = false;
    public float    m_LifeTime = 0.0f;
    public Image    m_LifeTimeImage = null;

    private float p_LifeTime = 60.0f;

    //보관
    public List<CharPrefab> p_LeftSpawnManager     = new List<CharPrefab>();
    public List<CharPrefab> p_RightSpawnManager    = new List<CharPrefab>();

    //스텝별 제한시간
    [System.Serializable]
    public class STEP
    {
        public float    m_MaxRandomCoolTime = 0.0f;
        public int      m_KeepEnemyCount    = 0;
    }
    public List<STEP> m_StepSettingList = new List<STEP>();
    private int p_StepCount = 0;

    private void Awake()
    {
        S = this;

        CreatePoolList();
        SetGame();
    }

    private void CreatePoolList()
    {
        p_CharObject_PoolList.Clear();

        int _poolCount = 10;

        for (int i = 0; i < _poolCount; i++)
        {
            CharObject _CharObj = new CharObject();

            _CharObj.m_RectTransfrom = Instantiate(m_CharObjectPrefab).gameObject.GetComponent<RectTransform>();
            _CharObj.m_MarkerSpriterenderer = _CharObj.m_RectTransfrom.gameObject.GetComponent<SpriteRenderer>();

            _CharObj.m_CharPrefabScript = _CharObj.m_RectTransfrom.gameObject.GetComponent<CharPrefab>();
            _CharObj.m_RectTransfrom.gameObject.SetActive(false);

            p_CharObject_PoolList.Add(_CharObj);
        }
    }

    private void SetGame()
    {
        m_IsGamePlay = true;
        p_StepCount = 0;
        m_StageText.text = "좌우구분하기 " + (p_StepCount + 1);
        m_LifeTime = p_LifeTime;
        m_LifeTimeImage.fillAmount = 1.0f;
        SetChar();
    }

    private void SetChar()
    {
        //주인공 캐릭터 생성하기 + boxCollider 셋팅
        p_CharObject_PoolList[0].m_RectTransfrom.anchoredPosition = Vector2.zero;
        p_CharObject_PoolList[0].m_MarkerSpriterenderer.sprite = m_CharImageArrary[(int)m_MyChar];
        p_CharObject_PoolList[0].m_CharPrefabScript.m_IsChar = true;
        p_CharObject_PoolList[0].m_RectTransfrom.gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(-0.78f, 0.0f);
        p_CharObject_PoolList[0].m_RectTransfrom.gameObject.GetComponent<BoxCollider2D>().size = new Vector2(6.5f, 5.12f);
        p_CharObject_PoolList[0].m_RectTransfrom.gameObject.SetActive(true);

        m_IsGamePlay = true;

        //각자 코루틴 돌리기
        StartCoroutine(Spawn_Cor(CharPrefab.LOOKDIR.LEFT));
        StartCoroutine(Spawn_Cor(CharPrefab.LOOKDIR.RIGHT));
    }

    public void GameOver()
    {
        Debug.Log("피격! / 게임오버");
        m_IsGamePlay = false;
    }

    public void AnsweCheck(CharPrefab _CharPrefabScript)
    {
        if(_CharPrefabScript.m_LookDir == CharPrefab.LOOKDIR.LEFT)
        {
            p_LeftSpawnManager.RemoveAt(p_LeftSpawnManager.IndexOf(_CharPrefabScript));
            _CharPrefabScript.gameObject.SetActive(false);
        }
        else
        {
            p_RightSpawnManager.RemoveAt(p_RightSpawnManager.IndexOf(_CharPrefabScript));
            _CharPrefabScript.gameObject.SetActive(false);
        }
    }

    private bool p_IsSpawnAvoidDuplicateWaitTime = false;
    IEnumerator Spawn_Cor(CharPrefab.LOOKDIR _Dir)
    {
        bool _isActive = false;

        float _eTime = 0.0f;
        float _delay = 0.0f;

        while (true)
        {
            //쿨타임 조건을 만족하는지 검사합니다.
            if(_Dir == CharPrefab.LOOKDIR.LEFT)
            {
                if(p_LeftSpawnManager.Count < m_StepSettingList[p_StepCount].m_KeepEnemyCount)
                {
                    _isActive = true;
                }
            }
            else
            {
                if (p_RightSpawnManager.Count < m_StepSettingList[p_StepCount].m_KeepEnemyCount)
                {
                    _isActive = true;
                }
            }

            //쿨타임이 ON될시, 쿨타임을 랜덤값을 추출합니다.
            if (_isActive)
            {
                float _coolTime = Random.Range(1.0f, m_StepSettingList[p_StepCount].m_MaxRandomCoolTime);
                _eTime = 0.0f;
                _delay = _coolTime;

                while(_eTime < 1.0f)
                {
                    _eTime += Time.deltaTime / _delay;

                    if (p_IsSpawnAvoidDuplicateWaitTime)
                    {
                        _eTime -= 0.2f;
                        if(_eTime <= 0.0f)
                        {
                            _eTime = 0.0f;
                        }
                        p_IsSpawnAvoidDuplicateWaitTime = false;
                    }
                    yield return new WaitForEndOfFrame();
                }

                //쿨타임 기다린뒤, 스폰합니다. + 적 종류를 스텝에따라 범위를 랜덤하게 선택합니다.
                if(_Dir == CharPrefab.LOOKDIR.LEFT)
                {
                    int _rVal = Random.Range((int)m_MyChar * p_EnemyRangeCount, ((int)m_MyChar * p_EnemyRangeCount) + m_StepSettingList[p_StepCount].m_KeepEnemyCount);
                    for (int i = 1; i < p_CharObject_PoolList.Count; i++)
                    {
                        if(p_CharObject_PoolList[i].m_RectTransfrom.gameObject.activeSelf == false)
                        {
                            p_CharObject_PoolList[i].m_RectTransfrom.anchoredPosition = new Vector2(-2.5f, 0.0f);
                            p_CharObject_PoolList[i].m_MarkerSpriterenderer.sprite = m_EnemyImageArrary[_rVal];
                            p_CharObject_PoolList[i].m_CharPrefabScript.m_IsChar = false;
                            p_CharObject_PoolList[i].m_CharPrefabScript.p_MyEnemy = ENEMY.Scorpion + _rVal;
                            p_CharObject_PoolList[i].m_CharPrefabScript.m_LookDir = CharPrefab.LOOKDIR.LEFT;
                            p_CharObject_PoolList[i].m_RectTransfrom.gameObject.SetActive(true);

                            p_LeftSpawnManager.Add(p_CharObject_PoolList[i].m_CharPrefabScript);
                            Debug.Log("LEFT : " + p_CharObject_PoolList[i].m_CharPrefabScript.p_MyEnemy + " 스폰완료");

                            if(p_IsSpawnAvoidDuplicateWaitTime == false)
                            {
                                p_IsSpawnAvoidDuplicateWaitTime = true;
                            }
                            _isActive = false;
                            break;
                        }
                    }
                }
                else
                {
                    int _rVal = Random.Range((int)m_MyChar * p_EnemyRangeCount, ((int)m_MyChar * p_EnemyRangeCount) + m_StepSettingList[p_StepCount].m_KeepEnemyCount);
                    for (int i = 1; i < p_CharObject_PoolList.Count; i++)
                    {
                        if (p_CharObject_PoolList[i].m_RectTransfrom.gameObject.activeSelf == false)
                        {
                            p_CharObject_PoolList[i].m_RectTransfrom.anchoredPosition = new Vector2(2.5f, 0.0f);
                            p_CharObject_PoolList[i].m_MarkerSpriterenderer.sprite = m_EnemyImageArrary[_rVal];
                            p_CharObject_PoolList[i].m_CharPrefabScript.m_IsChar = false;
                            p_CharObject_PoolList[i].m_CharPrefabScript.p_MyEnemy = ENEMY.Scorpion + _rVal;
                            p_CharObject_PoolList[i].m_CharPrefabScript.m_LookDir = CharPrefab.LOOKDIR.RIGHT;
                            p_CharObject_PoolList[i].m_RectTransfrom.gameObject.SetActive(true);

                            p_RightSpawnManager.Add(p_CharObject_PoolList[i].m_CharPrefabScript);
                            Debug.Log("RIGHT : " + p_CharObject_PoolList[i].m_CharPrefabScript.p_MyEnemy + " 스폰완료");

                            if (p_IsSpawnAvoidDuplicateWaitTime == false)
                            {
                                p_IsSpawnAvoidDuplicateWaitTime = true;
                            }
                            _isActive = false;
                            break;
                        }
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void Update()
    {
        if (m_IsGamePlay == false) return;

        if (m_LifeTime > 0.0f)
        {
            m_LifeTime -= Time.deltaTime;
            m_LifeTimeImage.fillAmount = m_LifeTime / p_LifeTime;


            //스텝 설정
            if (m_LifeTime > (p_LifeTime - 20.0f))
            {
                p_StepCount = 0;
                m_StageText.text = "좌우구분하기 " + (p_StepCount + 1);
            }
            else if (m_LifeTime > (p_LifeTime - 40.0f))
            {
                p_StepCount = 1;
                m_StageText.text = "좌우구분하기 " + (p_StepCount + 1);
            }
            else
            {
                p_StepCount = 2;
                m_StageText.text = "좌우구분하기 " + (p_StepCount + 1);
            }
        }
        else
        {
            m_LifeTime = 0.0f;
            Debug.Log("SYSTEM : 게임승리!");
            m_IsGamePlay = false;
        }
    }
}
