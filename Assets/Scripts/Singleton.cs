using System.Collections;
using System.Collections.Generic;using System.Data.SqlTypes;
using UnityEngine;

namespace SpecialAssignment
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        void Awake()
        {
            if (Instance)
            {
                Debug.LogError($"There is more than one instance of {typeof(T).Name}");
                Destroy(gameObject);
                return;
            }

            Instance = this as T;

            if (!Instance)
            {
                Debug.Log($"Singleton {gameObject.name} is not of type {typeof(T).Name}");
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}