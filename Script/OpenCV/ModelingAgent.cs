//=================================================
//================MODELING SYSTEM==================
//================Made By NAHUNKIM=================
//=================================================
using System.Collections;
using System.Collections.Generic;
// using OpenCvSharp;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using OpenCVForUnityExample;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.UnityUtils;
using System.IO;

public struct RoI
{
    public int _left, _upper, _right, _lower, _numul, _numlr;

    public RoI(int left, int lower, int right, int upper, int numul, int numlr)
    {
        _left = left;
        _upper = upper;
        _right = right;
        _lower = lower;
        _numul = numul;
        _numlr = numlr;
    }
}

public static class ERRTYPE
{
    public const int ERR_DETECTION_OBJECT = 0;
    public const int ERR_TEST = 1;
    public static string TypeToString(int type)
    {
        switch (type)
        {
            case 0:
                return "ERR_DETECTION_OBJECT";
            case 1:
                return "ERR_TEST";
            default:
                return "ERR";
        }
    }
}

class ModelingException : Exception
{
    public static GameObject ModelingPanel = null, MainPanel = null;
    public ModelingException()
    {
        ModelingPanel = GameObject.Find("ModelingPanel");
        MainPanel = GameObject.Find("MainPanel");
    }
    public ModelingException(int errtype) : base(ERRTYPE.TypeToString(errtype))
    {
        ERRCODE = errtype;
    }
    public void ProcessException()
    {
        switch (ERRCODE)
        {
            case 0:
                UnityEngine.Debug.LogError("ERROR CODE 0");
                break;
            case 1:
                UnityEngine.Debug.LogError("ERROR CODE 1");
                break;
        }
        UnityEngine.Debug.Log(base.Message);
    }
    public int ERRCODE { get; set; }
}

public class ModelingAgent : MonoBehaviour
{
    [SerializeField] private GameObject DrawPanel = null;
    [SerializeField] private GameObject Block_AM1 = null;
    [SerializeField] private Texture BlockMaterial = null;

    [SerializeField] private GameObject[] DrawAM1 = new GameObject[64];

    List<Feature> training_feature_list = null, test_feature_list = null;
    KNN_Classifier knn_classifier = null;
    Color_Histogram_Feature_Extraction color_histogram_feature_extraction = null;

    Manager.Util.XmlManager _xmlManager = null;
    Manager.Util.PackageManager _PackageManager = null;

    private List<char> grid;
    bool isAlgirhtm;


    // Use this for initialization
    private void Start()
    {
        // XML 매니저 호출
        try
        {
            _xmlManager = Manager.Util.XmlManager.S;
            _PackageManager = Manager.Util.PackageManager.Instance;
        }
        catch (System.NullReferenceException e)
        {
            UnityEngine.Debug.LogError("XML Component is NULL");
        }

        training_feature_list = new List<Feature>();
        knn_classifier = new KNN_Classifier();

        knn_classifier.LoadTrainingDataset("training.data", training_feature_list);

        color_histogram_feature_extraction = new Color_Histogram_Feature_Extraction();

    }

    public void InitDrawPanel()
    {
        DrawAM1 = new GameObject[64];

        int i = 0;
        foreach (Transform child in DrawPanel.transform)
        {
            DrawAM1[i] = child.gameObject;
            i++;
        }

        if (DrawPanel != null) return;

        DrawPanel = GameObject.Find("DrawPanel");
        DrawPanel.transform.localPosition = new Vector3(0, 0, -700);
    }

    public void InitGrid()
    {
        grid = new List<char>(64);
        for (int i = 0; i < 64; i++) grid.Add('0');
    }

    public void SaveData()
    {
        if (save_data != "")
        {
            PlayerPrefs.SetString(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, save_data);
            UnityEngine.Debug.Log(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            UnityEngine.Debug.Log(save_data);

            LoadMaster.S.LoadSceneFunc("Scene_World");
        }
    }

    void BuildModel(Hashtable args)
    {
        Mat BITIMG = Imgcodecs.imread(PATH.GetOnResources("BITIMG.png"), CvType.CV_16UC1);
        isAlgirhtm = (bool)args["draw"];
        //UnityEngine.Debug.Log(isAlgirhtm);
        try
        {

            /*declare - RGB/DEPTH*/
            int size = 2; // for resizing
            Mat mmInputImg = Imgcodecs.imread(PATH.GetOnResources(args["inputImg"].ToString())); // unitytest.jpg
            Mat mmDepthmap = Imgcodecs.imread(PATH.GetOnResources(args["outputDepth"].ToString())); // unitytest.png

            /*Resizing*/
            if (mmInputImg.size() != new Size(320, 240)) Imgproc.resize(mmInputImg, mmInputImg, new Size(320, 240)); //if Size(1:1)

            PreProcessor.Reszie(mmInputImg, mmInputImg, new Size(mmInputImg.width() / size, mmInputImg.height() / size));
            PreProcessor.Reszie(mmDepthmap, mmDepthmap, new Size(mmDepthmap.width() / size, mmDepthmap.height() / size));
            /*end - Resizing*/

            /*declare - grid*/
            RoI roi = new RoI();
            int diff_ul = -1, diff_lr = -1;
            /*end - declare*/

            /*Medain Blur - RGB*/
            PreProcessor.MedianBlur(mmInputImg, mmInputImg, 3);
            /*end - Median Blur*/

            /*Extract Object - Depth*/
            PreProcessor.CvtColor(mmDepthmap, mmDepthmap, Imgproc.COLOR_BGR2GRAY);
            PreProcessor.MedianBlur(mmDepthmap, mmDepthmap, 3);
            PreProcessor.Threshold(mmDepthmap, mmDepthmap, 160, 255, Imgproc.THRESH_BINARY_INV); // T = 100 => 160
            PreProcessor.Morphology(mmDepthmap, mmDepthmap, Imgproc.MORPH_CLOSE, Mat.ones(new Size(3, 3), CvType.CV_8UC1), new Point(-1, -1), 3);
            /*end - Extract Object*/

            /*Obtain Grid*/
            PreProcessor.GetGrid(mmDepthmap, mmDepthmap, ref roi, ref diff_ul, ref diff_lr);
            /*end - Obtain Grid*/

            Mat SubMat = mmDepthmap.clone(), Erosion = mmDepthmap.clone();
            Imgproc.erode(Erosion, Erosion, Mat.ones(3, 3, CvType.CV_8UC1));
            for (int i = 0; i < SubMat.height(); i++)
            {
                for (int j = 0; j < SubMat.width(); j++)
                {
                    if (SubMat.get(i, j)[0] != Erosion.get(i, j)[0]) SubMat.put(i, j, 255);
                    else SubMat.put(i, j, 0);
                }
            }

            /*Correction - Distortion*/
            Point[] points = new Point[4];
            PreProcessor.ResetPointArray(ref points);
            PreProcessor.GetPoints(SubMat, ref points);
            Point[] intersection_p = PreProcessor.GetRectanglePoints(SubMat, points, 360, 5);
            Point[] correction_p = { new Point(0, 0), new Point(SubMat.width(), 0), new Point(0, SubMat.height()), new Point(SubMat.width(), SubMat.height()) };
            MatOfPoint2f intersection_m = new MatOfPoint2f(intersection_p);
            MatOfPoint2f correction_m = new MatOfPoint2f(correction_p);
            Mat matrix = PreProcessor.GetPerspectiveTransform(intersection_m, correction_m);
            
            PreProcessor.WarpPerspective(mmInputImg, mmInputImg, matrix, mmInputImg.size());
            PreProcessor.WarpPerspective(mmDepthmap, mmDepthmap, matrix, mmDepthmap.size());

            //char[,] grid = new char[8, 8];
            if(isAlgirhtm)
                for (int i = 0; i < 64; i++) grid[i] = '0';

            DrawBlocks(mmInputImg, mmDepthmap, roi, diff_ul, diff_lr);
            /*end - Correction*/
        }
        catch (ModelingException e)
        {
            e.ProcessException();

            if (isAlgirhtm)
            {
                PlayerPrefs.SetInt("SAVE_TEXTURE_RATE" + PlayerPrefs.GetInt("SAVE_TEXTURE"), 0);
                Manager.RescueManager.S.MinigameCheck(0.1f);
            }
            return;
        }

        ////================End Modeling======================
    }

    ThresholdExample threshold = new ThresholdExample();
    string save_data = "";

    void DrawBlocks(Mat rgb, Mat depth, RoI roi, int diff_ul, int diff_lr)
    {
        /*declare - for making block variances*/
        int BlockMakeCount = 0, x = roi._left, y = roi._upper;
        double[] buf;
        /*end - declare*/

        /*declare - Color Lookup Table Class*/
        BlockColor bc = new BlockColor();
        /*end - declare*/

        /*Check - each grid*/

        int num_ul = roi._numul, num_lr = roi._numlr;
        //UnityEngine.Debug.Log("roi._numul : " + roi._numul + " / roi._numlr : " + roi._numlr);
        for (int i = y; i <= roi._lower - diff_ul; i += diff_ul)
        {
            num_lr = roi._numlr; // diff_ul_lr = Math.Abs(diff_ul - diff_lr);
            int diff_b_ul = Math.Abs(diff_ul - 15), diff_b_lr = Math.Abs(diff_lr - 15);
            for (int j = x; j <= roi._right - diff_lr; j += diff_lr)
            {
                Color color = new Color(0, 0, 0, 0);
                /*Check - White Pixel in DepthMap*/
                for (int subi = i; subi < i + diff_ul; subi++)
                {
                    for (int subj = j; subj < j + diff_lr; subj++)
                    {
                        buf = depth.get(subi, subj);
                        // if (depth.At<byte>(subi, subj) == 255)
                        if (buf[0] == 255)
                        {
                            BlockMakeCount++;
                        }
                    }
                }
                /*end - Check*/

                /*Check - Is it possible to make Block*/
                if ((float)BlockMakeCount > (float)((diff_ul * diff_lr) * 0.5)) // 0.7 => 0.65 => 0.8 => 0.7
                {
                    Mat each_grid = Mat.zeros(new Size(diff_lr, diff_ul), CvType.CV_8UC3);
                    //Mat each_grid = Mat.zeros(new Size(1, 1), CvType.CV_8UC3);
                    for (int h = 0, subi = i; h < each_grid.rows() && subi < i + diff_ul; h++, subi++)
                    {
                        for (int w = 0, subj = j; w < each_grid.cols() && subj < j + diff_lr; w++, subj++)
                        {
                            each_grid.put(h, w, new double[] { rgb.get(subi, subj)[0], rgb.get(subi, subj)[1], rgb.get(subi, subj)[2] });
                        }
                    }

                    //Imgcodecs.imwrite(string.Format("each_grid{0}.jpg",cnt), each_grid);//확인용.

                    // Color_Histogram_Feature_Extraction color_histogram_feature_extraction = new Color_Histogram_Feature_Extraction();

                    test_feature_list = color_histogram_feature_extraction.Color_Histogram_Of_Test_Image(each_grid);

                    // KNN_Classifier knn_classifier = new KNN_Classifier();
                    // List<string> results = knn_classifier.KNN_Main("training", "test");
                    List<string> results = knn_classifier.KNN_Main(training_feature_list, test_feature_list);



                    int matching_color = 0;
                    switch (results[0])
                    {
                        case "red":
                            matching_color = 0;
                            break;
                        case "orange":
                            matching_color = 1;
                            break;
                        case "yellow":
                            matching_color = 2;
                            break;
                        case "green":
                            matching_color = 3;
                            break;
                        case "blue":
                            matching_color = 4;
                            break;
                        case "brown":
                            matching_color = 5;
                            break;
                        case "white":
                            matching_color = 6;
                            break;
                        case "black":
                            matching_color = 7;
                            break;
                    }

                    int pos = (8 * (roi._numul - num_ul)) + (roi._numlr - num_lr);

                    if (isAlgirhtm)
                    {
                        //grid[(num_ul - 1) + (8 * (8 - num_lr))] = '1'; // 오른쪽 90도 회전
                        grid[pos] = '1'; // 찍은대로 표시
                    }
                    else
                    {
                        save_data += matching_color.ToString() + "x" + pos.ToString() + "/";

                        //GameObject cube = Instantiate(Block_AM1, DrawPanel.transform);
                        //MeshRenderer meshrenderer = cube.GetComponent<MeshRenderer>();
                        Color c = new Color(bc.Get_R(matching_color) / 255.0f, bc.Get_G(matching_color) / 255.0f, bc.Get_B(matching_color) / 255.0f);
                        //meshrenderer.material.SetColor("_Color", c);
                        //cube.transform.localPosition = new Vector3(0, i - (diff_b_ul * (roi._numul - num_ul)) - 50, j - (diff_b_lr * (roi._numlr - num_lr)) - 50);
                        //cube.transform.localScale = new Vector3(59, 59, 59);

                        DrawAM1[pos].SetActive(true); // 찍은대로 표시
                        DrawAM1[pos].GetComponent<MeshRenderer>().material.SetColor("_Color", c);
                    }
                }

                /*Reset*/
                num_lr--;
                BlockMakeCount = 0;
                bc.Reset_Count();
                /*end - Reset*/
            }
            num_ul--;
        }
        /*end - Check*/

        if (isAlgirhtm) // Rescue씬에서 도안인식
        {
            UnityEngine.Debug.Log(
            "=========LIST OF GRID=========\n" +
            grid[0] + grid[1] + grid[2] + grid[3] + grid[4] + grid[5] + grid[6] + grid[7] + "\n" +
            grid[8] + grid[9] + grid[10] + grid[11] + grid[12] + grid[13] + grid[14] + grid[15] + "\n" +
            grid[16] + grid[17] + grid[18] + grid[19] + grid[20] + grid[21] + grid[22] + grid[23] + "\n" +
            grid[24] + grid[25] + grid[26] + grid[27] + grid[28] + grid[29] + grid[30] + grid[31] + "\n" +
            grid[32] + grid[33] + grid[34] + grid[35] + grid[36] + grid[37] + grid[38] + grid[39] + "\n" +
            grid[40] + grid[41] + grid[42] + grid[43] + grid[44] + grid[45] + grid[46] + grid[47] + "\n" +
            grid[48] + grid[49] + grid[50] + grid[51] + grid[52] + grid[53] + grid[54] + grid[55] + "\n" +
            grid[56] + grid[57] + grid[58] + grid[59] + grid[60] + grid[61] + grid[62] + grid[63] + "\n"
            );

            float result = ((float)((int)(CompareGuide() * 100)) / 100);
            UnityEngine.Debug.Log(result);
            if (result == 0) result = 0.1f;

            PlayerPrefs.SetInt("SAVE_TEXTURE_RATE" + PlayerPrefs.GetInt("SAVE_TEXTURE"), (int)result);
            Manager.RescueManager.S.MinigameCheck(result);
        }
        else // CutScend에서 도안인식
        {
            CustomARCameraManager.Instance.DisableCamera();
        }
    }

    // 정답과 인식한 도안의 위치를 비교하여 정답비율 반환
    float CompareGuide()
    {
        List<char> answer = _xmlManager.GetCoord().CoordList;
        int x = _xmlManager.GetCoord().x; // char를 int로 변환. *ASCII값의 48부터 '0'
        int y = _xmlManager.GetCoord().y;
        float total, compare; // 24
        total = compare = x * y;

        string guide = "";
        for (int col = 0; col < y; col++) // cup 6 4 = 26
        {
            for(int row = 0; row < x; row++)
            {
                int pos = (x * col) + row;
                int pos2 = (8 * col) + row;

                try
                {
                    guide += answer[pos];
                    if (answer[pos] != grid[pos2])
                    {
                        --compare;
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Error pos1 : " + pos + ", (" + x + ", " + y + ")");
                    UnityEngine.Debug.LogError("Error pos2 : " + pos2 + ", (" + row + ", " + col + ")");
                }
            }
            guide += "\n";
        }

        UnityEngine.Debug.Log("-------------ANSWER-------------\n" + guide);

        return ((compare / total) * 100);
    }
}


//=================================================
//=========Image Processing For Modeling===========
//================Made By NAHUNKIM=================
//=================================================
public static class PreProcessor
{
    public static void Morphology(Mat src, Mat dst, int op, Mat kernel, Point anchor, int iterations)
    {
        Imgproc.morphologyEx(src, dst, op, kernel, anchor, iterations);
    }

    public static void Reszie(Mat src, Mat dst, Size size)
    {
        Imgproc.resize(src, dst, size);
    }

    public static void CvtColor(Mat src, Mat dst, int code, int dstCn = 0)
    {
        Imgproc.cvtColor(src, dst, code);
    }

    public static void MedianBlur(Mat src, Mat dst, int ksize)
    {
        Imgproc.medianBlur(src, dst, ksize);
    }

    public static void Threshold(Mat src, Mat dst, double thresh, double maxval, int type)
    {
        Imgproc.threshold(src, dst, thresh, maxval, type);
    }

    public static void GetGrid(Mat src, Mat dst, ref RoI roi, ref int diff_ul, ref int diff_lr)
    {
        int upper = 0, lower = 0, left = 0, right = 0;
        double buf = 0;

        for (int i = 0; i < src.height(); i++)
        {
            for (int j = 0; j < src.width(); j++)
            {
                buf = src.get(i, j)[0];
                if (buf == 255)
                {
                    upper = i;
                    i = src.height();
                    break;
                }
            }
        }
        for (int i = src.height() - 1; i >= 0; i--)
        {
            for (int j = src.width() - 1; j >= 0; j--)
            {
                buf = src.get(i, j)[0];
                if (buf == 255)
                {
                    lower = i;
                    i = -1;
                    break;
                }
            }
        }

        for (int j = 0; j < src.width(); j++)
        {
            for (int i = 0; i < src.height(); i++)
            {
                buf = src.get(i, j)[0];
                if (buf == 255)
                {
                    left = j;
                    j = src.width();
                    break;
                }
            }
        }
        for (int j = src.width() - 1; j >= 0; j--)
        {
            for (int i = src.height() - 1; i >= 0; i--)
            {
                buf = src.get(i, j)[0];
                if (buf == 255)
                {
                    right = j;
                    j = -1;
                    break;
                }
            }
        }

        int adata1 = SpaceCalculator(upper) - 1;
        int adata2 = SpaceCalculator(lower) - 1;
        int bdata1 = SpaceCalculator(left) - 2;
        int bdata2 = SpaceCalculator(right) - 2;

        int diff_div_ul = Mathf.Abs(lower - upper) / 13, diff_div_lr = Mathf.Abs(right - left) / 13;
        if (diff_div_lr == 0 || diff_div_ul == 0) throw new ModelingException(ERRTYPE.ERR_DETECTION_OBJECT);

        if (diff_div_ul > 8)
        {
            diff_div_ul = 8;
        }
        if (diff_div_lr > 8)
        {
            diff_div_lr = 8;
        }
        diff_ul = (int)((src.height() / diff_div_ul) + 0.5);
        diff_lr = (int)((src.width() / diff_div_lr) + 0.5);
        upper = left = 0;
        lower = src.height();
        right = src.width();

        dst = src;
        roi = new RoI(left, lower, right, upper, diff_div_ul, diff_div_lr);
    }

    private static int SpaceCalculator(int direction)
    {
        return (int)Mathf.Round(direction / 15 + 0.5f);
    }

    private static void Swap(ref int num1, ref int num2)
    {
        int tmp = num1;
        num1 = num2;
        num2 = tmp;
    }

    public static void GetPoints(Mat src, ref Point[] points)
    {
        double buf = 0;

        for (int i = 0; i < src.height(); i++)
        {
            for (int j = 0; j < src.width(); j++)
            {
                buf = src.get(i, j)[0];
                if (buf == 255)
                {
                    // upper = i;
                    points[0].y = i;
                    points[0].x = j;
                    i = src.height();
                    break;
                }
            }
        }
        for (int i = src.height() - 1; i >= 0; i--)
        {
            for (int j = src.width() - 1; j >= 0; j--)
            {
                buf = src.get(i, j)[0];
                if (buf == 255)
                {
                    // lower = i;
                    points[1].y = i;
                    points[1].x = j;
                    i = -1;
                    break;
                }
            }
        }
        for (int j = 0; j < src.width(); j++)
        {
            for (int i = 0; i < src.height(); i++)
            {
                buf = src.get(i, j)[0];
                if (buf == 255)
                {
                    // left = j;
                    points[2].y = i;
                    points[2].x = j;
                    j = src.width();
                    break;
                }
            }
        }
        for (int j = src.width() - 1; j >= 0; j--)
        {
            for (int i = src.height() - 1; i >= 0; i--)
            {
                buf = src.get(i, j)[0];
                if (buf == 255)
                {
                    // right = j;
                    points[3].y = i;
                    points[3].x = j;
                    j = -1;
                    break;
                }
            }
        }
    }

    private static void CountLinePoints(Mat src, int rho, int ang, int num_rho, int num_ang, ref int[,] arr)
    {
        int x, y, r, a;
        int w = src.width();
        int h = src.height();
        double buf = 0;

        // 수직선인 경우
        if (ang == 90)
        {
            x = (int)(rho + 0.5);
            for (y = 0; y < h; y++)
            {
                buf = src.get(y, x)[0];
                if (buf == 255)
                {
                    r = rho + (num_rho / 2);
                    a = (int)(ang * num_ang / 180.0d);
                    arr[r, a]++;
                }
            }
            return;
        }

        // (rho, ang) 파라미터를 이용하여 직선의 시작 좌표와 끝 좌표를 계산
        int x1 = 0;
        int y1 = (int)Math.Floor(rho / Math.Cos(ang * Math.PI / 180) + 0.5);
        int x2 = src.width() - 1;
        int y2 = (int)Math.Floor((rho - x2 * Math.Sin(ang * Math.PI / 180)) / Math.Cos(ang * Math.PI / 180) + 0.5);
        CountLinePoints(src, x1, y1, x2, y2, rho, ang, num_rho, num_ang, ref arr);
    }

    private static void CountLinePoints(Mat src, int x1, int y1, int x2, int y2, int rho, int ang, int num_rho, int num_ang, ref int[,] arr)
    {
        int x, y, r, a;
        double m;
        int w = src.width();
        int h = src.height();
        double buf;

        // 수직선인 경우
        if (x1 == x2)
        {
            if (y1 > y2) Swap(ref y1, ref y2);
            for (y = y1; y <= y2; y++)
            {
                buf = src.get(y, x1)[0];
                // src.Set<byte>(y, x1, 255);
                if (buf == 255)
                {
                    r = rho + (num_rho / 2);
                    a = (int)(ang * num_ang / 180.0d);
                    arr[r, a]++;
                }
            }
            return;
        }

        // (x1, y1)에서 (x2, y2)까지 직선 그리기
        m = (double)(y2 - y1) / (x2 - x1);
        if ((m > -1) && (m < 1))
        {
            if (x1 > x2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }
            for (x = x1; x <= x2; x++)
            {
                y = (int)Math.Floor(m * (x - x1) + y1 + 0.5);
                // if (y >= 0 && y < h) src.Set<byte>(y, x, 255);
                if (y >= 0 && y < h)
                {
                    buf = src.get(y, x)[0];
                    if (buf == 255)
                    {
                        r = rho + (num_rho / 2);
                        a = (int)(ang * num_ang / 180.0d);
                        arr[r, a]++;
                    }
                }
            }
        }
        else
        {
            if (y1 > y2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }
            for (y = y1; y <= y2; y++)
            {
                x = (int)Math.Floor((y - y1) / m + x1 + 0.5);
                // if (y >= 0 && y < h) src.Set<byte>(y, x, 255);
                if (y >= 0 && y < h)
                {
                    buf = src.get(y, x)[0];
                    if (buf == 255)
                    {
                        r = rho + (num_rho / 2);
                        a = (int)(ang * num_ang / 180.0d);
                        arr[r, a]++;
                    }
                }
            }
        }
    }

    private static Point[] GetIntersectionPoints(Mat src, ref LineParam lines)
    {
        int w = src.width(), h = src.height();
        Point[] p = new Point[8], intersection_p = new Point[4];
        ResetPointArray(ref p);
        ResetPointArray(ref intersection_p);

        for (int i = 0; i < lines.RhoLength(); i++)
        {
            if (lines.GetAng(i) == 90)
            {
                p[i * 2].x = p[i * 2 + 1].x = (int)(lines.GetRho(i) + 0.5);
                p[i * 2].y = 0;
                p[i * 2 + 1].y = h - 1;
                continue;
            }

            // (rho, ang) 파라미터를 이용하여 직선의 시작 좌표와 끝 좌표를 계산
            p[i * 2].x = 0;
            p[i * 2].y = (int)Math.Floor(lines.GetRho(i) / Math.Cos(lines.GetAng(i) * Math.PI / 180) + 0.5);
            p[i * 2 + 1].x = w - 1;
            p[i * 2 + 1].y = (int)Math.Floor((lines.GetRho(i) - p[i * 2 + 1].x * Math.Sin(lines.GetAng(i) * Math.PI / 180)) / Math.Cos(lines.GetAng(i) * Math.PI / 180) + 0.5);
        }

        for (int i = 0; i < 2; i++)
        {
            double t, s;
            double under = (p[5].y - p[4].y) * (p[i * 2 + 1].x - p[i * 2].x) - (p[5].x - p[4].x) * (p[i * 2 + 1].y - p[i * 2].y);
            if (under == 0) intersection_p[i * 2] = new Point(0, 0);

            double _t = (p[5].x - p[4].x) * (p[i * 2].y - p[4].y) - (p[5].y - p[4].y) * (p[i * 2].x - p[4].x);
            double _s = (p[i * 2 + 1].x - p[i * 2].x) * (p[i * 2].y - p[4].y) - (p[i * 2 + 1].y - p[i * 2].y) * (p[i * 2].x - p[4].x);

            t = _t / under;
            s = _s / under;

            if (t < 0 || t > 1 || s < 0 || s > 1) intersection_p[i * 2] = new Point(0, 0);
            if (_t == 0 && _s == 0) intersection_p[i * 2] = new Point(0, 0);

            intersection_p[i * 2].x = (int)(p[i * 2].x + t * (double)(p[i * 2 + 1].x - p[i * 2].x));
            intersection_p[i * 2].y = (int)(p[i * 2].y + t * (double)(p[i * 2 + 1].y - p[i * 2].y));

            under = (p[7].y - p[6].y) * (p[i * 2 + 1].x - p[i * 2].x) - (p[7].x - p[6].x) * (p[i * 2 + 1].y - p[i * 2].y);
            if (under == 0) intersection_p[i * 2] = new Point(0, 0);

            _t = (p[7].x - p[6].x) * (p[i * 2].y - p[6].y) - (p[7].y - p[6].y) * (p[i * 2].x - p[6].x);
            _s = (p[i * 2 + 1].x - p[i * 2].x) * (p[i * 2].y - p[6].y) - (p[i * 2 + 1].y - p[i * 2].y) * (p[i * 2].x - p[6].x);

            t = _t / under;
            s = _s / under;

            if (t < 0 || t > 1 || s < 0 || s > 1) intersection_p[i * 2] = new Point(0, 0);
            if (_t == 0 && _s == 0) intersection_p[i * 2] = new Point(0, 0);

            intersection_p[i * 2 + 1].x = (int)(p[i * 2].x + t * (double)(p[i * 2 + 1].x - p[i * 2].x));
            intersection_p[i * 2 + 1].y = (int)(p[i * 2].y + t * (double)(p[i * 2 + 1].y - p[i * 2].y));
        }
        return intersection_p;
    }

    public static Point[] GetRectanglePoints(Mat src, Point[] points, int ang, int range)
    {
        // HoughTransform을 위한 기본설정
        int i, j, m, n;
        int w = src.width();
        int h = src.height();
        int num_rho = (int)(Math.Sqrt((double)w * w + h * h) * 2);
        int num_ang = ang;
        int ratio = ang / 180;
        int max = 0, max_rho = 0, max_ang = 0;
        LineParam lines = new LineParam();

        // 0 ~ PI 각도에 해당하는 sin, cos 함수의 값을 룩업테이블에 저장
        double[] tsin = new double[num_ang];
        double[] tcos = new double[num_ang];
        for (i = 0; i < num_ang; i++)
        {
            tsin[i] = (double)Math.Sin(i * Math.PI / num_ang);
            tcos[i] = (double)Math.Cos(i * Math.PI / num_ang);
        }

        // 축적 배열(Accumulate Array) 생성
        int[,] arr = new int[num_rho, num_ang];

        // for (i = 0; i < points.Length; i++)
        for (i = 0; i < points.Length; i++)
        {
            switch (i)
            {
                case 0:
                case 1:
                    for (int theta = 179 * ratio - range; theta <= 179 * ratio; theta++)
                    {
                        m = (int)Math.Floor(points[i].x * tsin[theta] + points[i].y * tcos[theta] + 0.5);
                        n = (int)(theta * 180.0d / num_ang);
                        // DrawLine(src, m, n);
                        CountLinePoints(src, m, n, num_rho, num_ang, ref arr);
                    }
                    for (int theta = 0; theta <= ratio * range; theta++)
                    {
                        m = (int)Math.Floor(points[i].x * tsin[theta] + points[i].y * tcos[theta] + 0.5);
                        n = (int)(theta * 180.0d / num_ang);
                        // DrawLine(src, m, n);
                        CountLinePoints(src, m, n, num_rho, num_ang, ref arr);
                    }
                    break;
                case 2:
                case 3:
                    for (int theta = 90 * ratio - range; theta <= 90 * ratio + range; theta++)
                    {
                        m = (int)Math.Floor(points[i].x * tsin[theta] + points[i].y * tcos[theta] + 0.5);
                        n = (int)(theta * 180.0d / num_ang);
                        // DrawLine(src, m, n);
                        CountLinePoints(src, m, n, num_rho, num_ang, ref arr);
                    }
                    break;
            }

            for (int subi = 0; subi < num_rho; subi++)
            {
                for (int subj = 0; subj < num_ang; subj++)
                {
                    if (max < arr[subi, subj])
                    {
                        max = arr[subi, subj];
                        max_rho = subi;
                        max_ang = subj;
                    }
                }
            }
            lines.AddRho(max_rho - (num_rho / 2));
            lines.AddAng(max_ang * 180.0d / num_ang);
            for (int subi = 0; subi < num_rho; subi++)
            {
                for (int subj = 0; subj < num_ang; subj++)
                {
                    arr[subi, subj] = 0;
                }
            }
            max = 0;
        }
        Mat copy = src.clone(); ///////////////////////////////////////////////////////////////////
        for (int k = 0; k < lines.RhoLength(); k++)
        {
            DrawLine(copy, lines.GetRho(k), lines.GetAng(k));
        }
        Point[] Ipoints = GetIntersectionPoints(src, ref lines);
        for (int k = 0; k < Ipoints.Length; k++)
        {
            Imgproc.circle(copy, Ipoints[k], 10, new Scalar(255, 255, 255));
        }
        Imgcodecs.imwrite(PATH.GetOnResources("CorrectionLines.png"), copy);

        return GetIntersectionPoints(src, ref lines);
    }

    public static Mat GetPerspectiveTransform(Mat src, Mat dst)
    {
        return Imgproc.getPerspectiveTransform(src, dst);
    }

    public static void WarpPerspective(Mat src, Mat dst, Mat matrix, Size size)
    {
        Imgproc.warpPerspective(src, dst, matrix, size);
    }

    public static void ResetPointArray(ref Point[] p)
    {
        for (int i = 0; i < p.Length; i++)
        {
            p[i] = new Point(0, 0);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    public static void DrawLine(Mat src, double rho, double ang)
    {
        int x, y;
        int w = src.width();
        int h = src.height();

        // 수직선인 경우
        if (ang == 90)
        {
            x = (int)(rho + 0.5);
            for (y = 0; y < h; y++)
            {
                // src.Set<byte>(y, x, 255);
                src.put(y, x, 255);
            }
            return;
        }

        // (rho, ang) 파라미터를 이용하여 직선의 시작 좌표와 끝 좌표를 계산
        int x1 = 0;
        int y1 = (int)Math.Floor(rho / Math.Cos(ang * Math.PI / 180) + 0.5);
        int x2 = src.width() - 1;
        int y2 = (int)Math.Floor((rho - x2 * Math.Sin(ang * Math.PI / 180)) / Math.Cos(ang * Math.PI / 180) + 0.5);
        DrawLine(src, x1, y1, x2, y2);
    }

    public static void DrawLine(Mat src, int x1, int y1, int x2, int y2)
    {
        int x, y;
        double m;
        int w = src.width();
        int h = src.height();

        // 수직선인 경우
        if (x1 == x2)
        {
            if (y1 > y2) Swap(ref y1, ref y2);
            for (y = y1; y <= y2; y++)
            {
                // src.Set<byte>(y, x1, 255);
                src.put(y, x1, 255);
            }
            return;
        }

        // (x1, y1)에서 (x2, y2)까지 직선 그리기
        m = (double)(y2 - y1) / (x2 - x1);
        if ((m > -1) && (m < 1))
        {
            if (x1 > x2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }
            for (x = x1; x <= x2; x++)
            {
                y = (int)Math.Floor(m * (x - x1) + y1 + 0.5);
                if (y >= 0 && y < h) // src.Set<byte>(y, x, 255);
                    src.put(y, x, 255);
            }
        }
        else
        {
            if (y1 > y2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }
            for (y = y1; y <= y2; y++)
            {
                x = (int)Math.Floor((y - y1) / m + x1 + 0.5);
                if (y >= 0 && y < h) // src.Set<byte>(y, x, 255);
                    src.put(y, x, 255);
            }
        }
    }
}
