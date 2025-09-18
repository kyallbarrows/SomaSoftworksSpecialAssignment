using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace SpecialAssignment
{
    public class CinemachineCameraTester : MonoBehaviour
    {
        private List<CinemachineVirtualCamera> cameras;
        
        // Start is called before the first frame update
        void Start()
        {
            cameras = new List<CinemachineVirtualCamera>(
                FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.InstanceID));
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 50;
            
            int buttonWidth = 250;
            int x = 300;
            
            foreach (CinemachineVirtualCamera vcam in cameras)
            {
                if (GUI.Button(new Rect(x, 100, buttonWidth, 70), vcam.name))
                {
                    foreach (var turnItOff in cameras)
                    {
                        turnItOff.enabled = false;
                    }

                    vcam.enabled = true;
                    Debug.Log($"Setting camera to {vcam.name}");
                }
                
                x += (int)(buttonWidth * 1.2f);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
