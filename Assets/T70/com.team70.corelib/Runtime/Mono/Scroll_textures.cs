using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll_textures : MonoBehaviour
{
    public float scrollX = 0.5f;
    public float scrollY = 0.5f;
    public Material mat;

    void Awake()
    {
        GetMaterial();
    }
    
    void GetMaterial()
    {
        if (mat != null) return;
        var r = GetComponent<Renderer>();
        if (r != null) mat = r.sharedMaterial;
    }
    
    void Update ()
    {
        mat.mainTextureOffset = new Vector2 (scrollX, scrollY) * Time.deltaTime;
    }
}
