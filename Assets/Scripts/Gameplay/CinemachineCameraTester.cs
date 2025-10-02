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

        private int WideShotCameraIndex = 1;
        private int LotusCameraIndex = 0;
        private int WhitmanCamerIndex = 2;
        
        // Start is called before the first frame update
        void Start()
        {
            cameras = new List<CinemachineVirtualCamera>(
                FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.InstanceID));
            
            EventBetter.Listen(this, (AltEnding.SpeakerChangedMessage msg) => OnSpeakerChanged(msg.SpeakerName));
        }

        void OnDestroy()
        {
        }

        private void ShowCamera(int cameraIndex)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].enabled = (i == cameraIndex);
            }
        }

        public void OnSpeakerChanged(string speakerName)
        {
            Debug.Log(speakerName);
            switch (speakerName)
            {
                case "McLoughlin":
                    // yep, she apparently goes by "McLoughlin" now.  
                    ShowCamera(LotusCameraIndex);
                    break;
                
                case "Whitman":
                    ShowCamera(WhitmanCamerIndex);
                    break;
                
                default:
                    ShowCamera(WideShotCameraIndex);
                    break;
            }
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
