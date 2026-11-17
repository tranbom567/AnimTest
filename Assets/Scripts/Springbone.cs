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
    [SerializeField]
    private bool lockTwist=false;
    [SerializeField]
    private Vector3 boneUp=Vector3.forward;
    private Transform root;
    private NativeList<Bone> bonesList=new(Allocator.Persistent);
    private TransformAccessArray transformDataBlock;
    private SpringBoneCollider[] springBoneCollidersList;
    private NativeArray<SpringBoneCollider.SpringBoneColliderDataBlock>collidersDataBlock;
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
        springBoneCollidersList=FindObjectsByType<SpringBoneCollider>();
        collidersDataBlock=new(new SpringBoneCollider.SpringBoneColliderDataBlock[springBoneCollidersList.Length],Allocator.Persistent);
       
    }

    void LateUpdate()
    {
        
        if (springBoneCollidersList != null)
        {
            var currentIndex=0;
            foreach(var collider in springBoneCollidersList){
                collidersDataBlock[currentIndex]=new()
                {
                    position=collider.getPos,
                    radius=collider.getRadius
                };
                currentIndex++;
            }
        }
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
            collidersDataBlock=collidersDataBlock,
            rootRotation=root.rotation,
            boneUp=boneUp,
            lockTwist=lockTwist
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
        if(collidersDataBlock.IsCreated)
            collidersDataBlock.Dispose();
    }
}
