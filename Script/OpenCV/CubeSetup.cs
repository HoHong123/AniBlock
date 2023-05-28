using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSetup : MonoBehaviour {

    public float speed = 30f;
    public bool isColored = false;

    public CubeState[,] cubes = new CubeState[8, 8];
    [System.Serializable] public class CubeState
    {
        private MeshRenderer meshRenderer;
        
        public Color GetColor()
        {
            return meshRenderer.material.color;
        }

        public void Initialize(MeshRenderer mesh)
        {
            mesh.material.color = new Color(0, 0, 0, 0);

            meshRenderer = mesh;
        }

        public void ChangeMeshColor(Color color)
        {
            meshRenderer.material.color = color;
        }
    }


    // Use this for initialization
    void Awake ()
    {
        for(int y = 0; y < 8; y++)
        {
            for(int x = 0; x < 8; x++)
            {
                cubes[y, x] = new CubeState();
                cubes[y, x].Initialize(transform.GetChild(x + (y * 8)).GetComponent<MeshRenderer>());
            }
        }
    }
    
    private void Update()
    {
        transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));
    }

    public void ResetCubeColor()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                cubes[y, x].ChangeMeshColor(new Color(0, 0, 0, 0));
            }
        }
    }

    public void ChangeCubeColorWithArray(Color[] color)
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                cubes[y, x].ChangeMeshColor(color[x + (y * 8)]);
            }
        }
    }
}
