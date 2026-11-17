using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.XR;

public class SpringBone : MonoBehaviour
{
    public struct Bone
    {
        public float3 prevTip,currentTip,dx,force,velocity,localTipPos;
        public float boneLength;
    }
    [Header("Spring Bone Force Settings")]
    [SerializeField]
    private float stiffness=10f;
    [SerializeField]
    private float dragForce=0.5f;
    [SerializeField]
    private float3 gravity=Vector3.down*9.81f;
    [SerializeField]
    private float mass=1;
    [SerializeField]
    private bool windEffect=true;
    [SerializeField]
    private Vector3 boneAxis=Vector3.up;
    [Header("Spring Bone lock twists Settings")]
    [SerializeField]
    private bool lockTwist=false;
    [Header("Don't need to set boneUp if lockTwist is disabled")]
    [SerializeField]
    private Vector3 boneUp=Vector3.forward;

    private Transform root;
    private NativeList<Bone> bonesList=new(Allocator.Persistent);
    private TransformAccessArray transformDataBlock;

    
    private JobHandle handle;
    void Start()
    {
        root=transform.parent;
        var currentTrans=transform;
        var transList=new List<Transform>();
        //init bone
        while (currentTrans.childCount > 0)
        {
            var childBone=currentTrans.GetChild(0);
            var boneInit=new Bone();
            var putChildToRootLocal=root.InverseTransformPoint(childBone.position);
            boneInit.localTipPos=putChildToRootLocal;
            boneInit.currentTip=root.TransformPoint(boneInit.localTipPos);
            boneInit.prevTip=boneInit.currentTip;
            boneInit.boneLength=Vector3.Magnitude(boneInit.currentTip-(float3)currentTrans.position);
            bonesList.Add(boneInit);
            transList.Add(currentTrans);
            currentTrans=childBone;
        }
        var lastBoneLength=bonesList[bonesList.Count-1].boneLength;
        //check for leaf bone
        if (currentTrans.childCount == 0)
        {
            var boneInit=new Bone();
            var putChildToRootLocal=root.InverseTransformPoint(currentTrans.position+currentTrans.TransformDirection(boneAxis)*lastBoneLength);
            boneInit.localTipPos=putChildToRootLocal;
            boneInit.currentTip=root.TransformPoint(boneInit.localTipPos);
            boneInit.prevTip=boneInit.currentTip;
            boneInit.boneLength=lastBoneLength;
            bonesList.Add(boneInit);
            transList.Add(currentTrans);
        }
        transformDataBlock=new(transList.ToArray());
        //init colliders
        
       
    }

    void LateUpdate()
    {
        
        
        SpringBoneJobProcessing boneJob = new()
        {
            boneAxis=boneAxis,
            stiffness=stiffness,
            dragForce=dragForce,
            bonesList=bonesList.AsDeferredJobArray(),
            rootLocalToWorld=root.localToWorldMatrix,
            dt=Time.deltaTime,
            gravity=gravity,
            mass=mass,
            windForce=WindZoneData.getInstance.getWindForce,
            collidersDataBlock=SpringBoneColliderManager.getInstance.GetColliderDataBlocks,
            rootRotation=root.rotation,
            boneUp=boneUp,
            lockTwist=lockTwist,
            windEffect=windEffect
        };
        handle=boneJob.Schedule(transformDataBlock,handle);
        handle.Complete();
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position,WindZoneData.getInstance.getWindForce);
        handle.Complete();
        foreach(var bone in bonesList){
            Gizmos.color=Color.white;
            Gizmos.DrawSphere(bone.prevTip,0.005f);
            Gizmos.color=Color.red;
            Gizmos.DrawSphere(bone.currentTip,0.005f);
        }
    }
    void OnDestroy()
    {
        handle.Complete();
        if(transformDataBlock.isCreated)
            transformDataBlock.Dispose();
        if(bonesList.IsCreated)
            bonesList.Dispose();
        SpringBoneColliderManager.getInstance.resetAllColliders();
    }
}
