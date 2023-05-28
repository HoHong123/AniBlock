using System.IO;
using UnityEngine;

namespace FileIO.OutputJPG
{
    public class ImageOutput : MonoBehaviour
    {
        public void Initiation()
        {
            string srcPath_weight = Path.Combine(Application.streamingAssetsPath, "Depth/SangwonKim_Weights.pb"); // 기존 데이터 파일
            string srcPath_weight2 = Path.Combine(Application.streamingAssetsPath, "Depth/Weights2.pb"); // 기존 데이터 파일
            string srcPath_weight3 = Path.Combine(Application.streamingAssetsPath, "Depth/Weights_with_gray.pb"); // 기존 데이터 파일
            string srcPath_opt = Path.Combine(Application.streamingAssetsPath, "Depth/opt_freezed_graph.pb"); // 기존 데이터 파일
            string srcPath_dataset = Path.Combine(Application.streamingAssetsPath, "Depth/training.data"); // 기존 데이터 파일
            string srcPath_dataset2 = Path.Combine(Application.streamingAssetsPath, "Depth/training_org.data"); // 기존 데이터 파일

#if UNITY_EDITOR || UNITY_ANDROID

            string destPath = Path.Combine(Application.persistentDataPath, "ENet"); // 새로 파일을 저장할 위치
            if (!Directory.Exists(destPath)) { Directory.CreateDirectory(destPath); }

            // 새로 정의될 데이터 파일
            string destPath_weight = Path.Combine(destPath, "SangwonKim_Weights.pb");
            string destPath_weight2 = Path.Combine(destPath, "Weights2.pb");
            string destPath_weight3 = Path.Combine(destPath, "Weights_with_gray.pb");
            string destPath_opt = Path.Combine(destPath, "opt_freezed_graph.pb");
            string destPath_dataset = Path.Combine(destPath, "training.data");
            string destPath_dataset2 = Path.Combine(destPath, "training_org.data");


            // Weight 파일 생성
            if (!File.Exists(destPath_weight)) // SanwonKim_Weights.pb가 존재하지 않으면 새로운 파일 생성
            {
                using (WWW request = new WWW(srcPath_weight))
                {
                    while (!request.isDone) {; }
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        Debug.LogWarning(request.error);
                        return;
                    }

                    // Create Directory
                    string dirPath = Path.GetDirectoryName(destPath_weight);
                    if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                    File.WriteAllBytes(destPath_weight, request.bytes);
                }
            }

            if (!File.Exists(destPath_weight2)) // Weights2.pb가 존재하지 않으면 새로운 파일 생성
            {
                using (WWW request = new WWW(srcPath_weight2))
                {
                    while (!request.isDone) {; }
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        Debug.LogWarning(request.error);
                        return;
                    }

                    // Create Directory
                    string dirPath = Path.GetDirectoryName(destPath_weight2);
                    if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                    File.WriteAllBytes(destPath_weight2, request.bytes);
                }
            }

            if (!File.Exists(destPath_weight3)) // Weights_with_gray.pb가 존재하지 않으면 새로운 파일 생성
            {
                using (WWW request = new WWW(srcPath_weight3))
                {
                    while (!request.isDone) {; }
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        Debug.LogWarning(request.error);
                        return;
                    }

                    // Create Directory
                    string dirPath = Path.GetDirectoryName(destPath_weight3);
                    if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                    File.WriteAllBytes(destPath_weight3, request.bytes);
                }
            }

            // opt 파일 생성
            if (!File.Exists(destPath_opt)) // Weights_with_gray.pb가 존재하지 않으면 새로운 파일 생성
            {
                using (WWW request = new WWW(srcPath_opt))
                {
                    while (!request.isDone) {; }
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        Debug.LogWarning(request.error);
                        return;
                    }

                    // Create Directory
                    string dirPath = Path.GetDirectoryName(destPath_opt);
                    if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                    File.WriteAllBytes(destPath_opt, request.bytes);
                }
            }

            // training 파일 생성
            if (!File.Exists(destPath_dataset)) // training.data가 존재하지 않으면 새로운 파일 생성
            {
                using (WWW request = new WWW(srcPath_dataset))
                {
                    while (!request.isDone) {; }
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        Debug.LogWarning(request.error);
                        return;
                    }

                    // Create Directory
                    string dirPath = Path.GetDirectoryName(destPath_dataset);
                    if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                    File.WriteAllBytes(destPath_dataset, request.bytes);
                }
            }

            if (!File.Exists(destPath_dataset2)) // training.data가 존재하지 않으면 새로운 파일 생성
            {
                using (WWW request = new WWW(srcPath_dataset2))
                {
                    while (!request.isDone) {; }
                    if (!string.IsNullOrEmpty(request.error))
                    {
                        Debug.LogWarning(request.error);
                        return;
                    }

                    // Create Directory
                    string dirPath = Path.GetDirectoryName(destPath_dataset2);
                    if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                    File.WriteAllBytes(destPath_dataset2, request.bytes);
                }
            }
#elif UNITY_IOS
        // 새로 정의될 파일
        string destPath_weight = Path.Combine(Application.persistentDataPath, "SangwonKim_Weights.pb");
        string destPath_weight2 = Path.Combine(Application.persistentDataPath, "Weights2.pb"); 
        string destPath_weight3 = Path.Combine(Application.persistentDataPath, "Weights_with_gray.pb");
        string destPath_opt = Path.Combine(Application.persistentDataPath, "opt_freezed_graph.pb");
        string destPath_dataset = Path.Combine(Application.persistentDataPath, "training.data");
        string destPath_dataset2 = Path.Combine(Application.persistentDataPath, "training_org.data");

        // 새로 저장될 경로
        string dirPath = Path.GetDirectoryName(destPath_weight);

        // Weight 파일
        if (File.Exists(srcPath_weight) || !File.Exists(destPath_weight))
        {
            // Create Directory
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            File.WriteAllBytes(destPath_weight, File.ReadAllBytes(srcPath_weight));

            if (!File.Exists(destPath_weight))
            {
                File.WriteAllBytes(destPath_weight, new WWW(srcPath_weight).bytes);
            }
        }
        if (File.Exists(srcPath_weight2) || !File.Exists(destPath_weight2))
        {
            // Create Directory
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            File.WriteAllBytes(destPath_weight2, File.ReadAllBytes(srcPath_weight2));

            if (!File.Exists(destPath_weight2))
            {
                File.WriteAllBytes(destPath_weight2, new WWW(srcPath_weight2).bytes);
            }
        }
        if (File.Exists(srcPath_weight3) || !File.Exists(destPath_weight3))
        {
            // Create Directory
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            File.WriteAllBytes(destPath_weight3, File.ReadAllBytes(srcPath_weight3));

            if (!File.Exists(destPath_weight3))
            {
                File.WriteAllBytes(destPath_weight3, new WWW(srcPath_weight3).bytes);
            }
        }

        // opt 파일
        if (!File.Exists(destPath_opt)) 
        {
            dirPath = Path.GetDirectoryName(destPath_opt);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            File.WriteAllBytes(destPath_opt, File.ReadAllBytes(srcPath_opt));

            if (!File.Exists(destPath_opt))
            {
                File.WriteAllBytes(destPath_opt, new WWW(srcPath_opt).bytes);
            }
        }

        // training 파일
        if (!File.Exists(destPath_dataset)) 
        {
            dirPath = Path.GetDirectoryName(destPath_dataset);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            File.WriteAllBytes(destPath_dataset, File.ReadAllBytes(srcPath_dataset));

            if (!File.Exists(destPath_dataset))
            {
                File.WriteAllBytes(destPath_dataset, new WWW(srcPath_dataset).bytes);
            }
        }
        
        if (!File.Exists(destPath_dataset2)) 
        {
            dirPath = Path.GetDirectoryName(destPath_dataset2);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            File.WriteAllBytes(destPath_dataset2, File.ReadAllBytes(srcPath_dataset2));

            if (!File.Exists(destPath_dataset2))
            {
                File.WriteAllBytes(destPath_dataset2, new WWW(srcPath_dataset2).bytes);
            }
        }
#endif
        }

        public void Capture_1x1_JpgFile(out Texture2D crop, Vector2 _SizeOfTexture, string _FileName, float _StartHeight = 0)
        {
            Texture2D texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            texture2D = ScreenCapture.CaptureScreenshotAsTexture();

            FileStream fs2 = File.Create(PATH.GetOnResources(_FileName + ".jpg"));
            Color[] colors2 = texture2D.GetPixels((int)((Screen.width - _SizeOfTexture.y) * 0.5f), (int)_StartHeight, (int)_SizeOfTexture.y, (int)_SizeOfTexture.y);
            crop = new Texture2D((int)_SizeOfTexture.y, (int)_SizeOfTexture.y);
            crop.SetPixels(colors2);
            crop.Apply();

            byte[] savedata2 = crop.EncodeToJPG();

            fs2.Write(savedata2, 0, savedata2.Length);
            fs2.Close();
        }
    }
}
