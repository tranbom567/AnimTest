using System;
using UnityEngine;

public class ChainBuilder : MonoBehaviour
{
    [SerializeField]
    private float stiffness=10;
    [SerializeField]
    private float dragForce=0.5f;
    [SerializeField]
    private Vector3 boneAxis=Vector3.left;
    [SerializeField]
    private bool lockX,lockY,lockZ;
    void Start()
    {
        Transform root=transform;

        Springbone currentBone;
        while (root.childCount > 0)
        {
            currentBone=root.gameObject.AddComponent<Springbone>();
            currentBone.stiffness=stiffness;
            currentBone.dragForce=dragForce;
            currentBone.boneAxis=boneAxis;
            currentBone.lockX=lockX;
            currentBone.lockY=lockY;
            currentBone.lockZ=lockZ;
            root=root.GetChild(0);
            
        }
    }
}
