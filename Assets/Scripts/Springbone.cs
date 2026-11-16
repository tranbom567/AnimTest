using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

public class SpringBone : MonoBehaviour
{
    public struct Bone
    {
        public float3 prevTip,currentTip,dx,force,velocity,localTipPos;
        public float boneLength;
    }
    [SerializeField]
    private float stiffness=10f;
    [SerializeField]
    private float dragForce=0.5f;
    [SerializeField]
    private float3 gravity=Vector3.down*9.81f;
    [SerializeField]
    private float mass=1;
    [SerializeField]
    private Vector3 boneAxis=Vector3.up;
    private Transform root;
    private NativeList<Bone> bonesList=new(Allocator.Persistent);
    private NativeList<SpringBoneCollider.SpringBoneColliderDataBlock>collidersDataBlock=new(Allocator.Persistent);
    private TransformAccessArray transformDataBlock;
    private JobHandle handle;
    void Start()
    {
        var transList=new List<Transform>();
        root=transform.parent;
        var currentTrans=transform;
        
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
        var springBoneCollidersList=FindObjectsByType<SpringBoneCollider>();
        if(springBoneCollidersList==null)
            return;
        foreach(var collider in springBoneCollidersList)
        {
            collidersDataBlock.Add(new SpringBoneCollider.SpringBoneColliderDataBlock()
            {
                localPos=root.InverseTransformPoint(collider.getPos),
                radius=collider.getRadius
            });
        }
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
            collidersDataBlock=collidersDataBlock.AsDeferredJobArray()
        };
        handle=boneJob.Schedule(transformDataBlock,handle);
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
    }
}
