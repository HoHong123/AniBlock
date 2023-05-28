using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GongJunChook : MonoBehaviour {

    public float _speed = 10.0f;
    public List<GameObject> GongJunList = new List<GameObject>();

    private void Update()
    {
        for (int i = 0; i < GongJunList.Count; i++)
        {
            GongJunList[i].transform.RotateAround(this.gameObject.transform.position, Vector3.up, _speed * Time.deltaTime);
        }
    }
}
