using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeSceneryManager : MonoBehaviour {

	public interface IFadeable {
		Transform Transform { get; }
		float Alpha { get; set; }
		int SceneID { get; }
		int UniqueID { get; }
	}

	public class FadeableComparer : IEqualityComparer<IFadeable> {
		public bool Equals(IFadeable x, IFadeable y) {
			return x == y || (x != null && y != null && x.SceneID == y.SceneID && x.UniqueID == y.UniqueID); 
		}

		public int GetHashCode(IFadeable obj) {
			return obj == null ? 0 : obj.UniqueID;
		}
	}
	public static readonly FadeableComparer fadeableComparer = new FadeableComparer();

	public static FadeSceneryManager Inst { get; private set; }

	private Dictionary<int, Dictionary<int, IFadeable>> bySceneBI = new Dictionary<int, Dictionary<int, IFadeable>>();
	private HashSet<IFadeable> fadeOut = new HashSet<IFadeable>(fadeableComparer);
	private HashSet<IFadeable> fadeIn = new HashSet<IFadeable>(fadeableComparer);

	public float fadeOutTime = 5f; // In seconds
	public float fadeInTime = 5f;  // In seconds

	// Use this for initialization
	void Start() {
		Inst = this;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}
	
	// Update is called once per frame
	void Update() {
		if (fadeOut.Count > 0) {
			List<IFadeable> toDelete = new List<IFadeable>();
			foreach (IFadeable f in fadeOut) {
				f.Alpha -= 1 / fadeOutTime * Time.deltaTime;
				if (f.Alpha <= 0) {
					toDelete.Add(f);
				}
			}
			fadeOut.ExceptWith(toDelete);
			foreach (IFadeable f in toDelete) {
				Dictionary<int, IFadeable> fromThisScene = bySceneBI[f.SceneID];
				if (fromThisScene.Count <= 1) {
					bySceneBI.Remove(f.SceneID);
				} else {
					fromThisScene.Remove(f.UniqueID);
				}
				Destroy(f.Transform.gameObject);
			}
		}

		if (fadeIn.Count > 0) {
			List<IFadeable> toDelete = new List<IFadeable>();
			foreach (IFadeable f in fadeIn) {
				f.Alpha += 1 / fadeInTime * Time.deltaTime;
				if (f.Alpha >= 1) {
					toDelete.Add(f);
				}
			}
			fadeIn.ExceptWith(toDelete);
		}
	}

	public void CheckIn(IFadeable f) {
		Dictionary<int, IFadeable> fromThisScene;

		bool noneFromScene = !bySceneBI.TryGetValue(f.SceneID, out fromThisScene);
		if (noneFromScene) {
			// None from this scene were present; must add new Dict
			fromThisScene = new Dictionary<int, IFadeable>();
			bySceneBI.Add(f.SceneID, fromThisScene);
		}
		if (noneFromScene || !fromThisScene.ContainsKey(f.UniqueID)) {
			// Was not already present, so add to scene and fade in.
			fromThisScene.Add(f.UniqueID, f);
			f.Transform.parent = transform;
			f.Alpha = 0;
			fadeIn.Add(f);
		} else {
			if (fadeOut.Remove(f)) {
				// Was present but fading out, so fade the existing one back in.
				fadeIn.Add(fromThisScene[f.UniqueID]);
			}
			// A copy is already present, so get rid of the new one.
			Destroy(f.Transform.gameObject);
		}
	}

	public void OnSceneUnloaded(Scene s) {
		Dictionary<int, IFadeable> allFromScene;
		if (bySceneBI.TryGetValue(s.buildIndex, out allFromScene)) {
			fadeOut.UnionWith(allFromScene.Values);
			fadeIn.ExceptWith(allFromScene.Values);
		}
	}

	public void SkipFades() {

		foreach (IFadeable f in fadeOut) {
			Dictionary<int, IFadeable> fromThisScene = bySceneBI[f.SceneID];
			if (fromThisScene.Count <= 1) {
				bySceneBI.Remove(f.SceneID);
			} else {
				fromThisScene.Remove(f.UniqueID);
			}
			Destroy(f.Transform.gameObject);
		}
		fadeOut.Clear();

		foreach (IFadeable f in fadeIn) {
			f.Alpha = 1;
		}
		fadeIn.Clear();
	}
}
