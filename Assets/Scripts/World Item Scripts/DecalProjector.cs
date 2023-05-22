using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DecalProjector : MonoBehaviour
{
    private BoxCollider _projectorBox;
    private MeshRenderer _renderer;
    private static readonly int WorldToProjector = Shader.PropertyToID("_WorldToProjector");
    
    private bool _cameraChecked;

    void Start()
    {
        _projectorBox = GetComponent<BoxCollider>();
        _renderer = GetComponent<MeshRenderer>();
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }
}
