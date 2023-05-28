using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class BalanceManager : MonoBehaviour
{
    public static BalanceManager S = null;

    public enum STUFF
    {
        Cherry,
        Strawberry,
        Rat,
        Turtle,
        Camel,
        Whale,
    }

    //스테이지정보제공텍스트
    [Header("====텍스트====")]
    public Text t_Timer     = null;
    public Text t_StageText = null;

    [Header("====리소스 및 무게 셋팅===="), Space(5)]
    public Sprite[]     m_StuffImageArrary = null;
    public List<int>    m_StuffKgDataList = new List<int>();

    //풀
    public class Stuff
    {
        public RectTransform    m_RectTransfrom = null;
        public SpriteRenderer   m_MarkerSpriterenderer = null;
        public StuffPrefab      m_StuffPrefabScript = null;
    }
    [Header("====오브젝트===="), Space(5)]
    private List<Stuff> p_Stuff_PoolList    = new List<Stuff>();
    public GameObject   m_StuffPrefab       = null;

    //셋팅
    [Header("====셋팅===="), Space(5)]
    public int      m_StageCount    = 0;
    public bool     m_IsGamePlay    = false;
    public float    m_LifeTime      = 0.0f;
    public Image    m_LifeTimeImage = null;

    private int     p_StageMaxCount = 10;
    private float   p_LifeTime      = 60.0f;

    [System.Serializable] public class FirstCollection
    {
        public float        m_StageLifeTime         = 0.0f;
        public List<int>    m_StageStuffCountList   = new List<int>();
    }
    public List<FirstCollection> m_StageInfoList = new List<FirstCollection>();

    //저울관련
    [Header("====저울 오브젝트===="), Space(5)]
    public List<StuffPrefab>    m_LScalePartManager     = new List<StuffPrefab>();
    public List<StuffPrefab>    m_RScalePartManager     = new List<StuffPrefab>();
    public RectTransform        m_LScaleRectTrans       = null;
    public RectTransform        m_RScaleRectTrans       = null;
    public Transform            m_ArmTrans              = null;

    private void Awake()
    {
        S = this;

        CreatePoolList();
        SetGame();
    }

    private void CreatePoolList()
    {
        p_Stuff_PoolList.Clear();

        int _poolCount = 20;

        for (int i = 0; i < _poolCount; i++)
        {
            Stuff _Stuff = new Stuff();

            _Stuff.m_RectTransfrom = Instantiate(m_StuffPrefab).gameObject.GetComponent<RectTransform>();
            _Stuff.m_MarkerSpriterenderer = _Stuff.m_RectTransfrom.gameObject.GetComponent<SpriteRenderer>();

            _Stuff.m_StuffPrefabScript = _Stuff.m_RectTransfrom.gameObject.GetComponent<StuffPrefab>();
            _Stuff.m_RectTransfrom.gameObject.SetActive(false);

            p_Stuff_PoolList.Add(_Stuff);
        }
    }

    private void SetGame()
    {
        t_StageText.text = "무게 맞추기" + (m_StageCount + 1);
        m_LifeTime = p_LifeTime;
        m_LifeTimeImage.fillAmount = 1.0f;

        SetStuff();
    }

    private IEnumerator p_Coroutine = null;
    public void UpdateScalesSimulation()
    {
        int _left_Kg = 0;
        int _right_Kg = 0;

        //왼쪽리스트검사
        for (int i = 0; i < m_LScalePartManager.Count; i++)
        {
            _left_Kg += m_StuffKgDataList[(int)m_LScalePartManager[i].m_Stuff];
        }

        //오른쪽리스트 검사
        for (int i = 0; i < m_RScalePartManager.Count; i++)
        {
            _right_Kg += m_StuffKgDataList[(int)m_RScalePartManager[i].m_Stuff];
        }

        m_IsGamePlay = false;

        if(p_Coroutine == null)
        {
            p_Coroutine = ScalesSimulation_Cor(_left_Kg, _right_Kg);
        }
        else
        {
            StopCoroutine(p_Coroutine);
            p_Coroutine = ScalesSimulation_Cor(_left_Kg, _right_Kg);
        }
        StartCoroutine(p_Coroutine);
    }

    public void PopList(bool _IsScalePartLeft)
    {
        //왼쪽 리스트에서 팝
        if (_IsScalePartLeft)
        {
            if (m_LScalePartManager.Count == 0) return;

            StuffPrefab _Stuff = m_LScalePartManager[m_LScalePartManager.Count - 1];
            m_LScalePartManager.RemoveAt(m_LScalePartManager.Count - 1);

            _Stuff.ResetPos();
            UpdateScalesSimulation();
        }
        //오른쪽 리스트에서 팝
        else
        {
            if (m_RScalePartManager.Count == 0) return;

            StuffPrefab _Stuff = m_RScalePartManager[m_RScalePartManager.Count - 1];
            m_RScalePartManager.RemoveAt(m_RScalePartManager.Count - 1);

            _Stuff.ResetPos();
            UpdateScalesSimulation();
        }
    }

    const int DEFAULT_HEIGHT = -1;
    IEnumerator ScalesSimulation_Cor(int _leftKg, int _rightKg)
    {
        float _allKg = _leftKg + _rightKg;
        float _Left_WeightDiff = 0;
        float _Right_WeightDiff = 0;
        float _ArmZ = 0;
        if (_allKg != 0)
        {
            _Left_WeightDiff = ((float)(_leftKg - _rightKg) / _allKg) * 0.5f;
            _Right_WeightDiff = ((float)(_rightKg - _leftKg) / _allKg) * 0.5f;
        }

        //저울 러프값 셋팅
        Vector2 _LSpos = m_LScaleRectTrans.anchoredPosition;
        Vector2 _RSpos = m_RScaleRectTrans.anchoredPosition;
        Vector3 _ARMRot = m_ArmTrans.localEulerAngles;

        Vector2 _LEpos = new Vector2(_LSpos.x, (DEFAULT_HEIGHT - _Left_WeightDiff));
        Vector2 _REpos = new Vector2(_RSpos.x, (DEFAULT_HEIGHT - _Right_WeightDiff));
        Vector3 _ARMRotNew = _ARMRot;
        if(_ARMRotNew.z > 340)
        {
            _ARMRotNew.z = 360 + (_Left_WeightDiff) * 30.0f;
        }
        else
        {
            _ARMRotNew.z = (_Left_WeightDiff) * 30.0f;
        }
        //Debug.Log((_Left_WeightDiff) * 30.0f);
        //Debug.Log(_ARMRot);
        //Debug.Log(_ARMRotNew);

        //Debug.Log(_leftKg + " vs " + _rightKg + " : " + _allKg);
        //Debug.Log(_Left_WeightDiff + " vs " + _Right_WeightDiff);
        //Debug.Log("_LEpos : " + _LEpos);
        //Debug.Log("_REpos : " + _REpos);

        float _eTime = 0.0f;
        float _delay = 0.5f;

        while (_eTime < 1.0f)
        {
            _eTime += Time.deltaTime / _delay;

            m_LScaleRectTrans.anchoredPosition = Vector2.Lerp(_LSpos, _LEpos, _eTime);
            m_RScaleRectTrans.anchoredPosition = Vector2.Lerp(_RSpos, _REpos, _eTime);
            m_ArmTrans.localEulerAngles = Vector3.Lerp(_ARMRot, _ARMRotNew, _eTime);

            yield return new WaitForEndOfFrame();
        }

        bool _isAvaliable = true;
        for (int i = 0; i < p_Stuff_PoolList.Count; i++)
        {
            if (p_Stuff_PoolList[i].m_RectTransfrom.gameObject.activeSelf)
            {
                if(p_Stuff_PoolList[i].m_RectTransfrom.gameObject.transform.parent == null)
                {
                    _isAvaliable = false;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        //스테이지클리어체크
        if(!(_leftKg == 0 && _rightKg == 0) && (_leftKg == _rightKg) &&
            _isAvaliable)
        {
            m_StageCount++;
            if(m_StageCount == p_StageMaxCount)
            {
                Debug.Log("게임승리!");
            }
            else
            {
                Debug.Log("스테이지 통과");
                SetGame();
            }
        }
        else
        {
            m_IsGamePlay = true;
        }
    }

    private void SetStuff()
    {
        //TestCode..
        //m_StageCount = 9;

        for (int i = 0; i < m_LScalePartManager.Count; i++)
        {
            m_LScalePartManager[i].ResetPos();
        }

        for (int i = 0; i < m_RScalePartManager.Count; i++)
        {
            m_RScalePartManager[i].ResetPos();
        }

        m_LScalePartManager.Clear();
        m_RScalePartManager.Clear();
        UpdateScalesSimulation();

        for (int i = 0; i < p_Stuff_PoolList.Count; i++)
        {
            p_Stuff_PoolList[i].m_RectTransfrom.gameObject.SetActive(false);
        }

        //재료리스트를 작성합니다.
        List<STUFF> _StuffList = new List<STUFF>();
        for (int i = 0; i < m_StageInfoList[m_StageCount].m_StageStuffCountList.Count; i++)
        {
            if (m_StageInfoList[m_StageCount].m_StageStuffCountList[i] == 0) continue;
            for (int j = 0; j < m_StageInfoList[m_StageCount].m_StageStuffCountList[i]; j++)
            {
                _StuffList.Add(STUFF.Cherry + i);
            }
        }

        float _initX = -2.5f;
        float _initY = -1.15f;
        float _offSetX, _offSetY;
        _offSetX = _offSetY = 1.25f;
        Vector2 _sPos = new Vector2(_initX, _initY);
        
        int _xMaxCount = 0;
        int _EnterYCount = 5;
        for (int i = 0; i < _StuffList.Count; i++)
        {
            for (int j = 0; j < p_Stuff_PoolList.Count; j++)
            {
                if (p_Stuff_PoolList[j].m_RectTransfrom.gameObject.activeSelf) continue;

                p_Stuff_PoolList[j].m_MarkerSpriterenderer.sprite           = m_StuffImageArrary[(int)_StuffList[i]];
                p_Stuff_PoolList[j].m_StuffPrefabScript.m_Stuff             = _StuffList[i];
                p_Stuff_PoolList[j].m_StuffPrefabScript.m_OriRectPos        = _sPos;
                p_Stuff_PoolList[j].m_RectTransfrom.anchoredPosition        = _sPos;
                p_Stuff_PoolList[j].m_RectTransfrom.gameObject.SetActive(true);
                _xMaxCount++;

                if(_xMaxCount == _EnterYCount)
                {
                    _sPos.x = _initX;
                    _sPos.y -= _offSetY;
                    _xMaxCount = 0;
                }
                else
                {
                    _sPos.x += _offSetX;
                }

                break;
            }
        }

        m_IsGamePlay = true;
    }

    private void Update()
    {
        if (m_IsGamePlay == false) return;

        if (m_LifeTime > 0.0f)
        {
            m_LifeTime -= Time.deltaTime;
            m_LifeTimeImage.fillAmount = m_LifeTime / p_LifeTime;
            t_Timer.text = ((int)m_LifeTime).ToString();
        }
        else
        {
            m_LifeTime = 0.0f;
            Debug.Log("SYSTEM : 시간 초과 / 게임 오버");
            m_IsGamePlay = false;
        }
    }
}
