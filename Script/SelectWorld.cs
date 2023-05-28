using UnityEngine;


public class SelectWorld : MonoBehaviour
{

    [SerializeField]
    private GameObject PororoWorldOutLine;
    [SerializeField]
    private GameObject FirstWorldOutLine;
    [SerializeField]
    private GameObject blind;
   
    private void Update()
    {
        switch (PlayerPrefs.GetString("nowWorld"))
        {
            case "Pororo":
                PororoWorldOutLine.SetActive(true);
                FirstWorldOutLine.SetActive(false);
                break;
            case "First":
                PororoWorldOutLine.SetActive(false);
                FirstWorldOutLine.SetActive(true);
                break;
        }
    }

    public void SelectPororoWorld()
    {
        PlayerPrefs.SetString("nowWorld", "Pororo");
        LoadMaster.S.LoadSceneFunc("Scene_World_PRR");
    }

    public void SelectFirstWorld()
    {
        PlayerPrefs.SetString("nowWorld", "First");
        LoadMaster.S.LoadSceneFunc("Scene_World_First");
    }

    public void BackButton()
    {
        switch (PlayerPrefs.GetString("nowWorld"))
        {
            case "First":
                LoadMaster.S.LoadSceneFunc("Scene_World_First");
                break;
            case "Pororo":
                LoadMaster.S.LoadSceneFunc("Scene_World_PRR");
                break;
        }        
    }

    public void HelpButton()
    {
        blind.SetActive(true);
    }
}
