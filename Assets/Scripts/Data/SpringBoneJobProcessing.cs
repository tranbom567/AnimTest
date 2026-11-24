using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
[BurstCompile]
public struct SpringBoneJobProcessing : IJobParallelForTransform
{
    public NativeArray<SpringBone.Bone> bonesList;
    [ReadOnly]
    public NativeArray<SpringBoneCollider.SpringBoneColliderDataBlock> collidersDataBlock;
    public float stiffness, dragForce, mass;
    public float3 boneAxis;
    public Matrix4x4 rootLocalToWorld;
    public quaternion rootRotation;
    public bool windEffect;
    public float3 gravity;
    public float3 windForce;
    public float dt;
    private Vector3 targetDirection;
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
        SpringBone.Bone execCurrentBone = bonesList[index];
        execCurrentBone.currentTip = rootLocalToWorld.MultiplyPoint3x4(execCurrentBone.localTipPos);
        execCurrentBone.dx = execCurrentBone.currentTip - execCurrentBone.prevTip;

        execCurrentBone.force = stiffness * execCurrentBone.dx;
        execCurrentBone.force -= dragForce * execCurrentBone.velocity;
        execCurrentBone.force += gravity * mass;
        if(windEffect)
            execCurrentBone.force += windForce;
        float3 colliderWorldPos;

        execCurrentBone.velocity += execCurrentBone.force * dt;
        execCurrentBone.prevTip += execCurrentBone.velocity * dt;

        
        foreach (var colliderData in collidersDataBlock)
        {
            colliderWorldPos = colliderData.position;
            var distance=Vector3.Distance(execCurrentBone.prevTip, colliderWorldPos);
            if (distance < colliderData.radius)
            {
                execCurrentBone.prevTip += (float3)(colliderData.radius - distance) * ((Vector3)(execCurrentBone.prevTip - colliderWorldPos)).normalized;
            }
        }
        execCurrentBone.prevTip = transform.position + ((Vector3)execCurrentBone.prevTip - transform.position).normalized * execCurrentBone.boneLength;
        targetDirection = ((Vector3)execCurrentBone.prevTip - transform.position).normalized;

        transform.rotation = Quaternion.FromToRotation(execCurrentBone.initialRotation * boneAxis, targetDirection)*execCurrentBone.initialRotation;
        bonesList[index] = execCurrentBone;
    }
}
