using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    public Transform source; // Object (bone) that will do the looking
    public Transform target; // The target object (bone)
    public float lookSpeed = 5f;
    public bool enableLookAt = true;
    
    private float lookAtProgress = 0f;
    
    private void LateUpdate()
    {
        if (source == null || target == null)
            return;
        
        Vector3 direction = target.position - source.position;
        var lookRotation = Quaternion.LookRotation(direction);
        
        if (enableLookAt)
            lookAtProgress = Mathf.Min(lookAtProgress + Time.deltaTime, lookSpeed);
        else
            lookAtProgress = Mathf.Max(0f, lookAtProgress - Time.deltaTime);
        
        source.rotation = Quaternion.Slerp(source.rotation, lookRotation, lookAtProgress / lookSpeed);
    }
}