using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;

namespace SpecialAssignment
{
    public class SplashScreen : MonoBehaviour
    {
        [FormerlySerializedAs("HangTimeSeconds")] [SerializeField] private float hangTimeSeconds;

        // Start is called before the first frame update
        void Start()
        {
            _ = UniTask.Delay((int)(hangTimeSeconds * 1000f)).ContinueWith(() =>
            {
                NavigationManager.LoadScene(NavigationManager.MAINMENU);
            });
        }
    }
}
