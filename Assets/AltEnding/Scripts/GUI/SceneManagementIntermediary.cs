using UnityEngine;

namespace AltEnding.GUI
{
	public class SceneManagementIntermediary : MonoBehaviour
	{
#if UseNA
        [NaughtyAttributes.Label("Scene"), NaughtyAttributes.Scene]
#endif
		[SerializeField]
		private string inspectorSceneName;
#if UseNA
        [NaughtyAttributes.Label("Transition?")]
#endif
		[SerializeField]
		private bool doTransition = true;

		public void LoadSceneAdditive()
        {
			LoadSceneAdditive(inspectorSceneName);
        }

		public void LoadSceneSingle()
        {
			if (doTransition) LoadSceneSingle(inspectorSceneName);
			else LoadSceneSingleInstant(inspectorSceneName);
        }

		public void LoadSceneAdditive(string sceneName)
		{
			if (SceneManagementSingleton.instance_Initialised) SceneManagementSingleton.instance.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
		}

		public void LoadSceneSingleInstant(string sceneName)
		{
			if (SceneManagementSingleton.instance_Initialised) SceneManagementSingleton.instance.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
		}

		public void LoadSceneSingle(string sceneName)
		{
			if (SceneManagementSingleton.instance_Initialised) SceneManagementSingleton.instance.LoadSceneWithTransition(sceneName);
		}

		public void PlayNewGame()
		{
			if (SceneManagementSingleton.instance_Initialised) SceneManagementSingleton.instance.PlayNewGame();
		}

		public void PlayFromContinue()
		{
			if (SceneManagementSingleton.instance_Initialised) SceneManagementSingleton.instance.PlayFromContinue();
		}

		public void PlayFromMapNode(string saveName)
		{
			if (SceneManagementSingleton.instance_Initialised) SceneManagementSingleton.instance.PlayFromMapNode(saveName);
		}
	}
}
