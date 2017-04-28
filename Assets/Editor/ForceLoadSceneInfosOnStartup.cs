using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ForceLoadSceneInfosOnStartup {

	static ForceLoadSceneInfosOnStartup() {
		Resources.LoadAll<SceneInfo>("");
	}
}
