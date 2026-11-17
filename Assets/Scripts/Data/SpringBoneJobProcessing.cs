using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
[BurstCompile]
public struct SpringBoneJobProcessing : IJobParallelForTransform
{
    public NativeArray<SpringBone.Bone> bonesList;
    public NativeArray<SpringBoneCollider.SpringBoneColliderDataBlock> collidersDataBlock;
    public float stiffness, dragForce, mass;
    public float3 boneAxis, boneUp;
    public Matrix4x4 rootLocalToWorld;
    public quaternion rootRotation;
    public bool lockTwist,windEffect;
    public float3 gravity;
    public float3 windForce;
    public float dt;
    private Vector3 targetDirection, projectedRefUp, projectedCurUp, refUp, curUp;
    private Quaternion finalRot, clearTwist;
    private const float epsilon = .001f;
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
            if (Vector3.Distance(execCurrentBone.prevTip, colliderWorldPos) < colliderData.radius)
            {
                execCurrentBone.prevTip += (float3)(colliderData.radius - Vector3.Distance(execCurrentBone.prevTip, colliderWorldPos)) * ((Vector3)(execCurrentBone.prevTip - colliderWorldPos)).normalized;
            }
        }
        execCurrentBone.prevTip = transform.position + ((Vector3)execCurrentBone.prevTip - transform.position).normalized * execCurrentBone.boneLength;
        targetDirection = ((Vector3)execCurrentBone.prevTip - transform.position).normalized;
        finalRot = Quaternion.FromToRotation(transform.rotation * boneAxis, targetDirection) * transform.rotation;
        if (lockTwist)
        {
            refUp = (Quaternion)rootRotation * boneUp;
            curUp = finalRot * boneUp;
            projectedRefUp = Vector3.ProjectOnPlane(refUp, targetDirection);
            projectedCurUp = Vector3.ProjectOnPlane(curUp, targetDirection);
            if (projectedRefUp.sqrMagnitude > epsilon && projectedCurUp.sqrMagnitude > epsilon)
            {
                clearTwist = Quaternion.FromToRotation(projectedCurUp, projectedRefUp);
                finalRot = clearTwist * finalRot;
            }
        }

        transform.rotation = finalRot;
        bonesList[index] = execCurrentBone;
    }
}
