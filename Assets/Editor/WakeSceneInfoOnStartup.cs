using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class WakeSceneInfoOnStartup {

	public static readonly string[] scenePaths = { "Assets/Scenes" };

	static WakeSceneInfoOnStartup() {
		foreach (string guid in AssetDatabase.FindAssets("t:SceneInfo", scenePaths)) {
			AssetDatabase.LoadAssetAtPath<SceneInfo>(AssetDatabase.GUIDToAssetPath(guid));
		}
	}
	
}
