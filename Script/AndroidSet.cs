using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AndroidSet : MonoBehaviour
{
    private Manager.Util.PackageManager _PackageManager = null;
    private int ClickCount;
    [SerializeField]
    private Text txt;

    public string PreSceneName;

    private void Awake()
    {        
        txt.enabled = false;
    }
    private void Start()
    {
        _PackageManager = Manager.Util.PackageManager.Instance;
    }

    private void Update()
   {
        if (SceneManager.GetActiveScene().name == "Scnen_Main") {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (ClickCount == 0)
                {
                    if (Application.systemLanguage == SystemLanguage.Korean)
                    {
                        showToast("뒤로 가기 버튼을 한번 더 누르시면 종료 됩니다.", 3.0f);
                    }
                    else if (Application.systemLanguage == SystemLanguage.English)
                    {
                        showToast("Press the back button one more time to finish.", 3.0f);
                    }
                }
                ClickCount++;
                if (!IsInvoking("DoubleClick"))
                {
                    Invoke("DoubleClick", 3.0f);
                }
            }
            else if (ClickCount == 2)
            {
                CancelInvoke("DoubleClick");
                Application.Quit();
            }
        }
        else if(SceneManager.GetActiveScene().name == "Scnen_MiniGame")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (ClickCount == 0)
                {
                    if (Application.systemLanguage == SystemLanguage.Korean)
                    {
                        showToast("뒤로 가시겠습니까", 3.0f);
                    }
                    else if (Application.systemLanguage == SystemLanguage.English)
                    {
                        showToast("Press the back button one more time to finish.", 3.0f);
                    }
                }
                ClickCount++;
                if (!IsInvoking("DoubleClick"))
                {
                    Invoke("DoubleClick", 3.0f);
                }
                else if (ClickCount == 2)
                {
                    switch (_PackageManager.CURRENT_THEME_INFO)
                    {
                        case Manager.Util.PackageManager.PACKAGE_INFO.FIRST:
                            LoadMaster.S.LoadSceneFunc("Scene_Rescue");
                            break;

                        case Manager.Util.PackageManager.PACKAGE_INFO.RENEWEL:
                         //   LoadMaster.S.LoadSceneFunc(PreSceneName);
                            break;

                        case Manager.Util.PackageManager.PACKAGE_INFO.PPORORO:
                            LoadMaster.S.LoadSceneFunc("Scene_Rescue_PRR");
                            break;

                        case Manager.Util.PackageManager.PACKAGE_INFO.NumOf:
                        default:
                            Debug.LogError("MainManager(ChangeTheme) :: There is no theme info");
                            return;
                    }                    
                }
            }
        }        
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (ClickCount == 0)
                {
                    if (Application.systemLanguage == SystemLanguage.Korean)
                    {
                        showToast("뒤로 가시겠습니까", 3.0f);
                    }
                    else if (Application.systemLanguage == SystemLanguage.English)
                    {
                        showToast("Press the back button one more time to finish.", 3.0f);
                    }
                }
                ClickCount++;
                if (!IsInvoking("DoubleClick"))
                {
                    Invoke("DoubleClick", 3.0f);
                }
                else if (ClickCount == 2)
                {
                    LoadMaster.S.LoadSceneFunc(PreSceneName);
                }
            }
        }
    }

    private void showToast(string text, float duration)
    {
        StartCoroutine(showToastCOR(text, duration));
    }

    private IEnumerator showToastCOR(string text, float duration)
    {
        Color orginalColor = txt.color;

        txt.text = text;
        txt.enabled = true;

        //Fade in
        yield return fadeInAndOut(txt, true, 0.5f);

        //Wait for the duration
        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            yield return null;
        }

        //Fade out
        yield return fadeInAndOut(txt, false, 0.5f);

        txt.enabled = false;
        txt.color = orginalColor;
    }

    private IEnumerator fadeInAndOut(Text targetText, bool fadeIn, float duration)
    {
        //Set Values depending on if fadeIn or fadeOut
        float a, b;
        if (fadeIn)
        {
            a = 0f;
            b = 1f;
        }
        else
        {
            a = 1f;
            b = 0f;
        }

        Color currentColor = Color.white;
        float counter = 0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(a, b, counter / duration);

            targetText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }
    }

    private void DoubleClick()
    {        
        ClickCount = 0;
    }
}