using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEngine.SceneManagement;

/*
 * 
 * 
 * 
 */

namespace Manager.Util
{
    public class XmlManager : MonoBehaviour
    {
        // ======================= 정적 변수 ======================= //
        
        public static XmlManager S = null;

        // Resources/XML/TestItem.XML 파일.
        private const string XML_FILE = "StageInfo";
        private const string MAP_DATA_FILE = "DepthData";
        private const string LIVEGAME_DATA_FILE = "LiveGameInfo";
        private const string MARKER_DATA_FILE = "MarkerData";

        // ======================= 동적 변수 ======================= //

        #region ENUM
        public enum MINIGAME_LIST
        {
            Management, // 분류
            Arithmetic, // 사칙연산
            MatchToShadow, // 그림자
            FlipCard, // 카드
            DivideLR, // 좌우 타워
            Balance, // 저울
            SeparateLR, // 살아남기 게임
            SpaceShooter, // 우주선 게임
            ARgame, // AR 미니게임
            WreckAMole, // 두더지
            Seesaw, // 시소
            Trickery, // 야바위
            Jumping, // 장애물 뛰어넘기
            LiveGame, // 라이브게임(AR)
        }

        //#1. AR가이드의 블록 종류 정의
        public enum WAITBLOCK_NUM
        {
            AM1,
            D1,
            TR1,
            TR2,
            TE1,
            TE2,
            TE3,
            TE4,
            TE5,
            TE6,
            TE7,
        }

        //#2. AR가이드의 블록 색상 정의
        public enum WAITBLOCK_MATCOLOR
        {
            YELLOW, //
            ORANGE, //
            GREEN,  //
            BLUE,   //
            RED,    //
            BROWN,  //
            WHITE,  //
            BLACK,  //
            GRAY,   //
            NAVY,   //
            SKYBLUE,//
            HOTPINK,//
            DEEPBLUE,//
            NONE,
        }
        #endregion

        #region 도안인식 변수
        //#2. AR가이드 정답을 관리하는 리스트 (중요) -> AR가이드의 모든정보를 담음 (3개)
        [System.Serializable] public class GUIDE_ITEM // 정답 블록 정보
        {
            public WAITBLOCK_NUM ID; // string 
            public WAITBLOCK_MATCOLOR COLOR; // string
            public int DX; // int
            public int DY; // int
            public int ROT; // int
        }
        [System.Serializable] public class ALLGIODECLASS
        {
            public List<GUIDE_ITEM> GUIDE_LIST = new List<GUIDE_ITEM>();
        }

        [Header("====== 인식 도안 정답 정보 ======")]
        public List<ALLGIODECLASS> ALLGUIDE_LIST = new List<ALLGIODECLASS>();
        #endregion

        #region 도안 정보 및 각 칸마다의 색상 정보 저장 변수
        public struct Coord
        {
            public List<char> CoordList;
            public int x, y;
            public MINIGAME_LIST minigame;
        }
        public Coord coordInfo = new Coord();
        public Coord GetCoord()
        {
            return coordInfo;
        }
        #endregion

        // ======================= 초기화 ======================= //

        private void Awake()
        {
            if (S != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                S = this;
                DontDestroyOnLoad(this.gameObject);
            }

            coordInfo.CoordList = new List<char>();
        }

        /// <summary>
        /// 도안 정답 정보 호출
        /// </summary>
        /// <param name="_fileName">StageInfo에서 사용되는 엘리멘트 명칭 = 도안 트래커 명칭</param>
        public void LoadToStageXML(string _fileName)
        {
            ALLGUIDE_LIST.Clear();
            if (_fileName == "") return;

            TextAsset txtAsset = (TextAsset)Resources.Load("XML/" + XML_FILE);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(txtAsset.text);
            //Debug.Log("dataroot/" + _fileName);

            int _value = 0;
            while (true)
            {
                // 전체 아이템 가져오기 예제.(Stage 별 대기블록 정보 추출 - Duck + _ + 0(random Select))
                XmlNodeList all_nodes = xmlDoc.SelectNodes("dataroot/" + _fileName + "_" + _value);

                if (all_nodes.Count == 0)
                {
                    //Debug.Log("최대 정답도안 불러온 갯수 : " + _value);
                    break;
                }
                ALLGUIDE_LIST.Add(new ALLGIODECLASS());

                foreach (XmlNode node in all_nodes)
                {
                    int fIndex = 0;

                    foreach (XmlElement element in node)
                    {
                        if (fIndex == 0)
                            ALLGUIDE_LIST[_value].GUIDE_LIST.Add(new GUIDE_ITEM());

                        //Debug.Log(element.InnerText);

                        switch (fIndex)
                        {
                            case 0:
                                ALLGUIDE_LIST[_value].GUIDE_LIST[ALLGUIDE_LIST[_value].GUIDE_LIST.Count - 1].ID = DefineID(element.InnerText);
                                break;

                            case 1:
                                ALLGUIDE_LIST[_value].GUIDE_LIST[ALLGUIDE_LIST[_value].GUIDE_LIST.Count - 1].COLOR = DefineColor(element.InnerText);
                                break;

                            case 2:
                                ALLGUIDE_LIST[_value].GUIDE_LIST[ALLGUIDE_LIST[_value].GUIDE_LIST.Count - 1].DX = int.Parse(element.InnerText);
                                break;

                            case 3:
                                ALLGUIDE_LIST[_value].GUIDE_LIST[ALLGUIDE_LIST[_value].GUIDE_LIST.Count - 1].DY = int.Parse(element.InnerText);
                                break;

                            case 4:
                                ALLGUIDE_LIST[_value].GUIDE_LIST[ALLGUIDE_LIST[_value].GUIDE_LIST.Count - 1].ROT = int.Parse(element.InnerText);
                                break;
                        }

                        fIndex++;
                        if (fIndex == 5)
                            fIndex = 0;
                    }
                }

                //Sorting.. - 색상, 종류
                ALLGUIDE_LIST[_value].GUIDE_LIST.Sort(delegate (GUIDE_ITEM a, GUIDE_ITEM b)
                {
                    if (a.COLOR > b.COLOR) return 1;
                    else if (a.COLOR < b.COLOR) return -1;
                    else
                    {
                        if (a.ID > b.ID) return 1;
                        else if (a.ID < b.ID) return -1;
                    }
                    return 0;
                });

                _value++;
            }
        }

        // ======================= 블럭 인식 정답 ======================= //

        /// <summary>
        /// 인식된 도안의 모든 칸 데이터 호출 함수
        /// </summary>
        /// <param name="_fileName">DepthData에서 사용되는 엘리멘트 명칭 = 도안 트래커 명칭</param>
        public void LoadMapData(string _fileName)
        {
            if (_fileName == "") return;
            coordInfo.CoordList.Clear();

            //Debug.Log(_fileName);
            TextAsset txtAsset = (TextAsset)Resources.Load("XML/" + MAP_DATA_FILE);

            XmlDocument xmlDoc = new XmlDocument();
            //Debug.Log(txtAsset.text);
            xmlDoc.LoadXml(txtAsset.text);

            //Debug.Log("mapdata/" + _fileName);
            // 전체 아이템 가져오기 예제.(Stage 별 대기블록 정보 추출 - Duck + _ + 0(random Select))
            XmlNodeList node = xmlDoc.SelectNodes("mapdata/" + _fileName);

            //Debug.Log(node.Item(0).InnerText);
            string[] xmlInfo = node.Item(0).InnerText.Split('/');

            char[] guide = xmlInfo[0].ToCharArray();
            for (int i = 0; i < guide.Length; i++)
            {
                coordInfo.CoordList.Add(guide[i]);
            }

            string[] pos = xmlInfo[1].Split('x');
            coordInfo.x = int.Parse(pos[0]);
            coordInfo.y = int.Parse(pos[1]);

            coordInfo.minigame = (MINIGAME_LIST.Management + int.Parse(xmlInfo[2]));
        }

        /// <summary>
        /// 인식된 도안의 모든 칸의 색상 확인 함수
        /// </summary>
        /// <param name="_fileName">MarkerData에서 사용되는 엘리멘트 명칭 = 도안 트래커 명칭</param>
        /// <returns>인식 도안의 각칸의 블록의 색상 반환</returns>
        public int[,] LoadMarkerData(string _fileName)
        {
            if (_fileName == "") return null;

            TextAsset txtAsset = (TextAsset)Resources.Load("XML/" + MARKER_DATA_FILE);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(txtAsset.text);

            // 전체 아이템 가져오기 예제.(Stage 별 대기블록 정보 추출 - Duck + _ + 0(random Select))
            XmlNodeList node = xmlDoc.SelectNodes("markerdata/" + _fileName);

            int[,] result = new int[8, 8];
            string[] gridLine = node.Item(0).InnerText.Split('/');
            string output = "";
            for (int i = 0; i < gridLine.Length; i++)
            {
                char[] element = gridLine[i].ToCharArray();
                for (int j = 0; j < element.Length; j++)
                {
                    result[i, j] = element[j] - '0';
                    output += result[i, j].ToString();
                }
                output += "\n";
            }

            // Debug.Log("**************** ANSWER ****************\n" + output);

            return result;
        }

        // ======================= 미니게임 캐릭터 정보 ======================= //

        /// <summary>
        /// 리소스파일에 들어있는 모델 데이터 호출 함수
        /// </summary>
        /// <param name="_fileName">LiveGameInfo에서 사용되는 엘리멘트 명칭 = 도안 트래커 명칭</param>
        /// <returns></returns>
        public LiveGameManager.LiveGamePrefab LoadLiveData(string _fileName)
        {
            if (_fileName == "") return null;

            // 프리팹 정보 저장용 클래스 생성 및 메모리할당
            LiveGameManager.LiveGamePrefab prefab = new LiveGameManager.LiveGamePrefab();

            // LiveGameInfo.xml에 해당 정보 검색
            TextAsset txtAsset = (TextAsset)Resources.Load("XML/" + LIVEGAME_DATA_FILE);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(txtAsset.text);

            // 전체 아이템 가져오기
            //Debug.Log("Character Data : " + _fileName);
            // _fileName과 동일한 명칭을 가진 모든 노드를 검색하기 위한 노드리스트 변수 생성
            XmlNodeList all_nodes = xmlDoc.SelectNodes("inforoot/" + _fileName);

            // 가져온 정보들을 하나씩 탐색
            foreach (XmlNode node in all_nodes)
            {
                // 각 노드의 엘리멘트 확인
                int index = 0;
                foreach (XmlElement element in node)
                {
                    switch (index)
                    {
                        case 0:
                            // 각 애니블록의 초기 회전값 확인
                            prefab.i_Rot = int.Parse(element.InnerText);
                            //Debug.Log(prefab.i_Rot);
                            break;
                        case 1:
                            // 각 애니블록의 3D 모델 프리팹 확인
                            prefab.p_Body = (GameObject)Resources.Load(element.InnerText);
                            //Debug.Log(prefab.p_Body);
                            break;
                    }

                    // 모든 엘리멘트는 3개만 존재함으로 3개 이상 검색하면 break
                    index++;
                    if (index > 2) break;
                }
            }

            return prefab;
        }

        // ======================= FUNCTION ======================= //

        private WAITBLOCK_NUM DefineID(string element)
        {
            switch (element)
            {
                case "AM1":
                    return WAITBLOCK_NUM.AM1;
                case "D1":
                    return WAITBLOCK_NUM.D1;
                case "P3":
                case "TE1":
                    return WAITBLOCK_NUM.TE1;
                case "TE2":
                    return WAITBLOCK_NUM.TE2;
                case "TE3":
                    return WAITBLOCK_NUM.TE3;
                case "TE4":
                    return WAITBLOCK_NUM.TE4;
                case "TE5":
                    return WAITBLOCK_NUM.TE5;
                case "TE6":
                    return WAITBLOCK_NUM.TE6;
                case "TE7":
                    return WAITBLOCK_NUM.TE7;
                case "TR1":
                    return WAITBLOCK_NUM.TR1;
            }
            return WAITBLOCK_NUM.TR2;
        }

        private WAITBLOCK_MATCOLOR DefineColor(string element)
        {
            switch (element)
            {
                case "YELLOW":
                    return WAITBLOCK_MATCOLOR.YELLOW;
                case "ORANGE":
                    return WAITBLOCK_MATCOLOR.ORANGE;
                case "GREEN":
                    return WAITBLOCK_MATCOLOR.GREEN;
                case "BLUE":
                    return WAITBLOCK_MATCOLOR.BLUE;
                case "RED":
                    return WAITBLOCK_MATCOLOR.RED;
                case "BROWN":
                    return WAITBLOCK_MATCOLOR.BROWN;
                case "WHITE":
                    return WAITBLOCK_MATCOLOR.WHITE;
                case "BLACK":
                    return WAITBLOCK_MATCOLOR.BLACK;
                case "GRAY":
                    return WAITBLOCK_MATCOLOR.GRAY;
                case "NAVY":
                    return WAITBLOCK_MATCOLOR.NAVY;
                case "SKYBLUE":
                    return WAITBLOCK_MATCOLOR.SKYBLUE;
                case "HOTPINK":
                    return WAITBLOCK_MATCOLOR.HOTPINK;
            }

            return WAITBLOCK_MATCOLOR.DEEPBLUE;
        }
    }
}