using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneInfo : ScriptableObject {
	public static SceneInfo[] allScenes = { };

	[HideInInspector]
	public string sceneName;
	[HideInInspector]
	public int buildIndex = -1;
	[HideInInspector]
	public Rect bounds = new Rect(-5, -5, 10, 10);
	[HideInInspector]
	public SceneInfo[] adjacentScenes;

	[NonSerialized]
	public Scene file;

	public void OnEnable() {
		if (buildIndex >= 0) {
			file = SceneManager.GetSceneByBuildIndex(buildIndex);

			if (allScenes.Length <= buildIndex) {
				Array.Resize(ref allScenes, buildIndex + 1);
			}
			if (allScenes[buildIndex] != null && allScenes[buildIndex] != this) {
				Debug.Log('"' + sceneName + "\" claims to have the same buildIndex (" + buildIndex + ") as \"" + allScenes[buildIndex].sceneName + '"');
			}
			allScenes[buildIndex] = this;
		}
	}

	public override string ToString() {
		return "[" + buildIndex + "] " + sceneName;
	}
}
