using Unity.Mathematics;
using UnityEngine;
[ExecuteInEditMode]
public class SpringBoneCollider : MonoBehaviour
{
    public struct SpringBoneColliderDataBlock
    {
        public float3 position;
        public float radius;
    }
    [SerializeField]
    private float radius=.07f;
    public Vector3 getPos=>transform.position;
    public float getRadius=>radius*transform.localScale.magnitude;
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color=Color.red;
        Gizmos.DrawWireSphere(transform.position,getRadius);
    }
}
