using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharPrefab : MonoBehaviour
{
    public bool m_IsChar = false;

    public enum LOOKDIR
    {
        LEFT,
        RIGHT,
    }
    public LOOKDIR m_LookDir = LOOKDIR.LEFT;
    private RectTransform p_RectTrans = null;

    //ENEMY..
    public SeparateLRManager.ENEMY p_MyEnemy = SeparateLRManager.ENEMY.Scorpion;

    private SeparateLRManager p_SeparateLRManager = null;

    //DATASET..
    private float p_SlowSpeed = 0.3f;
    private float p_MidiumSpeed = 0.5f;
    private float p_HighSpeed = 1.25f;
    private float p_HighHighSpeed = 2.0f;

    private void Awake()
    {
        p_RectTrans = this.gameObject.GetComponent<RectTransform>();
    }

    private void Start()
    {
        p_SeparateLRManager = SeparateLRManager.S;
    }

    private bool    p_EnemyMovingWaitTime = false;
    private float   p_EnemyStopWaitTime = 0.0f;

    public float    p_EnemyStopStartTime = 0.0f;

    private void Update()
    {
        if (p_SeparateLRManager.m_IsGamePlay == false) return;

        if (m_IsChar)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (SeparateLRManager.S.m_IsGamePlay == false) return;
                if (p_EnemyMovingWaitTime) return;

                p_EnemyMovingWaitTime = true;

                if (m_LookDir == LOOKDIR.LEFT)
                {
                    m_LookDir = LOOKDIR.RIGHT;
                    p_RectTrans.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                }
                else
                {
                    m_LookDir = LOOKDIR.LEFT;
                    p_RectTrans.rotation = Quaternion.Euler(Vector3.zero);
                }

                Invoke("ResetEnemyMovingWaitTime", 0.25f);
            }
        }
        else
        {
            if (p_EnemyMovingWaitTime) return;

            if (m_LookDir == LOOKDIR.LEFT)
            {
                switch (p_MyEnemy)
                {
                    case SeparateLRManager.ENEMY.Scorpion:
                        p_RectTrans.anchoredPosition += new Vector2(1.0f, 0.0f) * p_MidiumSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Bird:
                        p_RectTrans.anchoredPosition += new Vector2(1.0f, 0.0f) * p_MidiumSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Mantis:
                        p_RectTrans.anchoredPosition += new Vector2(1.0f, 0.0f) * p_HighSpeed * Time.deltaTime;

                        p_EnemyStopStartTime += Time.deltaTime;

                        if (p_EnemyStopStartTime >= 2.0f)
                        {
                            p_EnemyMovingWaitTime = true;
                            Invoke("ResetEnemyMovingWaitTime", 1.0f);
                        }
                        break;

                    //=====================================

                    case SeparateLRManager.ENEMY.Snake:
                        p_RectTrans.anchoredPosition += new Vector2(1.0f, 0.0f) * p_MidiumSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Dinosaur:
                        p_RectTrans.anchoredPosition += new Vector2(1.0f, 0.0f) * p_HighSpeed * Time.deltaTime;

                        p_EnemyStopStartTime += Time.deltaTime;

                        if (p_EnemyStopStartTime >= 1.0f)
                        {
                            p_EnemyMovingWaitTime = true;
                            Invoke("ResetEnemyMovingWaitTime", 1.0f);
                        }
                        break;

                    case SeparateLRManager.ENEMY.Rhinoceros:
                        if (p_EnemyStopStartTime <= 5.0f)
                        {
                            p_EnemyStopStartTime += Time.deltaTime;
                        }
                        else
                        {
                            p_RectTrans.anchoredPosition += new Vector2(1.0f, 0.0f) * p_HighHighSpeed * Time.deltaTime;
                        }
                        break;

                    //=====================================

                    case SeparateLRManager.ENEMY.Crocodile:
                        p_RectTrans.anchoredPosition += new Vector2(1.0f, 0.0f) * p_SlowSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Hippopotamus:
                        p_RectTrans.anchoredPosition += new Vector2(1.0f, 0.0f) * p_MidiumSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Eagle:
                        p_RectTrans.anchoredPosition += new Vector2(1.0f, 0.0f) * p_HighHighSpeed * Time.deltaTime;
                        break;
                }
            }
            else
            {
                switch (p_MyEnemy)
                {
                    case SeparateLRManager.ENEMY.Scorpion:
                        p_RectTrans.anchoredPosition += new Vector2(-1.0f, 0.0f) * p_MidiumSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Bird:
                        p_RectTrans.anchoredPosition += new Vector2(-1.0f, 0.0f) * p_MidiumSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Mantis:
                        p_RectTrans.anchoredPosition += new Vector2(-1.0f, 0.0f) * p_HighSpeed * Time.deltaTime;

                        p_EnemyStopStartTime += Time.deltaTime;

                        if (p_EnemyStopStartTime >= 2.0f)
                        {
                            p_EnemyMovingWaitTime = true;
                            Invoke("ResetEnemyMovingWaitTime", 1.0f);
                        }
                        break;

                    //=====================================

                    case SeparateLRManager.ENEMY.Snake:
                        p_RectTrans.anchoredPosition += new Vector2(-1.0f, 0.0f) * p_MidiumSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Dinosaur:
                        p_RectTrans.anchoredPosition += new Vector2(-1.0f, 0.0f) * p_HighSpeed * Time.deltaTime;

                        p_EnemyStopStartTime += Time.deltaTime;

                        if (p_EnemyStopStartTime >= 1.0f)
                        {
                            p_EnemyMovingWaitTime = true;
                            Invoke("ResetEnemyMovingWaitTime", 1.0f);
                        }
                        break;

                    case SeparateLRManager.ENEMY.Rhinoceros:
                        if (p_EnemyStopStartTime <= 5.0f)
                        {
                            p_EnemyStopStartTime += Time.deltaTime;
                        }
                        else
                        {
                            p_RectTrans.anchoredPosition += new Vector2(-1.0f, 0.0f) * p_HighHighSpeed * Time.deltaTime;
                        }
                        break;

                    //=====================================

                    case SeparateLRManager.ENEMY.Crocodile:
                        p_RectTrans.anchoredPosition += new Vector2(-1.0f, 0.0f) * p_SlowSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Hippopotamus:
                        p_RectTrans.anchoredPosition += new Vector2(-1.0f, 0.0f) * p_MidiumSpeed * Time.deltaTime;
                        break;

                    case SeparateLRManager.ENEMY.Eagle:
                        p_RectTrans.anchoredPosition += new Vector2(-1.0f, 0.0f) * p_HighHighSpeed * Time.deltaTime;
                        break;
                }
            }
        }
    }

    public void ResetEnemyMovingWaitTime()
    {
        p_EnemyStopStartTime = 0.0f;
        p_EnemyMovingWaitTime = false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_IsChar == false) return;
        if (collision.transform.gameObject.GetComponent<CharPrefab>() == null) return;

        if (collision.transform.gameObject.GetComponent<CharPrefab>().m_LookDir == m_LookDir)
        {
            p_EnemyStopStartTime = 0.0f;
            p_SeparateLRManager.AnsweCheck(collision.transform.gameObject.GetComponent<CharPrefab>());
        }
        else
        {
            p_SeparateLRManager.GameOver();
        }
    }
}
