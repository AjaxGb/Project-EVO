using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRoot : MonoBehaviour {
	public SceneInfo sceneInfo;
	public Vector2 respawnPoint;

	private void Start() {
		if (SceneLoader.inst == null) {
			SceneLoader.overrideStartScene = sceneInfo;
			SceneManager.LoadScene(SceneLoader.buildIndex);
		}
	}
}
