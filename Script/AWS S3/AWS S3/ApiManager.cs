using System;
using Newtonsoft.Json;
using Proyecto26;
using UnityEngine;
namespace URL
{
    public class ApiManager : MonoBehaviour
    {
        #region VARIABLES

        private static ApiManager m_Instance;
        public static ApiManager Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new GameObject("[API_MANAGER_CREATED]").AddComponent<ApiManager>();

                return m_Instance;
            }
        }

        #endregion

        #region MONOBEHAVIOUR_METHODS

        private void Awake()
        {
            m_Instance = this;
        }

        #endregion

        #region PUBLIC_METHODS

        public void UploadFileToS3(string bodyJson, Action<string, string> result)
        {
            RestClient.Post(Constants.ENDPOINT_UPLOAD, bodyJson, (err, res) =>
            {
                if (err != null)
                {
                    result?.Invoke(err.Message, null);
                    Debug.Log(err.Message);
                }
                else
                {
                    result?.Invoke(null, res.Text);
                }
            });
        }

        #endregion
    }
}
