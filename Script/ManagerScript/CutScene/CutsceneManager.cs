using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CutScene
{
    public class CutsceneManager : MonoBehaviour
    {
        [SerializeField] Button BTN_Touch;
        [SerializeField] Animator Animator;

        void Start()
        {
            int num = 0;
            BTN_Touch.onClick.AddListener(() => {
                Debug.Log("Touch : " + num);
                Animator.SetBool("New Bool " + num, true);
                num++;
            });
        }
    }
}
