using UnityEngine;

public class MassCloneTest : MonoBehaviour
{
    [SerializeField]
    private int instanceCount=50;
    [SerializeField]
    private GameObject instanceObject;
    void Start()
    {
        var currentPos=instanceObject.transform.position;
        currentPos+=Vector3.right;
        for(int i = 0; i < instanceCount; i++)
        {
            Instantiate(instanceObject,currentPos,Quaternion.identity);
            currentPos+=Vector3.right;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
