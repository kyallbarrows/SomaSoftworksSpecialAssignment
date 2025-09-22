using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AltEnding
{
    public class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        public delegate void SingletonInstanceInitialized();

        protected override void Awake()
        {
            base.Awake();
            if (instance != null && Application.isPlaying) DontDestroyOnLoad(instance);
        }
    }
}