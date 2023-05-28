using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Manager.Util;


[RequireComponent(typeof(WorldAnimationManager))]
public class WORLD_CharacterSoundController : MonoBehaviour
{

    [Header("====== 애니메이션 변수 ======")]
    [SerializeField] private PackageManager.FirstCollection e_Character = PackageManager.FirstCollection.NumOf;
    [SerializeField] private AnimationControlScript SCRIPT_Character;

    [Header("====== 길 찾기 변수 ======")]
    [SerializeField] private Patrol SCRIPT_Patrol = null;
    [SerializeField] private AIPath SCRIPT_AI = null;
    [SerializeField] private float f_Wait = 1.5f;
    private bool b_isHit = false;

    public bool b_AnimationIsRunning = false;

    private void OnEnable()
    {
        // 게임 업브젝트의 이름과 동일한 명칭의 FirstCollection 값을 가져와 입력
        e_Character = (PackageManager.FirstCollection)System.Enum.Parse(typeof(PackageManager.FirstCollection), gameObject.name);

        SCRIPT_Character = GetComponentInChildren<AnimationControlScript>();
        SCRIPT_Patrol = GetComponentInChildren<Patrol>();
        SCRIPT_AI = GetComponentInChildren<AIPath>();
    }

    public void OnHit()
    {
        if (b_isHit) return;

        Manager.Sound.SoundManager.S.Play_CharacterEffect(e_Character);
        SCRIPT_Character.SetTrigger(AnimationControlScript.AnimationState.C1);

        if(SCRIPT_Patrol != null && SCRIPT_AI != null)
        {
            SCRIPT_Patrol.enabled = false;
            SCRIPT_AI.enabled = false;

            b_isHit = true;

            StartCoroutine(wait());
        }
    }

    private IEnumerator wait()
    {
        yield return new WaitForSeconds(f_Wait);

        b_isHit = false;
        SCRIPT_Patrol.enabled = true;
        SCRIPT_AI.enabled = true;
    }

    private void Update()
    {
        if (SCRIPT_Patrol == null || SCRIPT_AI == null)
        {
            Debug.LogError("Patrol Script : " + ((SCRIPT_Patrol != null ? true : false) + ", AI Script : " + ((SCRIPT_AI != null) ? true : false)));
            enabled = false;
            return;
        }

        // A*알고리즘이 활성화되어 있을때
        if (SCRIPT_Patrol.enabled == true)
        {
            // 캐릭터가 이동 중일때
            if (SCRIPT_Patrol.reach == false && SCRIPT_Character.GetCurrentAnimation("M1") == false)
            {
                SCRIPT_Character.SetTrigger(AnimationControlScript.AnimationState.M1);
            }
            // 캐릭터가 목표에 도달했을때
            else if (SCRIPT_Patrol.reach == true)
            {
                if (SCRIPT_Character.GetCurrentAnimation("I2") == false && SCRIPT_Character.GetCurrentAnimation("ETC3") == false)
                {
                    //if (Random.RandomRange(0, 4) > 1)
                    //{
                    //    SCRIPT_Character.SetTrigger(AnimationControlScript.AnimationState.ETC3);
                    //}
                    //else
                    //{
                        SCRIPT_Character.SetTrigger(AnimationControlScript.AnimationState.I2);
                    //}
                }
            }
        }
    }
}
