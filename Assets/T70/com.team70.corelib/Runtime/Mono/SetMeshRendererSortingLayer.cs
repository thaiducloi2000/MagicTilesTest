using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class SetMeshRendererSortingLayer : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("renderer")] private Renderer _renderer;
 
    [SerializeField]
    private string sortingLayerName;
 
    [SerializeField]
    private int sortingOrder;
 
    public void Start()
    {
        _renderer.sortingLayerName = sortingLayerName;
        _renderer.sortingOrder = sortingOrder;
    }
}