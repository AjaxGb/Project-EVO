using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SceneAdjacency : IComparable<SceneAdjacency>, IEquatable<SceneAdjacency> {
	public int toBI;
	public Vector2 connectionPoint;

	public SceneAdjacency(int bi, Vector2 cp) {
		toBI = bi;
		connectionPoint = cp;
	}

	public override string ToString() {
		SceneInfo si;
		if (SceneInfo.scenesByBI.TryGetValue(toBI, out si)) {
			return si + " : " + connectionPoint;
		} else {
			return "[" + toBI + "] !!NULL!! : " + connectionPoint;
		}
	}

	public int CompareTo(SceneAdjacency other) {
		return this.toBI - other.toBI;
	}

	public bool Equals(SceneAdjacency other) {
		return this.toBI == other.toBI;
	}
}

public class SceneInfo : ScriptableObject, IComparable<SceneInfo> {
	public static readonly Dictionary<int, SceneInfo> scenesByBI = new Dictionary<int, SceneInfo>();
	public static readonly List<SceneInfo> allScenes = new List<SceneInfo>();

	[HideInInspector]
	public string sceneName;
	[HideInInspector]
	public int buildIndex = -1;
	[HideInInspector]
	public Rect bounds = new Rect(-5, -5, 10, 10);
	[HideInInspector]
	public List<SceneAdjacency> adjacentScenes = new List<SceneAdjacency>();

	[NonSerialized]
	public Scene file;
	[NonSerialized]
	public SceneRoot root;

	public void OnEnable() {
		if (buildIndex >= 0) {
			file = SceneManager.GetSceneByBuildIndex(buildIndex);

			SceneInfo val;
			if (scenesByBI.TryGetValue(buildIndex, out val) && val != this) {
				Debug.LogWarningFormat(this, "\"{0}\" claims to have the same buildIndex ({1}) as \"{2}\"", name, buildIndex, val.name);
			} else {
				scenesByBI[buildIndex] = this;
				allScenes.AddSorted(this);
			}
		} else {
			Debug.LogWarningFormat(this, "SceneInfo \"{0}\" is not initialized", name);
		}
	}

	public void OnDisable() {
		if (buildIndex >= 0) {
			SceneInfo val;
			if (!scenesByBI.TryGetValue(buildIndex, out val)) {
				Debug.LogWarningFormat(this, "\"{0}\" was not enabled properly", name);
			} else if (val != this) {
				Debug.LogWarningFormat(this, "\"{0}\" claims to have the same buildIndex ({1}) as \"{2}\"", name, buildIndex, val.name);
			} else {
				scenesByBI.Remove(buildIndex);
				allScenes.RemoveSorted(this);
			}
		} else {
			Debug.LogWarningFormat(this, "SceneInfo \"{0}\" is not initialized", name);
		}
	}

	public override string ToString() {
		return "[" + buildIndex + "] " + sceneName;
	}

	public int CompareTo(SceneInfo other) {
		return this.buildIndex - other.buildIndex;
	}
}
