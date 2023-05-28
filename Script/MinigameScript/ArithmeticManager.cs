using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ArithmeticManager : MonoBehaviour
{
    public static ArithmeticManager S = null;

    //룰
    private const int MAX_CARD_COUNT = 3; //3 + 2(2)
    private const int MAX_NUMBER_COUNT = 5; //1 ~ 5
    private const int MAX_QUESTION_COUNT = 10;
    private const int MAX_ANSWER_COUNT = 3;
    private const float DEFAULT_LIFETIME = 10.0f;

    //풀
    [System.Serializable] public class Card
    {
        public string Name = "";
        public Transform[] m_Transfrom = new Transform[5];
    }
    [SerializeField] private Card[] m_Models = new Card[4];
    [SerializeField] private List<Transform> m_ModelStack = new List<Transform>();
    [SerializeField] private List<Transform[]> p_Arithmetic_PoolList = new List<Transform[]>();


    [Header("====이미지====")]
    [SerializeField] private Sprite         s_AddMark = null;
    [SerializeField] private Sprite         s_MinusMark = null;

    [Header("====문제===="), Space(5)]
    [SerializeField] private List<string>   p_ArithmeticQuestion_List = new List<string>();
    [SerializeField] private List<int>      p_ArithmeticAnswer_List = new List<int>();
    
    [Header("====셋팅===="), Space(5)]
    [SerializeField] private bool               p_IsGamePlay    = false;
    [SerializeField] private int                m_QuestionCount = 1;
    [SerializeField] private float              m_LifeTime      = 0.0f;
    [SerializeField] private Image              m_LifeTimeImage = null;
    [SerializeField] private Transform          m_PullingStack = null;
    [SerializeField] private Button[]           m_AnswerButton = null;
    [SerializeField] private SpriteRenderer[]   m_Operator = null;
    [SerializeField] private Arthmetic          m_ArthmeticTwo = null;
    [SerializeField] private Arthmetic          m_ArthmeticThree = null;
    [System.Serializable] public class Arthmetic
    {
        public Transform m_Parent;
        public Transform[] m_ChildPos;
    }

    [Header("====텍스트===="), Space(5)]
    [SerializeField] private Text t_Timer;
    [SerializeField] private Text t_StageText;
    [SerializeField] private Text[] t_AnswersText;


    private void Awake()
    {
        S = this;

        p_Arithmetic_PoolList.Clear();

        CreateQuestion();
        SetGame();
    }

    // 문제 생성
    private void CreateQuestion()
    {
        p_ArithmeticQuestion_List.Clear();
        p_ArithmeticAnswer_List.Clear();

        for (int i = 0; i < MAX_QUESTION_COUNT; i++)
        {
            int _CellCount = 2 + (i / 6); //항의 갯수 설정

            while (true)
            {
                string _QuestionData = "";

                int _Val = Random.Range(1, MAX_NUMBER_COUNT + 1);
                _QuestionData += _Val;
                int _isMinus = Random.Range(0, 2); //0 - false, 1 - true

                for (int j = 0; j < _CellCount - 1; j++) //순차 할당
                {
                    _QuestionData += _isMinus;

                    if (_isMinus == 0) // -
                    {
                        int _p = Random.Range(1, MAX_CARD_COUNT + 1);
                        _QuestionData += _p;
                        _isMinus = 1;
                        _Val -= _p;

                        if(_Val <= 0)
                        {
                            break;
                        }
                    }
                    else // +
                    {
                        int _p = Random.Range(1, MAX_CARD_COUNT + 1);
                        _QuestionData += _p;
                        _isMinus = 0;
                        _Val += _p;
                    }
                }

                if(_Val >= 1 && _Val <= MAX_NUMBER_COUNT)
                {
                    p_ArithmeticQuestion_List.Add(_QuestionData);
                    p_ArithmeticAnswer_List.Add(_Val);
                    break;
                }
            }

        }
    }
    private void SetGame()
    {
        p_IsGamePlay = true;

        m_QuestionCount = 1;
        t_StageText.text = "사칙연산게임 " + m_QuestionCount; 
        m_LifeTime = DEFAULT_LIFETIME;

        m_LifeTimeImage.fillAmount = 1.0f;
        SetCard();
    }
    private void NextGame()
    {
        if(m_QuestionCount + 1 > MAX_QUESTION_COUNT)
        {
            Debug.Log("SYSTEM : 게임 승리!");
            p_IsGamePlay = false;
        }
        else
        {
            m_QuestionCount++;
            t_StageText.text = "사칙연산게임 " + m_QuestionCount;
            SetCard();
            m_LifeTime = DEFAULT_LIFETIME;
            m_LifeTimeImage.fillAmount = 1.0f;
        }
    }
    private void SetCard()
    {
        //초기화
        for (int i = 0; i < m_ModelStack.Count; i++)
        {
            m_ModelStack[i].gameObject.SetActive(false);
            m_ModelStack[i].transform.SetParent(m_PullingStack);
            m_ModelStack[i].transform.localScale = Vector3.one;
        }

        m_ModelStack.Clear();

        //1.항이 몇개인지 체크한다. + 배치
        int _CellCount = p_ArithmeticQuestion_List[m_QuestionCount - 1].Length - (p_ArithmeticQuestion_List[m_QuestionCount - 1].Length / 2);
        int[] _ModelCount = new int[_CellCount];

        int length = 0;
        while (true)
        {
            if (length == _CellCount) break;

            int num = Random.Range(0, m_Models.Length - 1);
            _ModelCount[length] = num;

            for (int i = 0; i < length; i++)
            {
                if (_ModelCount[i] == num)
                {
                    length--;
                    break;
                }
            }

            length++;
        }

        Arthmetic arthmetic;
        if (_CellCount == 2)
        {
            arthmetic = m_ArthmeticTwo;
            m_ArthmeticTwo.m_Parent.gameObject.SetActive(true);
            m_ArthmeticThree.m_Parent.gameObject.SetActive(false);
            
            //2.연산자를 출력한다.
            if (p_ArithmeticQuestion_List[m_QuestionCount - 1].Substring(1, 1) == "0")
            {
               m_Operator[0].sprite = s_MinusMark;
            }
            else
            {
                m_Operator[0].sprite = s_AddMark;
            }
        }
        else
        {
            arthmetic = m_ArthmeticThree;
            m_ArthmeticTwo.m_Parent.gameObject.SetActive(false);
            m_ArthmeticThree.m_Parent.gameObject.SetActive(true);

            //2.연산자를 출력한다.
            if (p_ArithmeticQuestion_List[m_QuestionCount - 1].Substring(1, 1) == "0")
            {
                m_Operator[1].sprite = s_MinusMark;
            }
            else
            {
                m_Operator[1].sprite = s_AddMark;
            }

            if (p_ArithmeticQuestion_List[m_QuestionCount - 1].Substring(3, 1) == "0")
            {
                m_Operator[2].sprite = s_MinusMark;
            }
            else
            {
                m_Operator[2].sprite = s_AddMark;
            }
        }

        for (int i = 0; i < _CellCount; i++)
        {
            int _CellNumber = int.Parse(p_ArithmeticQuestion_List[m_QuestionCount - 1].Substring(i * 2, 1));
            //Debug.Log(i * 2 + "/" + _CellNumber);

            for (int k = 0; k < _CellNumber; k++)
            {
                m_ModelStack.Add(m_Models[_ModelCount[i]].m_Transfrom[k]);
                m_Models[_ModelCount[i]].m_Transfrom[k].gameObject.SetActive(true);
            }

            switch (_CellNumber)
            {
                case 1:
                    m_Models[_ModelCount[i]].m_Transfrom[0].SetParent(arthmetic.m_ChildPos[(i * 5) + 2]);
                    break;
                case 2:
                    m_Models[_ModelCount[i]].m_Transfrom[0].SetParent(arthmetic.m_ChildPos[(i * 5)]);
                    m_Models[_ModelCount[i]].m_Transfrom[1].SetParent(arthmetic.m_ChildPos[(i * 5) + 4]);
                    break;
                case 3:
                    m_Models[_ModelCount[i]].m_Transfrom[0].SetParent(arthmetic.m_ChildPos[(i * 5)]);
                    m_Models[_ModelCount[i]].m_Transfrom[1].SetParent(arthmetic.m_ChildPos[(i * 5) + 2]);
                    m_Models[_ModelCount[i]].m_Transfrom[2].SetParent(arthmetic.m_ChildPos[(i * 5) + 4]);
                    break;
                case 4:
                    m_Models[_ModelCount[i]].m_Transfrom[0].SetParent(arthmetic.m_ChildPos[(i * 5)]);
                    m_Models[_ModelCount[i]].m_Transfrom[1].SetParent(arthmetic.m_ChildPos[(i * 5) + 1]);
                    m_Models[_ModelCount[i]].m_Transfrom[2].SetParent(arthmetic.m_ChildPos[(i * 5) + 3]);
                    m_Models[_ModelCount[i]].m_Transfrom[3].SetParent(arthmetic.m_ChildPos[(i * 5) + 4]);
                    break;
                case 5:
                    m_Models[_ModelCount[i]].m_Transfrom[0].SetParent(arthmetic.m_ChildPos[(i * 5)]);
                    m_Models[_ModelCount[i]].m_Transfrom[1].SetParent(arthmetic.m_ChildPos[(i * 5) + 1]);
                    m_Models[_ModelCount[i]].m_Transfrom[2].SetParent(arthmetic.m_ChildPos[(i * 5) + 2]);
                    m_Models[_ModelCount[i]].m_Transfrom[3].SetParent(arthmetic.m_ChildPos[(i * 5) + 3]);
                    m_Models[_ModelCount[i]].m_Transfrom[4].SetParent(arthmetic.m_ChildPos[(i * 5) + 4]);
                    break;
            }
        }

        for(int k = 0; k < m_ModelStack.Count; k++)
        {
            m_ModelStack[k].transform.localPosition = Vector3.zero;
        }


        //3.정답 카드를 N장 출력한다.
        int rVal = Random.Range(0, MAX_ANSWER_COUNT); //정답을 몇번째에 위치할건지 선택
        //Debug.Log("정답 숫자 : " + p_ArithmeticAnswer_List[m_QuestionCount - 1]);
        Debug.Log("정답 위치 : " + (rVal + 1));

        int postAnswer = 0;
        for (int i = 0; i < MAX_ANSWER_COUNT; i++)
        {
            m_AnswerButton[i].onClick.RemoveAllListeners();

            if(i == rVal)
            {
                m_AnswerButton[rVal].onClick.AddListener(()=> { AnswerCheck(true); });
                t_AnswersText[rVal].text = p_ArithmeticAnswer_List[m_QuestionCount - 1].ToString();
                continue;
            }

            m_AnswerButton[i].onClick.AddListener(()=> { AnswerCheck(false); });
            while (true)
            {
                int _CellNumber = Random.Range(1, MAX_NUMBER_COUNT);
                if (_CellNumber != p_ArithmeticAnswer_List[m_QuestionCount - 1] && _CellNumber != postAnswer) 
                {
                    postAnswer = _CellNumber;
                    t_AnswersText[i].text = _CellNumber.ToString();
                    break;
                }
            }
        }
    }

    private void AnswerCheck(bool _isAnswer)
    {
        if (_isAnswer)
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

        if(m_LifeTime > 0.0f)
        {
            m_LifeTime -= Time.deltaTime;
            m_LifeTimeImage.fillAmount = m_LifeTime / DEFAULT_LIFETIME;
            t_Timer.text = ((int)m_LifeTime).ToString();
        }
        else
        {
            m_LifeTime = 0.0f;
            Debug.Log("SYSTEM : 시간 초과 / 게임 오버");
        }
    }
}
