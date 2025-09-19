using UnityEngine;
using UnityEngine.Events;

namespace AltEnding
{
    public class PSB_UnityEvents : PlatformSpecificBehavior
    {
        [SerializeField] protected UnityEvent passEvent = new UnityEvent();
        [SerializeField] protected UnityEvent failEvent = new UnityEvent();

        protected override void PlatformCheckPass()
        {
            //Do custom stuff here
            if (passEvent != null) passEvent.Invoke();
        }

        protected override void PlatformCheckFail()
        {
            //Do custom stuff here
            if (failEvent != null) failEvent.Invoke();
        }
    }
}