using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
namespace AltEnding
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        private static object _lock = new object();

        private static event Action instanceInitializedAction;

        public static T instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning(
                        $"[Singleton] Instance '{typeof(T).Name}' already destroyed on application quit.  Won't create again - returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (_instance != null)
                        {
                            Debug.Log(
                                $"[Singleton] An instance of {typeof(T).Name} was found in a loaded scene.  Using that instance: " +
                                _instance.gameObject.name);
                        }
                        else
                        {
#if UNITY_EDITOR
                            //Try to load from assets
                            string[] singletonGUIIDs =
                                UnityEditor.AssetDatabase.FindAssets($"t:prefab {typeof(T).Name}",
                                    new[] { "Assets/_Prefabs" });

                            if (singletonGUIIDs.Length > 0)
                            {
                                string singletonPrefabPath =
                                    UnityEditor.AssetDatabase.GUIDToAssetPath(singletonGUIIDs[0]);
                                T singletonPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(singletonPrefabPath);
                                if (singletonPrefab != null)
                                {
                                    _instance = Instantiate(singletonPrefab);
                                    _instance.name = typeof(T).Name;
                                    Debug.Log(
                                        $"[Singleton] An instance of {typeof(T).Name} is needed in the scene, so one was loaded from the _Prefabs folder.");
                                }
                                else
                                    Debug.LogWarning(
                                        $"[Singleton] The _Prefabs folder was searched for an instance of {typeof(T).Name}.  A prefab was found with the correct name, but it did not have a {typeof(T).Name} component on it!");
                            }
#endif

                            if (_instance == null)
                            {
                                GameObject singleton = new GameObject();
                                _instance = singleton.AddComponent<T>();
                                singleton.name = typeof(T).Name;
                                Debug.Log(
                                    $"[Singleton] An instance of {typeof(T).Name} is needed in the scene, but none was found in a loaded scene or the _Prefabs folder, so one with default values was created.");
                            }
                        }

                        instanceInitializedAction?.Invoke();
                        instanceInitializedAction = null;
                    }

                    return _instance;
                }
            }
        }

        public static bool instance_Initialised
        {
            get { return (_instance != null && !applicationIsQuitting); }
        }

        public static void WhenInitialized(Action executeWhenInitialized)
        {
            if (instance_Initialised)
            {
                executeWhenInitialized?.Invoke();
            }
            else
            {
                instanceInitializedAction += executeWhenInitialized;
            }
        }

        public virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private static bool applicationIsQuitting = false;

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        public void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        protected virtual void Awake()
        {
            //Debug.Log($"[Singleton] Awake() called on an instance of {typeof(T).Name}");
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)(MonoBehaviour)this;
                    instanceInitializedAction?.Invoke();
                    instanceInitializedAction = null;
                    //Debug.Log($"[Singleton] Set _instance to object of type {typeof(T).Name} in awake call.", _instance);
                }
            }

            ClearDuplicates();
        }

        void ClearDuplicates()
        {
            //Debug.Log($"[Singleton] Clearing duplicate instances of {typeof(T).Name}");
            //Search for duplicates and remove them
            Object[] objects = FindObjectsOfType(typeof(T));
            //Make sure _instance isn't null if there's at least 1 instance
            if (objects.Length > 0 && _instance == null) _instance = (T)objects[0];
            if (objects.Length > 1)
            {
                //There are duplicate instances of the singleton, remove the ones that aren't _instance
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] != _instance)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(objects[i]);
                        }
                        else
                        {
                            DestroyImmediate(objects[i]);
                        }
                    }
                }
            }
        }
    }
}