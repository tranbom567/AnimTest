using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
[BurstCompile]
public struct SpringBoneJobProcessing : IJobParallelForTransform
{
    public NativeArray<SpringBone.Bone>bonesList;
    public float stiffness,dragForce,mass;
    public float3 boneAxis;
    public Matrix4x4 rootLocalToWorld;
    public float3 gravity;
    public float dt;
    public void Execute(int index, TransformAccess transform)
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
        SpringBone.Bone execCurrentBone=bonesList[index];
        execCurrentBone.currentTip=rootLocalToWorld.MultiplyPoint3x4(execCurrentBone.localTipPos);
        execCurrentBone.dx=execCurrentBone.currentTip-execCurrentBone.prevTip;

        execCurrentBone.force=stiffness*execCurrentBone.dx;
        execCurrentBone.force-=dragForce*execCurrentBone.velocity;
        execCurrentBone.force+=gravity*mass;
        execCurrentBone.velocity+=execCurrentBone.force*dt;
        execCurrentBone.prevTip+=execCurrentBone.velocity*dt;

        execCurrentBone.prevTip=transform.position+((Vector3)execCurrentBone.prevTip-transform.position).normalized*execCurrentBone.boneLength;
        transform.rotation=Quaternion.FromToRotation(transform.rotation*boneAxis,((Vector3)execCurrentBone.prevTip-transform.position).normalized)
        *transform.rotation;
        bonesList[index]=execCurrentBone;
    }
}
