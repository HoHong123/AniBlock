using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WORLD_SetCutScenePrefab : MonoBehaviour
{
    static BlockColor _blockColor = null;
    public static BlockColor S_BlockColor {
        get
        {
            if (_blockColor == null) _blockColor = new BlockColor();

            return _blockColor;
        }
    }

    public string CutsceneName = "";
    public Vector3 V3_DefaultSize;
    public Transform TRAN_Pivot = null;


    private GameObject DrawPanel = null;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(PlayerPrefs.HasKey(CutsceneName));
        if (!PlayerPrefs.HasKey(CutsceneName)) return;

        DrawPanel = Instantiate(Resources.Load<GameObject>("Prefab/CUT_DrawPanel"));
        DrawPanel.transform.SetParent(TRAN_Pivot);
        DrawPanel.transform.localPosition = Vector3.zero;
        DrawPanel.transform.localRotation = Quaternion.identity;

        DrawPanel.transform.localScale = V3_DefaultSize;

        string[] colors = PlayerPrefs.GetString(CutsceneName).Split('/');

        for(int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == "") continue;

            int color = int.Parse(colors[i].Split('x')[0]);
            int pos = int.Parse(colors[i].Split('x')[1]);

            Color c = new Color(S_BlockColor.Get_R(color) / 255.0f, S_BlockColor.Get_G(color) / 255.0f, S_BlockColor.Get_B(color) / 255.0f);

            DrawPanel.transform.GetChild(pos).gameObject.SetActive(true); // 찍은대로 표시
            DrawPanel.transform.GetChild(pos).GetComponent<MeshRenderer>().material.SetColor("_Color", c);
        }
    }
}
