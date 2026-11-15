using System.Collections.Generic;
using Unity.Collections;
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
    private TransformAccessArray transformDataBlock;
    void Start()
    {
        var transList=new List<Transform>();
        root=transform.root;
        var currentTrans=transform;
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
    }

    void LateUpdate()
    {
       /* for(int i = 0; i < bonesList.Count; i++)
        {
            execCurrentBone=bonesList[i];

            execCurrentBone.currentTip=root.TransformPoint(execCurrentBone.localTipPos);
            execCurrentBone.dx=execCurrentBone.currentTip-execCurrentBone.prevTip;

            execCurrentBone.force=stiffness*execCurrentBone.dx;
            execCurrentBone.force-=dragForce*execCurrentBone.velocity;

            execCurrentBone.velocity+=execCurrentBone.force*Time.deltaTime;
            execCurrentBone.prevTip+=execCurrentBone.velocity*Time.deltaTime;

            execCurrentBone.prevTip=execCurrentBone.currentBone.position+(execCurrentBone.prevTip-execCurrentBone.currentBone.position).normalized*execCurrentBone.boneLength;
            execCurrentBone.currentBone.rotation=Quaternion.FromToRotation(execCurrentBone.currentBone.TransformDirection(boneAxis),(execCurrentBone.prevTip-execCurrentBone.currentBone.position).normalized)
            *execCurrentBone.currentBone.rotation;

        }*/
        SpringBoneJobProcessing boneJob = new()
        {
            boneAxis=boneAxis,
            stiffness=stiffness,
            dragForce=dragForce,
            bonesList=bonesList.AsDeferredJobArray(),
            rootLocalToWorld=root.localToWorldMatrix,
            dt=Time.deltaTime,
            gravity=gravity,
            mass=mass
        };
        boneJob.Schedule(transformDataBlock).Complete();
    }
    
    void OnDestroy()
    {
        if(transformDataBlock.isCreated)
            transformDataBlock.Dispose();
        if(bonesList.IsCreated)
            bonesList.Dispose();
    }
}
