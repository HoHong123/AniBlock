using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalancePart : MonoBehaviour
{
    public bool m_IsScalePartLeft = false;

    private bool p_IsMouseDown = false;
    private BalanceManager p_BalanceManager = null;

    private void Start()
    {
        p_BalanceManager = BalanceManager.S;
    }

    private void OnMouseDown()
    {
        if (p_BalanceManager.m_IsGamePlay == false) return;

        p_IsMouseDown = true;
    }

    private void OnMouseUp()
    {
        if (p_BalanceManager.m_IsGamePlay == false) return;
        if (p_IsMouseDown == false) return;

        p_BalanceManager.PopList(m_IsScalePartLeft);
        p_IsMouseDown = false;
    }
}
