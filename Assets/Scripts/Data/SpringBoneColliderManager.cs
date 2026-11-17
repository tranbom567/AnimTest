using Unity.Collections;
using UnityEngine;

public class SpringBoneColliderManager : MonoBehaviour
{
    private NativeArray<SpringBoneCollider.SpringBoneColliderDataBlock>collidersDataBlock;
    public NativeArray<SpringBoneCollider.SpringBoneColliderDataBlock>GetColliderDataBlocks=>collidersDataBlock;
    private SpringBoneCollider[] springBoneCollidersList;
    private static SpringBoneColliderManager instance;
    public static SpringBoneColliderManager getInstance=>instance;
    void Awake()
    {
        instance=this; 
        DontDestroyOnLoad(gameObject); 
    }
    void Start()
    {
        springBoneCollidersList=FindObjectsByType<SpringBoneCollider>();
        collidersDataBlock=new(new SpringBoneCollider.SpringBoneColliderDataBlock[springBoneCollidersList.Length],Allocator.Persistent);
    }

    // Update is called once per frame
    void Update()
    {
        if (springBoneCollidersList != null)
        {
            var currentIndex=0;
            foreach(var collider in springBoneCollidersList){
                collidersDataBlock[currentIndex]=new()
                {
                    position=collider.getPos,
                    radius=collider.getRadius
                };
                currentIndex++;
            }
        }
    }
    public void resetAllColliders()
    {
        if(collidersDataBlock.IsCreated)
            collidersDataBlock.Dispose();
        springBoneCollidersList=FindObjectsByType<SpringBoneCollider>();
        collidersDataBlock=new(new SpringBoneCollider.SpringBoneColliderDataBlock[springBoneCollidersList.Length],Allocator.Persistent);
    }
    void OnDestroy()
    {
        if(collidersDataBlock.IsCreated)
            collidersDataBlock.Dispose();
    }
}
