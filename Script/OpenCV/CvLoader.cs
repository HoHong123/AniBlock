//=================================================
//================CvLoader System==================
//===============Made By SangWonKim================
//=================================================
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using OpenCvSharp;
// using OpenCvSharp.Dnn;
using Rect = UnityEngine.Rect;
using UnityEngine.UI;

using System.Diagnostics;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.DnnModule;

public static class PATH
{
    // https://huiyoi.tistory.com/99

    //================Start File Path======================
    /*For Windows exe*/
    /*public static string PROJ { get; } = System.IO.Directory.GetCurrentDirectory();
    public static string ASSETS { get; } = PROJ + @"\Assets\";
    public static string RESOURCES { get; } = ASSETS + @"\Resources\";
    public static string DATA { get; } = PROJ + @"\" + Application.productName + @"_Data\";
    public static string RESOURCES { get; } = DATA + @"\Resources\";*/

    /*For Unity Editor*/
    public static string DATA { get; } = Application.dataPath;
    //public static string RESOURCES { get; } = DATA + @"\Resources\";
    public static string STREAMINGASSETS { get; } = DATA + @"\StreamingAssets\"; // ANDROID

    /*For Android*/
    public static string ENETPATH { get; } = Path.Combine(Application.persistentDataPath, "ENet");

    /*For IOS*/
    public static string IOSPATH { get; } = Application.persistentDataPath;

    //================End File Path======================

    public static string GetOnResources(string file)
    {
#if UNITY_ANDROID
        return Path.Combine(ENETPATH, file); // 안드로이드 모바일
#elif UNITY_IOS
        return Path.Combine(IOSPATH, file); // IOS
#elif UNITY_EDITOR
        return Path.Combine(Application.streamingAssetsPath, file); // 에디터
#endif
    }
}

public static class DIMENSION
{
    public static short INPUT_W { get; } = 513;
    public static short INPUT_H { get; } = 385;

    public static short OUTPUT_W { get; } = 65; // 65
    public static short OUTPUT_H { get; } = 49; // 49
}

public class CvLoader
{
    delegate void _RESIZING(Mat src, Mat dst, Size newSize);
    private readonly _RESIZING Upscaling = (src, dst, size) => Imgproc.resize(src, dst, size);
    private readonly _RESIZING Downscaling = (src, dst, size) => Imgproc.resize(src, dst, size);

    delegate Mat _IMG2TENSOR(Mat img);
    private readonly _IMG2TENSOR LoadTensorFromImage = (Mat img) => Dnn.blobFromImage(img, 1, new Size(0, 0), new Scalar(0, 0, 0, 0), true, true);

    delegate Mat _TENSOR2DEPTH(Mat tensor, int h, int w);
    private readonly _TENSOR2DEPTH LoadDepthFromTensor = (Mat tensor, int h, int w) =>
    {
        /*tensor : [B]x[H]x[W]x[C] = [1]x[49]x[65]x[1] => [B]x[C]x[H]x[W] = [1]x[1]x[49]x[65]*/
        Mat img = new Mat(DIMENSION.OUTPUT_H, DIMENSION.OUTPUT_W, CvType.CV_8UC1);

        /*reshape([C], [C * H])*/
        Mat m_tmp = tensor.reshape(0, h); // 1 channel, 65 width, 49 height
        Mat submat = m_tmp.submat(0, h, 0, w); // 49*65*CV_32FC1
        float[] value = new float[1];
        for (int y = 0; y < submat.height(); y++)
        {
            for (int x = 0; x < submat.width(); x++)
            {
                submat.get(y, x, value);
                if (value[0] > 255 || value[0] < 0) value[0] = 255;
                byte[] newvalue = { (byte)value[0], (byte)value[0], (byte)value[0] };
                img.put(y, x, newvalue);
            }
        }

        return img;
    };

    //private readonly string model = "SangwonKim_Weights.pb";
    //private readonly string model = "Weights_with_gray.pb";
    private readonly string model = "Weights2.pb";
    //private readonly string model = "opt_freezed_graph.pb";
    private readonly string objOnUnity_ImageRGB = "ImageRGB";
    private readonly string objOnUnity_ImageDepth = "ImageDepth";


    public void DoInference(Hashtable args)
    {
        //=============TENSORFLOW==========================
        /*declare - module*/
        Net tensorflow = Dnn.readNetFromTensorflow(PATH.GetOnResources(model));
        /*end delcare*/

        /*pre-process - input*/
        Mat mmInputImg = Imgcodecs.imread(PATH.GetOnResources(args["inputImg"].ToString() + args["number"]));
        Imgproc.equalizeHist(mmInputImg, mmInputImg);
        Imgproc.cvtColor(mmInputImg, mmInputImg, Imgproc.COLOR_BGR2GRAY);
        //Downscaling(mmInputImg, mmInputImg, new Size(DIMENSION.INPUT_W, DIMENSION.INPUT_H));
        //Mat mmInputTensor = LoadTensorFromImage(mmInputImg);
        /*end pre-process*/

        if (mmInputImg.size() != new Size(320, 240)) // if Size(1:1)
        {
            Mat mmInputImg_size = Mat.zeros(new Size(DIMENSION.INPUT_W, DIMENSION.INPUT_H), CvType.CV_8UC3);

            Imgproc.resize(mmInputImg, mmInputImg, new Size(DIMENSION.INPUT_H * (0.9), DIMENSION.INPUT_H * (0.9)));
            //Downscaling(mmInputImg, mmInputImg, new Size(DIMENSION.INPUT_H * (0.9), DIMENSION.INPUT_H * (0.9)));

            int diff_h = (mmInputImg_size.height() - mmInputImg.height()) / 2;
            int diff_w = (mmInputImg_size.width() - mmInputImg.width()) / 2;


            for (int i = 0; i < mmInputImg_size.height(); i++)
            {
                for (int j = 0; j < mmInputImg_size.width(); j++)
                {

                    if (i >= diff_h && j >= diff_w && i < mmInputImg.height() + diff_h && j < mmInputImg.width() + diff_w)
                    {
                        mmInputImg_size.put(i, j, new double[] {
                            (mmInputImg.get(i - diff_h, j - diff_w)[0] / 255) * 100,
                            (mmInputImg.get(i - diff_h, j - diff_w)[0] / 255) * 100,
                            (mmInputImg.get(i - diff_h, j - diff_w)[0] / 255) * 100 });
                    }
                    else { mmInputImg_size.put(i, j, new double[] { 200, 200, 200 }); }
                }
            }
            Imgcodecs.imwrite(PATH.GetOnResources("unitytest.jpg"), mmInputImg_size);

            mmInputImg = mmInputImg_size.clone();
        }
        else
        {
            //Mat mmInputImg_preprocess1 = DetectBlockRectangles(mmInputImg);
            Mat mmInputImg_preprocess2 = BackgroundMargin(mmInputImg);
            mmInputImg = mmInputImg_preprocess2;

            Downscaling(mmInputImg, mmInputImg, new Size(DIMENSION.INPUT_W, DIMENSION.INPUT_H));

            Mat mmInputImg_clone = Mat.zeros(mmInputImg.size(), CvType.CV_8UC3);
            for (int i = 0; i < mmInputImg.height(); i++)
            {
                for (int j = 0; j < mmInputImg.width(); j++)
                {
                    mmInputImg_clone.put(i, j, new double[] { (mmInputImg.get(i, j)[0] / 255) * 100, (mmInputImg.get(i, j)[0] / 255) * 100, (mmInputImg.get(i, j)[0] / 255) * 100 });
                }
            }
            mmInputImg = mmInputImg_clone.clone();
        }

        Mat mmInputTensor = LoadTensorFromImage(mmInputImg);
        ///*end pre-process*/

        /*do*/
        tensorflow.setInput(mmInputTensor, "input_placeholder_x");
        var inferenced = tensorflow.forward();
        /*end do*/

        /*post-process - output*/
        Mat mmOutputImg = LoadDepthFromTensor(inferenced, DIMENSION.OUTPUT_H, DIMENSION.OUTPUT_W);
        Upscaling(mmOutputImg, mmOutputImg, new Size(320, 240));
        Imgcodecs.imwrite(PATH.GetOnResources(args["outputDepth"].ToString()), mmOutputImg);
        /*end post-process*/
        //=================================================

        //DrawImageUI(args["inputImg"].ToString(), "Image_01");
        //DrawImageUI(args["outputDepth"].ToString(), "Image_02");
    }

    static WWW LoadFromDisk(string path){
        Uri uri = new Uri(PATH.GetOnResources(path));
        string convert = uri.AbsoluteUri;
        return new WWW(convert);
    }

    static void DrawImageUI(string imgPath, string imageName){
        try{
            Image image = GameObject.Find(imageName).GetComponent<Image>();

#if UNITY_IOS
            image.transform.localScale = new Vector3(1, -1, 1);
#endif

            var www = LoadFromDisk(imgPath);

            image.sprite = Sprite.Create(www.textureNonReadable, new Rect(0, 0, www.textureNonReadable.width, www.textureNonReadable.height), new Vector2(0, 0));
        } catch (Exception e){
            return;
        }
    }

    static Mat BackgroundMargin(Mat src, float H_RATIO = 0.05f, float W_RATIO = 0.15f)
    {
        Mat dst = src.clone();
        for (int h = 0; h < dst.height(); h++)
        {
            for (int w = 0; w < dst.width(); w++)
            {
                if (h < H_RATIO * dst.height() || h > (1 - H_RATIO) * dst.height()
                    || w < W_RATIO * dst.width() || w > (1 - W_RATIO) * dst.width())
                    dst.put(h, w, new byte[] { 255 });
            }
        }

        return dst;
    }
}
