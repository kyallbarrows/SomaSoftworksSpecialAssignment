using System.Collections.Generic;
using UnityEngine;

namespace AltEnding
{
    public class PSB_SetGameObjectsActive : PlatformSpecificBehavior
    {
        [SerializeField] protected List<GameObject> gameObjects = new List<GameObject>();
        [SerializeField] protected bool platformCheckPassState = true;

        protected override void PlatformCheckPass()
        {
            //Do custom stuff here
            foreach (GameObject gameObject in gameObjects) gameObject.SetActive(platformCheckPassState);
        }

        protected override void PlatformCheckFail()
        {
            //Do custom stuff here
            foreach (GameObject gameObject in gameObjects) gameObject.SetActive(!platformCheckPassState);
        }
    }
}