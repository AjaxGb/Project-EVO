using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
	public static SceneLoader inst;
	public const int buildIndex = 1;

	public static SaveState loadSaveState;

	public Player player;
	public CameraFollow cameraFollow;
	public SceneInfo currScene;

	private bool currSceneLoaded = false;
	private HashSet<SceneInfo> _activeScenes = new HashSet<SceneInfo>();
	private Dictionary<SceneInfo, Vector2> _worldPositions = new Dictionary<SceneInfo, Vector2>();
	private HashSet<SceneInfo> _justLoaded   = new HashSet<SceneInfo>();

	// START
	private void Start() {
		inst = this;
		SceneManager.sceneLoaded += OnSceneLoaded;

		// This does nothing, but it ensures that the SaveManager
		// (and by extension, all SceneInfo instances) is loaded.
#pragma warning disable 0219 // "Unused"
		SaveManager noop = SaveManager.inst;
#pragma warning restore 0219

		if (loadSaveState != null) {
			currScene = SceneInfo.scenesByBI[loadSaveState.currSceneBI];
		}

		int maxSkill = Mathf.Min(BossBase.learnSkills.Length, BossBase.highestKilled);
		for (int i = 0; i < maxSkill; i++) {
			BossBase.learnSkills[i]();
		}

		if (currScene != null) {
			CalculateAllScenePositions(_worldPositions, currScene);
			SceneManager.LoadScene(currScene.buildIndex, LoadSceneMode.Additive);
			_activeScenes.Add(currScene);
			EnsureAdjacency();

			if (SaveManager.inst.currentSave == null) {
				SaveManager.inst.currentSave = new SaveState("!EDITOR! Test play", currScene.buildIndex, Vector2.zero, 0);
			} 
		}
	}

	// UPDATE
	public void Update() {
		if (_justLoaded.Count != 0) {
			foreach (SceneInfo si in _justLoaded) {
				if (si.file.rootCount < 1 || (si.root = si.file.GetRootGameObjects()[0].GetComponent<SceneRoot>()) == null) {
					Debug.LogErrorFormat(si, "Scene \"{0}\" does not contain a SceneRoot!", si.name);
				}
				Vector2 loadPos;
				if (_worldPositions.TryGetValue(si, out loadPos)) {
					si.root.transform.position = loadPos;
				} else {
					Debug.LogErrorFormat(si, "\"{0}\" did not have a loadPos when loaded!", si);
				}
			}
			_justLoaded.Clear();
		}

		if (currScene == null) return;
		currScene = GetCurrScene();

		if (!currSceneLoaded && currScene.file.isLoaded) {
			// Was in unloaded scene, now loaded
			SceneManager.SetActiveScene(currScene.file);
			player.gameObject.SetActive(true);
			currSceneLoaded = true;
		} else if (currSceneLoaded && !currScene.file.isLoaded) {
			// Should be in loaded scene, currently not loaded
			player.gameObject.SetActive(false);
			currSceneLoaded = false;
			if (!_activeScenes.Contains(currScene)) {
				SceneManager.LoadScene(currScene.buildIndex, LoadSceneMode.Additive);
				_activeScenes.Add(currScene);
			}
			return;
		}

		if (loadSaveState != null && currScene.root) {
			player.transform.position = (Vector2)currScene.root.transform.position + loadSaveState.posInScene;
			cameraFollow.WarpToTarget();
			loadSaveState = null;
		}
		EnsureAdjacency();
	}

	// DESTROY
	private void OnDestroy() {
		inst = null;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		SceneInfo si;
		if (SceneInfo.scenesByBI.TryGetValue(scene.buildIndex, out si)) {
			_justLoaded.Add(si);
			si.file = scene;
		}
	}

	private SceneInfo GetCurrScene() {
		SceneInfo curr = currScene;
		if (!(player.IsAlive && curr.root)) return curr;
		Vector2 currPos = curr.root.transform.position;

		while (!currPos.TransformRect(curr.bounds).Contains(player.transform.position)) {
			bool currChanged = false;
			foreach (SceneAdjacency ourAdjacency in curr.adjacentScenes) {
				SceneInfo adjacentScene = SceneInfo.scenesByBI[ourAdjacency.toBI];
				Vector2 adjPos = _worldPositions[adjacentScene];

				Rect worldBounds = adjPos.TransformRect(adjacentScene.bounds);
				if (worldBounds.Contains(player.transform.position)) {
					curr = adjacentScene;
					currPos = adjPos;
					currChanged = true;
					break;
				}
			}
			if (!currChanged) break;
		}
		return curr;
	}

	private void EnsureAdjacency() {
		// Load scenes that are needed, unload scenes that aren't.

		HashSet<SceneInfo> toLoad = new HashSet<SceneInfo>(from adj in currScene.adjacentScenes
		                                                   select SceneInfo.scenesByBI[adj.toBI]);
		
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
		
		foreach (SceneInfo scene in toLoad) {
			SceneManager.LoadSceneAsync(scene.buildIndex, LoadSceneMode.Additive);
			_activeScenes.Add(scene);
		}
	}

	public static void CalculateAllScenePositions(Dictionary<SceneInfo, Vector2> worldPositions, SceneInfo currScene, Vector2 currScenePos = default(Vector2)) {
		worldPositions.Clear();

		SceneInfo curr = currScene;
		Vector2 currPos = currScenePos;
		worldPositions.Add(curr, currPos);

		foreach (SceneAdjacency adj in curr.adjacentScenes) {
			_CalcScenePos(worldPositions, curr, currPos, adj);
		}

		if (worldPositions.Count < SceneInfo.scenesByBI.Count) {
			Debug.LogWarningFormat(currScene,
				"Some scenes (\"{0}\") are not connected to the starting scene \"{1}\" by adjacency.",
				string.Join("\" and \"",
					(from s in SceneInfo.scenesByBI.Values
					 where !worldPositions.ContainsKey(s)
					 select s.name).ToArray()),
				currScene.name);
		}
	}

	private static void _CalcScenePos(Dictionary<SceneInfo, Vector2> worldPositions, SceneInfo currScene, Vector2 currScenePos, SceneAdjacency ourAdjacency) {
		SceneInfo adjacentScene = SceneInfo.scenesByBI[ourAdjacency.toBI];

		Vector2 adjPos = Vector2.zero;
		bool posFound = false;
		foreach (SceneAdjacency theirAdjacency in adjacentScene.adjacentScenes) {
			if (theirAdjacency.toBI == currScene.buildIndex) {
				adjPos = currScenePos + ourAdjacency.connectionPoint - theirAdjacency.connectionPoint;
				posFound = true;
				Vector2 oldPos;
				if (worldPositions.TryGetValue(adjacentScene, out oldPos) && Vector2.SqrMagnitude(oldPos - adjPos) > 0.000001f) {
					Debug.LogErrorFormat(adjacentScene, "Adjacency map seems impossible; \"{0}\" has conflicting claims on its world position.", adjacentScene.name);
				} else {
					worldPositions[adjacentScene] = adjPos;
				}
				break;
			}
		}
		if (!posFound) {
			Debug.LogErrorFormat(currScene, "SceneInfo \"{0}\" claims adjacency with \"{1}\", which does not have a matching entry!", currScene.name, adjacentScene.name);
			return;
		}

		foreach (SceneAdjacency theirAdjacency in adjacentScene.adjacentScenes) {
			if (theirAdjacency.toBI == currScene.buildIndex || worldPositions.ContainsKey(SceneInfo.scenesByBI[theirAdjacency.toBI])) continue;
			_CalcScenePos(worldPositions, adjacentScene, adjPos, theirAdjacency);
		}
	}

	public Vector2 GetSceneWorldPosition(SceneInfo scene) {
		Vector2 pos;
		if (_worldPositions.TryGetValue(scene, out pos)) {
			return pos;
		}
		return Vector2.zero;
	}

	public static bool IsInCurrentScene(GameObject go) {
		return inst == null || inst.currScene.buildIndex == go.scene.buildIndex;
	}
}
