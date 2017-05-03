using UnityEngine;

public class ThoughtSpawner : MonoBehaviour {

	public Thought[] text;

	public void OnSceneEntered(SceneInfo old, bool firstVisit) {
		if (firstVisit) {
			PlayThought();
		}
	}

	public void PlayThought() {
		foreach (Thought t in text) {
			ThoughtManager.Inst.AddThought(t);
		}
	}
}
