using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
[BurstCompile]
public struct SpringBoneJob:IJob
{
    public float stiffness,dragforce,boneLength,dt;
    //private float3 prevTip,currentTip,localTipPos,force,dx,velocity,head;
    public NativeReference<SpringBoneData>springDataBlock;
    
    public void Execute()
    {
        var jobSpringData=springDataBlock.Value;
        jobSpringData.dx=jobSpringData.currentTip-jobSpringData.prevTip;
        jobSpringData.force=jobSpringData.dx*stiffness;
        jobSpringData.force-=dragforce*jobSpringData.velocity;
        jobSpringData.velocity+=jobSpringData.force*dt;
        jobSpringData.prevTip+=jobSpringData.velocity*dt;
        jobSpringData.prevTip=jobSpringData.head+(jobSpringData.prevTip-jobSpringData.head).normalizeFloat3()*boneLength;
        springDataBlock.Value=jobSpringData;
    }
    
}
