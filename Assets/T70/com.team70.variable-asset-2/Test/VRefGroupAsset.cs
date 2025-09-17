using System;
using UnityEngine;
using nano.va2;

[Serializable] public class VRefGroup
{
    public Transform pTransform;
    public Transform cTransform;
    public MonoBehaviour cMono;
    public Animator animator;
}

[CreateAssetMenu(fileName = "vrefgroup.asset", menuName = "VAsset2/Custom/VRefGroup")]
public class VRefGroupAsset : VClassAssetT<VRefGroup>
{

}

