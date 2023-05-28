using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CV_GalleryButtonScript : MonoBehaviour
{
    [SerializeField] private Image[] image = new Image[64];
    private Image backGround;

    public bool isSelected
    {
        set
        {
            backGround.color = (value) ? new Color(0.984f, 0.552f, 0.125f, 1) : new Color(0.094f, 0.576f, 0.635f, 1);
        }
    }

    private void Start()
    {
        backGround = GetComponent<Image>();
    }

    public void SetColorArray(Color[] colors)
    {        
        int i = 0;
        foreach (Color cell in colors)
        {
            if(cell.a != 0)
            {
                image[i].color = cell;
            }
            i++;
        }
    }
}
