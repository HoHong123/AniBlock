using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControlScript : MonoBehaviour 
{
    public enum AnimationState
    {
        C1,
        I1,
        I2,
        I3,
        M1,
        J1,
        ETC1,
        ETC2,
        ETC3,
    }

    [SerializeField] private Animator a_Animator;
    [SerializeField] private AnimationState e_AnimationState = AnimationState.I2;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        if(a_Animator == null)
            a_Animator = GetComponent<Animator>();

        SetTrigger(e_AnimationState);
    }

    public void SetTrigger(AnimationState animationState)
    {
        //Debug.Log(animationState.ToString());
        a_Animator.SetTrigger(animationState.ToString());
    }

    public bool GetCurrentAnimation(string _name)
    {
        return (a_Animator.GetCurrentAnimatorStateInfo(0).IsName(_name)) ? true : false;
    }
}