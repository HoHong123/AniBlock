using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace URL
{
    public class Upload : MonoBehaviour
    {
        [ContextMenu("TestUploadFile")]
        public void TestUploadFile(string pathFile, string name)
        {
            Debug.Log("Start upload");
            byte[] myBytes = File.ReadAllBytes(pathFile);
            string myBase64 = Convert.ToBase64String(myBytes);

            RequestUploadModel requestUploadModel = new RequestUploadModel
            {
                pathFile = name,  //AWS S3에 저장되는 이름
                base64 = myBase64
            };

            string json = JsonConvert.SerializeObject(requestUploadModel);

            ApiManager.Instance.UploadFileToS3(json, (error, response) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                }
                else
                {
                    ResponseUploadModel responseUploadModel = JsonConvert.DeserializeObject<ResponseUploadModel>(response);
                    Debug.Log("Link file:: " + responseUploadModel.Location);
                }
            });
            
        }
    }
}