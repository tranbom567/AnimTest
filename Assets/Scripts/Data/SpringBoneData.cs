using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
[BurstCompile]
public struct SpringBoneData
{
    public float3 prevTip,currentTip,localTipPos,force,dx,velocity,head;
}
public static class SpringMathExtensions
{
    public static float3 normalizeFloat3(this float3 vec)
    {
        return 1/vec.VecMag()*vec;
    }
    public static float3 VecMag(this float3 vec)
    {
        return Mathf.Sqrt(vec.x*vec.x+vec.y*vec.y+vec.z*vec.z);
    }
}
