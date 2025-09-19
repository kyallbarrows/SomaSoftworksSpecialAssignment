using UnityEngine;
using UnityEngine.SceneManagement;

namespace AltEnding
{
    public class LoadSceneHelper : MonoBehaviour
    {
#if UseNA
        [NaughtyAttributes.Scene]
#endif
        [SerializeField]
        protected string sceneName;

        [SerializeField] protected LoadSceneMode loadSceneMode;
        [SerializeField] protected bool forceIfManagerUninitialised;
#if UseNA
        [NaughtyAttributes.Label("Transition?"),
         NaughtyAttributes.ShowIf(nameof(loadSceneMode), LoadSceneMode.Single)]
#endif
        [SerializeField]
        private bool doTransition = true;

        public void LoadScene()
        {
            if (SceneManagementSingleton.instance_Initialised)
            {
                if (doTransition && loadSceneMode == LoadSceneMode.Single)
                    SceneManagementSingleton.instance.LoadSceneWithTransition(sceneName);
                else SceneManagementSingleton.instance.LoadScene(sceneName, loadSceneMode);
            }
            else if (forceIfManagerUninitialised)
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
            }
        }
    }
}