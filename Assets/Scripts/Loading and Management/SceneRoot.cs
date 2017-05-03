using System;
using UnityEngine;
using UnityEngine.Events;

public class SceneRoot : MonoBehaviour {
	public SceneInfo sceneInfo;
	public Vector2 respawnPoint;

	public Vector2 WorldSpaceRespawnPoint { get { return (Vector2)transform.position + respawnPoint; } }

	// SceneInfo - from? Bool - First time entering?
	[Serializable]
	public class SceneEnteredEvent : UnityEvent<SceneInfo, bool> { }
	public SceneEnteredEvent playerEntered = new SceneEnteredEvent();
	// SceneInfo - to?
	[Serializable]
	public class SceneLeftEvent : UnityEvent<SceneInfo> { }
	public SceneLeftEvent playerLeft = new SceneLeftEvent();

	private void Start() {
		if (SceneLoader.inst == null) {
			SaveManager.inst.LoadState(
				new SaveState("!EDITOR! Quickstart", gameObject.scene.buildIndex, respawnPoint, 0));
		} else {
			// If this does not happen in Start, colliders will collide with incorrect positions.
			this.transform.position = SceneLoader.inst.GetSceneWorldPosition(sceneInfo);
		}
	}
}
