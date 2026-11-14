using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
public class Springbone : MonoBehaviour
{
    private Transform children;
    public float stiffness{get;set;}
    public float dragForce{get;set;}
    public Vector3 boneAxis{get;set;}
    public Vector3 minClampRot{get;set;}
    public Vector3 maxClampRot{get;set;}
    public bool lockX{get;set;}
    public bool lockY{get;set;}
    public bool lockZ{get;set;}
    private float boneLength;
    //private float3 prevTip,currentTip,localTipPos,force,dx,velocity,head;
    private NativeReference<SpringBoneData>springDataBlock=new(Allocator.Persistent);
    private Vector3 finalRot;
    private SpringBoneJob spbJob;
    private SpringBoneData mainBoneData;
    void Start()
    {
        children=transform.GetChild(0);
        boneLength=Vector3.Magnitude(children.position-transform.position);
        var localTipWorld=transform.TransformPoint(children.localPosition);
        mainBoneData=new SpringBoneData()
        {
            localTipPos=children.localPosition,
            currentTip=localTipWorld,
            prevTip=localTipWorld
        };
    }
    void LateUpdate() 
    {
        mainBoneData.head=transform.position;
        mainBoneData.currentTip=transform.TransformPoint(mainBoneData.localTipPos);
        springDataBlock.Value=mainBoneData; //pass data as reference
        spbJob=new()
        {
            springDataBlock=springDataBlock,
            stiffness=stiffness,
            boneLength=boneLength,
            dragforce=dragForce,
            dt=Time.deltaTime
        };
    spbJob.Schedule().Complete();
    mainBoneData=springDataBlock.Value; //receive processed data
    finalRot=Quaternion.FromToRotation(transform.TransformDirection(boneAxis),((Vector3)(mainBoneData.head-mainBoneData.prevTip)).normalized).eulerAngles;
    if(lockX)
        finalRot.x=0;
    if(lockY)
        finalRot.y=0;
    if(lockZ)
        finalRot.z=0;
    finalRot.x=Mathf.Clamp(finalRot.x,minClampRot.x,maxClampRot.x);
    finalRot.y=Mathf.Clamp(finalRot.y,minClampRot.y,maxClampRot.y);
    finalRot.z=Mathf.Clamp(finalRot.z,minClampRot.z,maxClampRot.z);
    children.SetPositionAndRotation(mainBoneData.prevTip,Quaternion.Euler(finalRot));
    }
    
    void OnDrawGizmos()
    {
        
        Gizmos.DrawSphere(mainBoneData.currentTip,0.005f);
        Gizmos.color=Color.red;
        Gizmos.DrawSphere(mainBoneData.prevTip,0.005f);
        
    }
}