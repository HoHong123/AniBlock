using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DividePrefab : MonoBehaviour
{
    public bool m_IsEnabled = false;
    public int  m_Number    = 0;

    public enum DIR
    {
        LEFT,
        RIGHT,
    }
    public DIR m_Dir = DIR.LEFT;

    private void OnMouseDown()
    {
        if (m_IsEnabled == false) return;

        DivideLRManager.S.AnswerCheck(m_Dir);
    }
}
