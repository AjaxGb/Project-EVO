using UnityEngine;
using UnityEngine.UI;

public class SavedGameMenuItem : MonoBehaviour {

	private SaveState _representedState;
	public SaveState RepresentedState {
		set {
			_representedState = value;
			saveName.text = value.name;
			timeStamp.text = value.timestamp;
			currSceneName.text = SceneInfo.scenesByBI[value.currSceneBI].sceneName;
		}
	}
	public Text saveName;
	public Text timeStamp;
	public Text currSceneName;

	public void OnClicked() {
		SaveManager.inst.LoadState(_representedState);
	}

	public void DeletThis() {
		SaveManager.inst.DeleteSave(_representedState);
		Destroy(gameObject);
	}
}
