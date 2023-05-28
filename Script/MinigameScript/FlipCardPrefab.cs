using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipCardPrefab : MonoBehaviour
{
    public bool m_IsEnabled = false;
    public int  m_Number = 0;

    public bool m_IsFlip = true;
    public bool m_IsClear = false;

    private SpriteRenderer p_MarkerSpriteRender = null;

    private void Awake()
    {
        p_MarkerSpriteRender = this.gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        if (FlipCardManager.S.m_IsGamePlay == false || m_IsEnabled == false || m_IsClear) return;

        if (m_IsFlip)
        {
            ActiveFlip(false);
            FlipCardManager.S.PairCheck(this);
        }
    }

    public void ActiveFlip(bool _isAtive)
    {
        if (_isAtive)
        {
            m_IsFlip = true;
            p_MarkerSpriteRender.enabled = false;
        }
        else
        {
            m_IsFlip = false;
            p_MarkerSpriteRender.enabled = true;
        }
    }
}
