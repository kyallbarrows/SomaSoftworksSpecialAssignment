using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform target; // The other character
    public string headBoneName = "CC_Base_Head"; // Name of the head bone
    private Transform headBone;
    public float lookSpeed = 5f;
    public bool enableLookAt = true;
    
    void Start()
    {
        // Automatically find the head bone by name
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.name == headBoneName)
            {
                headBone = child;
                Debug.Log("Found head bone: " + child.name);
                break;
            }
        }
        
        if (headBone == null)
        {
            Debug.LogError("Could not find head bone named: " + headBoneName);
        }
    }
    
    void LateUpdate()
    {
        if (enableLookAt && target != null && headBone != null)
        {
            Vector3 direction = target.position - headBone.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            headBone.rotation = Quaternion.Slerp(headBone.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }
    }
}