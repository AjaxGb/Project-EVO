using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[Serializable]
public class SaveState {
	public string name;
	public string timestamp;
	public int currSceneBI;
	public Vector2 posInScene;
	public int lastBossBeaten;

	public static string CurrentDateTimeString {
		get { return DateTime.Now.ToString("MMMM dd, yyyy\nhh:mm:ss tt"); }
	}

	public SaveState(string name) {
		this.name = name;
		timestamp = CurrentDateTimeString;
		currSceneBI = SceneLoader.inst.currScene.buildIndex;
		posInScene =
			SceneLoader.inst.player.transform.position - SceneLoader.inst.currScene.root.transform.position;
		lastBossBeaten = BossBase.highestKilled;
	}

	public SaveState(string name, int scene, Vector2 pos, int boss) {
		this.name = name;
		timestamp = CurrentDateTimeString;
		currSceneBI = scene;
		posInScene = pos;
		lastBossBeaten = boss;
	}
}

[Serializable]
public sealed class SaveManager : IEnumerable<SaveState> {

	[SerializeField]
	private List<SaveState> saveStates;
	[NonSerialized]
	public SaveState currentSave;

	public static readonly string savePath = Path.Combine(Application.persistentDataPath, "saves.json");
	public static readonly SaveManager inst = new SaveManager();
	private SaveManager() {
		
		Debug.Log("Save Path: " + savePath);

		try {
			JsonUtility.FromJsonOverwrite(File.ReadAllText(savePath), this);
		} catch {
			saveStates = new List<SaveState>();
			SaveToFile();
		}
	}

	public void SaveToFile() {
		File.WriteAllText(savePath, JsonUtility.ToJson(this, true));
	}

	public void AddSave(SaveState state) {
		saveStates.Insert(0, state);
		SaveToFile();
	}

	public void UpdateSave(Vector2 savePos) {
		currentSave.timestamp = SaveState.CurrentDateTimeString;
		currentSave.currSceneBI = SceneLoader.inst.currScene.buildIndex;
		currentSave.posInScene = savePos - (Vector2)SceneLoader.inst.currScene.root.transform.position;
		currentSave.lastBossBeaten = BossBase.highestKilled;
		// Move to front
		saveStates.Remove(currentSave);
		saveStates.Insert(0, currentSave);
		SaveToFile();
	}

	public void DeleteSave(SaveState state) {
		saveStates.Remove(state);
		currentSave = null;
		SaveToFile();
	}

	public void LoadState(SaveState state) {
		if (state == null) throw new ArgumentNullException("state");
		BossBase.highestKilled = state.lastBossBeaten;
		currentSave = state;
		SceneLoader.loadSaveState = state;
		SceneManager.LoadScene(SceneLoader.buildIndex);
	}

	public void LoadCurrentSave() {
		LoadState(currentSave);
	}

	public IEnumerator<SaveState> GetEnumerator() {
		return saveStates.GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator() {
		return saveStates.GetEnumerator();
	}
}
