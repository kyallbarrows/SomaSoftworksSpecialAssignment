using System;
using UnityEngine;

namespace SpecialAssignment
{
    public class App : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}