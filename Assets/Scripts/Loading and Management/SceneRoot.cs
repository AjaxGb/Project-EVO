using UnityEngine;

public class SceneRoot : MonoBehaviour {
	public SceneInfo sceneInfo;
	public Vector2 respawnPoint;

	public Vector2 WorldSpaceRespawnPoint { get { return (Vector2)transform.position + respawnPoint; } }

	private void Start() {
		if (SceneLoader.inst == null) {
			SaveManager.inst.LoadState(
				new SaveState("!EDITOR Quickstart!", gameObject.scene.buildIndex, respawnPoint, 0));
		} else {
			// If this does not happen in Start, colliders will collide with incorrect positions.
			this.transform.position = SceneLoader.inst.GetSceneWorldPosition(sceneInfo);
		}
	}
}
