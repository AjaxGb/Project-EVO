using UnityEngine;

public class SceneRoot : MonoBehaviour {
	public SceneInfo sceneInfo;
	public Vector2 respawnPoint;

	private void Start() {
		if (SceneLoader.inst == null) Debug.LogError("No SceneLoader in use! Scene loading will not work!");
	}
}
