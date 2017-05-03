using UnityEngine;

public class ThoughtSpawner : MonoBehaviour {

	public string[] text;

	public void OnSceneEntered(SceneInfo old, bool firstVisit) {
		if (firstVisit) {
			foreach (string t in text) {
				ThoughtManager.Inst.AddThought(t);
			}
		}
	}
}
