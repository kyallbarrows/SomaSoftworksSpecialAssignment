using UnityEngine;
using UnityEngine.Events;
#if UseNA
using NaughtyAttributes;
#endif

public class ClickthroughScrollableWindow : MonoBehaviour
{
    [SerializeField]
    private bool doDebugs;
    [SerializeField]
    private UnityEvent clickthroughEvent;
#if UseNA
    [ReadOnly]
#endif
    [SerializeField]
    private double initializeTime;
#if UseNA
    [ReadOnly]
#endif
    [SerializeField]
    private bool dragConfirmed;

    public void InitializeDrag()
    {
        dragConfirmed = false;
        if(doDebugs) Debug.Log($"<color='cyan'>Initialize drag at {Time.timeAsDouble}</color>", this);
    }

    public void Click()
    {
        if (doDebugs) Debug.Log($"<color='yellow'>Recieved Click Event; Drag is {dragConfirmed}</color>", this);
        if (!dragConfirmed)
        {
            clickthroughEvent?.Invoke();
        }
    }

    public void BeginDrag()
    {
        dragConfirmed = true;
        if (doDebugs) Debug.Log($"Drag Confirmed");
    }
}
