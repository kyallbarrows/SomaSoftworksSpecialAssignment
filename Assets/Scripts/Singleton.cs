using System.Collections;
using System.Collections.Generic;using System.Data.SqlTypes;
using UnityEngine;

namespace SpecialAssignment
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Search for an existing instance in the scene
                    _instance = FindObjectOfType<T>();

                    // If no instance found, create a new GameObject and add the component
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                // Destroy duplicate instances
                Destroy(gameObject);
            }
            else
            {
                _instance = (T)this;
                DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
            }
        }
    }
}