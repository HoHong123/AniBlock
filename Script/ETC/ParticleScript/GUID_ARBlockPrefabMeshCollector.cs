using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUID_ARBlockPrefabMeshCollector : MonoBehaviour
{
    [SerializeField] private Renderer ARGuideBlockMesh;
    [SerializeField] private Renderer ARGuideLineMesh;
    public void Set_BlockColor(Color _color, Color _reversColor)
    {
        _color.a = 0.5f;
        ARGuideBlockMesh.material.color = _color;
        ARGuideLineMesh.material.color = _reversColor;
    }
}
