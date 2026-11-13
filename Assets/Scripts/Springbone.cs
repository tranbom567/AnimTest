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
    public bool lockX{get;set;}
    public bool lockY{get;set;}
    public bool lockZ{get;set;}
    private float boneLength;
    //private float3 prevTip,currentTip,localTipPos,force,dx,velocity,head;
    private NativeArray<float3>springDataBlock=new(new float3[7],Allocator.Persistent);
    private Vector3 finalRot;
    private SpringBoneJob spbJob;
    void Start()
    {
        children=transform.GetChild(0);
        springDataBlock[2]=children.localPosition;
        boneLength=Vector3.Magnitude(children.position-transform.position);
        springDataBlock[1]=transform.TransformPoint(springDataBlock[2]);
        springDataBlock[0]=springDataBlock[1];
    }
    void LateUpdate() 
    {
        springDataBlock[6]=transform.position;
        springDataBlock[1]=transform.TransformPoint(springDataBlock[2]);
        spbJob=new()
        {
            springDataBlock=springDataBlock,
            stiffness=stiffness,
            boneLength=boneLength,
            dragforce=dragForce,
            dt=Time.deltaTime
        };
    spbJob.Schedule().Complete();
    finalRot=Quaternion.FromToRotation(transform.TransformDirection(boneAxis),((Vector3)(springDataBlock[6]-springDataBlock[0])).normalized).eulerAngles;
    if(lockX)
        finalRot.x=0;
    if(lockY)
        finalRot.y=0;
    if(lockZ)
        finalRot.z=0;
    children.SetPositionAndRotation(springDataBlock[0],Quaternion.Euler(finalRot));
    }
    
    void OnDrawGizmos()
    {
        
        Gizmos.DrawSphere(springDataBlock[1],0.005f);
        Gizmos.color=Color.red;
        Gizmos.DrawSphere(springDataBlock[0],0.005f);
        
    }
}