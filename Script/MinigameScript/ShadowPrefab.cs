using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowPrefab : MonoBehaviour
{
    public bool m_IsAnswer = false;

    private RectTransform p_RectTrans = null;

    private Vector2 p_OriRectPos    = Vector2.zero;
    private bool    p_isMouseDown   = false;

    //자신 숫ㅈ
    public int m_Number = -1;
    //충돌체 감지
    private ShadowPrefab p_CollisionShadowPrefabScript = null;

    private void Awake()
    {
        p_RectTrans = this.gameObject.GetComponent<RectTransform>();
    }

    private void OnMouseDown()
    {
        if (m_IsAnswer == false) return;

        p_isMouseDown = true;
        p_OriRectPos = p_RectTrans.anchoredPosition;
    }

    private void OnMouseUp()
    {
        if (m_IsAnswer == false) return;

        if(p_CollisionShadowPrefabScript == null)
        {
            p_isMouseDown = false;
            p_RectTrans.anchoredPosition = p_OriRectPos;
        }
        else
        {
            if(m_Number == p_CollisionShadowPrefabScript.m_Number)
            {
                MatchToShadowManager.S.AnswerCheck(true);
            }
            else
            {
                MatchToShadowManager.S.AnswerCheck(false);
            }
        }
    }

    private void OnMouseDrag()
    {
        if (m_IsAnswer == false) return;

        var v3 = Input.mousePosition;
        v3.z = 10.0f;
        v3 = Camera.main.ScreenToWorldPoint(v3);

        this.gameObject.transform.position = v3;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_IsAnswer == false) return;

        if (collision.transform.gameObject.GetComponent<ShadowPrefab>() != null)
        {
            p_CollisionShadowPrefabScript = collision.transform.gameObject.GetComponent<ShadowPrefab>();
            //Debug.Log("충돌 감지 : " + collision.transform.gameObject.name);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (m_IsAnswer == false) return;

        if (collision.transform.gameObject.GetComponent<ShadowPrefab>() != null)
        {
            p_CollisionShadowPrefabScript = null;
            //Debug.Log("충돌 해제 : " + collision.transform.gameObject.name);
        }
    }
}
