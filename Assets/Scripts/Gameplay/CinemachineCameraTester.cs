using System;
using System.Collections;
using System.Collections.Generic;
using AltEnding;
using Articy.Special_Assignment;
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
        
        private void Start()
        {
            cameras = new List<CinemachineVirtualCamera>(
                FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.InstanceID));
            
            ArticyFlowController.SpecialActionObjectReached += OnSpecialAction;
        }

        private void OnDestroy()
        {
            ArticyFlowController.SpecialActionObjectReached -= OnSpecialAction;
        }

        private void ShowCamera(int cameraIndex)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].enabled = (i == cameraIndex);
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

        private void OnSpecialAction(string fullAction)
        {
            var actionParts = fullAction.Split('|');
            if (actionParts.Length < 8 || !actionParts[2].Equals("CameraAngle"))
                return;
            
            var action = Enum.Parse<Camera_Angle_01>(actionParts[3]);
            int cameraIndex = action switch
            {
                Camera_Angle_01.MC_CU => WhitmanCamerIndex,
                Camera_Angle_01.Char2_CU => LotusCameraIndex,
                _ => WideShotCameraIndex
            };
            ShowCamera(cameraIndex);
            Debug.Log($"[CameraAngle] {action}");
        }
    }
}
