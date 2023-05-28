using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;

public class Color_Histogram_Feature_Extraction : MonoBehaviour {
    public List<Feature> Color_Histogram_Of_Test_Image(Mat test_src_img)
    {
        Feature feature;
        List<Feature> feature_list = new List<Feature>();
        List<Mat> mat_split_ch = new List<Mat>();
        int count = 0;
        Core.split(test_src_img, mat_split_ch);
        int red = 0, green = 0, blue = 0;
        string content = null;
        for (int i = 0; i < mat_split_ch.Count; i++)
        {
            count += 1;
            Mat hist = new Mat();
            Imgproc.calcHist(mat_split_ch, new MatOfInt(i), new Mat(), hist, new MatOfInt(256), new MatOfFloat(new float[] { 0, 256 }));
            switch (count)
            {
                case 1:
                    blue = (int)Core.minMaxLoc(hist).maxLoc.y;
                    break;
                case 2:
                    green = (int)Core.minMaxLoc(hist).maxLoc.y;
                    break;
                case 3:
                    red = (int)Core.minMaxLoc(hist).maxLoc.y;
                    // content = red + "," + green + "," + blue;
                    feature_list.Add(new Feature(red, green, blue));
                    break;
            }
        }
        // File.WriteAllText(PATH.GetOnResources("test.data"), content);

        return feature_list;
    }

    public void Color_Histogram_Of_Training_Image(string img_name) {}
    public void Training(){}
}
