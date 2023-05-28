using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSCRIPT_StunStar : MonoBehaviour
{
    [SerializeField] private Camera OBJ_GameCamera = null;
    [SerializeField] private Transform[] TRAN_Stars = new Transform[2];
    [SerializeField] private float f_RotationSpeed = 30.0f;

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < 2; i++)
        {       
            TRAN_Stars[i].RotateAround(transform.position, Vector3.up, f_RotationSpeed * Time.deltaTime);

            // 스프라이트를 카메라를 바라보도록 회전
            Vector3 dir = TRAN_Stars[i].transform.position - OBJ_GameCamera.transform.position;
            TRAN_Stars[i].transform.rotation = Quaternion.LookRotation(dir, OBJ_GameCamera.transform.rotation * Vector3.up);
        }
    }
}
