using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneScript : MonoBehaviour {

    public string moveSceneName = "";
    public bool permission = false;

    public static string prevMoveScene;

    [Space(10)]
    [Header("==== 조건부 이동 ====")]

    public string conditionA = "";
    public string ALeadTo = "";

    [Space(5)]

    public string conditionB = "";
    public string BLeadTo = "";


    public void changeWorld(string world)
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Scene_World_First":
                //Aster 초기화
                LoadMaster.S.LoadSceneFunc(world);
                break;
            case "Scene_World_PRR":
                //Aster 초기화
                LoadMaster.S.LoadSceneFunc(world);
                break;
            default:
                break;
        }
    }

    public void LoadSceneFunc()
    {
        
        if (permission)
        {
            prevMoveScene = SceneManager.GetActiveScene().name;
            LoadMaster.S.LoadSceneFunc(moveSceneName);
        }
    }

    public void LoadSceneInCondition()
    {
        if(LoadMaster.S.s_PrevSceneName == conditionA)
        {
            LoadMaster.S.LoadSceneFunc(ALeadTo);
        }
        else if(LoadMaster.S.s_PrevSceneName == conditionB)
        {
            LoadMaster.S.LoadSceneFunc(BLeadTo);
        }
    }
}
