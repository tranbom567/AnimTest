using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
[BurstCompile]
public struct SpringBoneJob:IJob
{
    public float stiffness,dragforce,boneLength,dt;
    //private float3 prevTip,currentTip,localTipPos,force,dx,velocity,head;
    public NativeArray<float3>springDataBlock;
    public void Execute()
    {
        springDataBlock[4]=springDataBlock[1]-springDataBlock[0];
        springDataBlock[3]=springDataBlock[4]*stiffness;
        springDataBlock[3]-=dragforce*springDataBlock[5];
        springDataBlock[5]+=springDataBlock[3]*dt;
        springDataBlock[0]+=springDataBlock[5]*dt;
        springDataBlock[0]=springDataBlock[6]+normalizeFloat3(springDataBlock[0]-springDataBlock[6])*boneLength;
    }
    private float3 normalizeFloat3(float3 vec)
    {
        return 1/VecMag(vec)*vec;
    }
    private float3 VecMag(float3 vec)
    {
        return Mathf.Sqrt(vec.x*vec.x+vec.y*vec.y+vec.z*vec.z);
    }
}
