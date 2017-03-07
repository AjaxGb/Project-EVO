using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
	public static SceneLoader inst;

	public Player player;
	public SceneInfo currScene;
	public bool warpToSpawn = true;

	private HashSet<SceneInfo> _activeScenes = new HashSet<SceneInfo>();

	// START
	private void Start() {
		inst = this;
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.LoadScene(currScene.buildIndex, LoadSceneMode.Additive);
		_activeScenes.Add(currScene);
		EnsureAdjacency();
	}

	// UPDATE
	public void Update() {
		if (!currScene.file.isLoaded) {
			player.gameObject.SetActive(false);
			if (!_activeScenes.Contains(currScene)) {
				SceneManager.LoadScene(currScene.buildIndex, LoadSceneMode.Additive);
				_activeScenes.Add(currScene);
			}
			return;
		} else {
			player.gameObject.SetActive(true);
		}
		if (warpToSpawn) {
			SceneRoot root = currScene.file.GetRootGameObjects()[0].GetComponent<SceneRoot>();
			if (root) {
				player.transform.position = root.respawnPoint;
			} else {
				Debug.LogError("Scene does not contain a SceneRoot!");
			}
			warpToSpawn = false;
			return;
		}
		currScene = GetCurrScene();
		EnsureAdjacency();
	}

	// DESTROY
	private void OnDestroy() {
		inst = null;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex < 0 || scene.buildIndex >= SceneInfo.allScenes.Length) {
			return;
		}
		SceneInfo si = SceneInfo.allScenes[scene.buildIndex];
		if (si != null) {
			si.file = scene;
		}
	}

	private SceneInfo GetCurrScene() {
		SceneInfo curr = currScene;
		while (!curr.bounds.Contains(player.transform.position)) {
			bool currChanged = false;
			foreach (SceneInfo adj in curr.adjacentScenes) {
				if (adj.bounds.Contains(player.transform.position)) {
					curr = adj;
					currChanged = true;
					break;
				}
			}
			if (!currChanged) break;
		}
		return curr;
	}

	private void EnsureAdjacency() {

		HashSet<SceneInfo> toLoad = new HashSet<SceneInfo>(currScene.adjacentScenes);

		_activeScenes.RemoveWhere(si => {
			if (si != currScene && !toLoad.Remove(si)) {
				// Not the current scene, not adjacent to
				// the current scene. Unload it.
				SceneManager.UnloadSceneAsync(si.buildIndex);
				return true;
			} else {
				return false;
			}
		});

		foreach (SceneInfo si in toLoad) {
			SceneManager.LoadSceneAsync(si.buildIndex, LoadSceneMode.Additive);
			_activeScenes.Add(si);
		}
	}
}
