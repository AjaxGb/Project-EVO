using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SceneInfo))]
public class SceneInfoEditor : Editor {

	new SceneInfo target;

	private void OnEnable() {
		target = (SceneInfo)base.target;
	}

	public override void OnInspectorGUI() {
		EditorGUILayout.LabelField(new GUIContent("Build Index"), new GUIContent(target.buildIndex.ToString()));
		EditorGUILayout.LabelField(new GUIContent("Scene Name"), new GUIContent(target.sceneName));
		EditorGUILayout.LabelField(new GUIContent("Bounds"), new GUIContent(target.bounds.ToString()));
		EditorGUILayout.PrefixLabel("Adjacent Scenes (" + target.adjacentScenes.Count + "):");
		foreach (SceneAdjacency adj in target.adjacentScenes) {
			EditorGUILayout.LabelField("    " + adj.ToString());
		}
	}
}

[CustomEditor(typeof(SceneRoot))]
public class SceneRootEditor : Editor {

	SceneRoot targetRoot;

	SerializedProperty sceneInfoProp;
	SerializedProperty respawnPointProp;

	SerializedObject sceneInfoObject;
	SceneInfo sceneInfoInst;
	SerializedProperty sceneNameProp;
	SerializedProperty buildIndexProp;
	SerializedProperty boundsProp;
	List<SceneAdjacency> adjacentScenesList;

	void OnEnable() {
		targetRoot = (SceneRoot)target;
		Scene scene = targetRoot.gameObject.scene;

		sceneInfoProp = serializedObject.FindProperty("sceneInfo");
		respawnPointProp = serializedObject.FindProperty("respawnPoint");

		serializedObject.Update();
		if (sceneInfoProp.objectReferenceValue) {
			sceneInfoInst = (SceneInfo)sceneInfoProp.objectReferenceValue;
		} else {
			// Make new SceneInfo, save it, and update the reference
			string assetPath = AssetDatabase.GenerateUniqueAssetPath(
				scene.path.Remove(scene.path.Length - Path.GetExtension(scene.path).Length) + ".asset");

			sceneInfoInst = CreateInstance<SceneInfo>();
			AssetDatabase.CreateAsset(sceneInfoInst, assetPath);
			AssetDatabase.SaveAssets();
			sceneInfoProp.objectReferenceValue = sceneInfoInst;
		}
		sceneInfoObject = new SerializedObject(sceneInfoProp.objectReferenceValue);

		sceneNameProp = sceneInfoObject.FindProperty("sceneName");
		buildIndexProp = sceneInfoObject.FindProperty("buildIndex");
		boundsProp = sceneInfoObject.FindProperty("bounds");
		adjacentScenesList = sceneInfoInst.adjacentScenes;

		sceneInfoObject.Update();
		if (scene.buildIndex < 0) {
			var scenes = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length + 1];
			EditorBuildSettings.scenes.CopyTo(scenes, 0);
			scenes[scenes.Length - 1] = new EditorBuildSettingsScene(scene.path, true);
			EditorBuildSettings.scenes = scenes;
		}
		buildIndexProp.intValue = scene.buildIndex;
		sceneInfoObject.ApplyModifiedPropertiesWithoutUndo();
		sceneInfoInst.OnEnable();
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
		
		connectionLabelStyle.normal.textColor = connectionLableColor;
		connectionLabelStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/bar1 fill.png");
		connectionLabelStyle.border = new RectOffset(5, 5, 5, 5);
		connectionLabelStyle.alignment = TextAnchor.MiddleCenter;
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		sceneInfoObject.Update();

		EditorGUILayout.LabelField(new GUIContent("Build Index"), new GUIContent(buildIndexProp.intValue.ToString()));
		EditorGUILayout.PropertyField(sceneNameProp);
		EditorGUILayout.PropertyField(respawnPointProp);
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.PrefixLabel("Bounds");
			boundsProp.rectValue = EditorGUILayout.RectField(boundsProp.rectValue);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField("Adjacent Scenes");
		SceneAdjacency testAdj = new SceneAdjacency(0, Vector2.zero);
		bool needSort = false;
		for (int i = 0; i < adjacentScenesList.Count; ++i) {
			SceneInfo newInfo, oldInfo;
			Vector2 point;
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("-")) {
					adjacentScenesList.RemoveAt(i);
					if (i >= adjacentScenesList.Count) break;
				}
				SceneAdjacency adj = adjacentScenesList[i];
				oldInfo = SceneInfo.scenesByBI[adj.toBI];
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginVertical();
				{
					newInfo = EditorGUILayout.ObjectField("Scene Info", oldInfo, typeof(SceneInfo), false) as SceneInfo;
					point = EditorGUILayout.Vector2Field("Connection Point", adj.connectionPoint);
				}
				EditorGUILayout.EndVertical();
				testAdj.toBI = newInfo.buildIndex;
				if (EditorGUI.EndChangeCheck() && newInfo != sceneInfoInst && (newInfo == oldInfo || !adjacentScenesList.Contains(testAdj))) {
					adj.toBI = newInfo.buildIndex;
					adj.connectionPoint = point;
					needSort = true;
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}
		if (needSort) {
			adjacentScenesList.Sort();
			EditorUtility.SetDirty(sceneInfoInst);
		}
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		if (GUILayout.Button("+")) {
			GenericMenu menu = new GenericMenu();
			foreach (SceneInfo si in SceneInfo.allScenes) {
				testAdj.toBI = si.buildIndex;
				if (si == sceneInfoInst || adjacentScenesList.ContainsSorted(testAdj)) {
					menu.AddDisabledItem(new GUIContent(si.ToString()));
				} else {
					menu.AddItem(new GUIContent(si.ToString()), false, AddAdjacentItemClick, si);
				}
			}
			menu.ShowAsContext();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();

		serializedObject.ApplyModifiedProperties();
		sceneInfoObject.ApplyModifiedProperties();

		if (GUILayout.Button("Arrange all loaded scenes correctly")) {
			Dictionary<SceneInfo, Vector2> worldPositions = new Dictionary<SceneInfo, Vector2>();
			SceneLoader.CalculateAllScenePositions(worldPositions, sceneInfoInst, targetRoot.transform.position);
			for (int i = 0; i < SceneManager.sceneCount; ++i) {
				Scene s = SceneManager.GetSceneAt(i);
				SceneInfo si;
				if (SceneInfo.scenesByBI.TryGetValue(s.buildIndex, out si)) {
					s.GetRootGameObjects()[0].transform.position = worldPositions[si];
				}
			}
		}
	}

	public static readonly Color boundsHandleColor   = new Color(0, 0.5f, 1, 1.0f);
	public static readonly Color boundsHandleColorBG = new Color(0, 0.5f, 1, 0.1f);
	public const float boundHandleSize = 0.05f;

	public static readonly Color respawnHandleColor   = new Color(1, 0, 0, 1.0f);
	public static readonly Color respawnHandleColorBG = new Color(1, 0, 0, 0.1f);
	public const float respawnHandleSize = 1f;

	public static readonly Color connectionHandleColor  = new Color(1, 1, 0, 1.0f);
	public static readonly Color connectionLableColor   = new Color(1, 1, 1, 0.8f);
	public static readonly Color connectionLableColorBG = new Color(0, 0, 0, 0.2f);
	public static readonly GUIStyle connectionLabelStyle = new GUIStyle();
	public const float connectionHandleSize = 1f;

	public void OnSceneGUI() {

		// DRAW BOUNDS HANDLES
		Rect rect = targetRoot.transform.position.TransformRect(sceneInfoInst.bounds);

		Vector2 xMin = new Vector2(rect.xMin, rect.center.y);
		Vector2 xMax = new Vector2(rect.xMax, rect.center.y);
		Vector2 yMin = new Vector2(rect.center.x, rect.yMin);
		Vector2 yMax = new Vector2(rect.center.x, rect.yMax);

		Handles.DrawSolidRectangleWithOutline(rect, boundsHandleColorBG, boundsHandleColor);

		Handles.color = boundsHandleColor;
		EditorGUI.BeginChangeCheck();
		{
			xMin = Handles.Slider(xMin, Vector2.left, boundHandleSize * HandleUtility.GetHandleSize(xMin), Handles.DotCap, 0);
			xMax = Handles.Slider(xMax, Vector2.left, boundHandleSize * HandleUtility.GetHandleSize(xMax), Handles.DotCap, 0);
			yMin = Handles.Slider(yMin, Vector2.up,   boundHandleSize * HandleUtility.GetHandleSize(yMin), Handles.DotCap, 0);
			yMax = Handles.Slider(yMax, Vector2.up,   boundHandleSize * HandleUtility.GetHandleSize(yMax), Handles.DotCap, 0);
		}
		if (EditorGUI.EndChangeCheck()) {
			sceneInfoObject.Update();
			rect.Set(xMin.x, yMin.y, xMax.x - xMin.x, yMax.y - yMin.y);
			boundsProp.rectValue = targetRoot.transform.position.InverseTransformRect(rect);
			sceneInfoObject.ApplyModifiedProperties();
			this.Repaint();
		}

		// DRAW RESPAWN POINT HANDLE
		Vector2 respawn = targetRoot.transform.TransformPoint(targetRoot.respawnPoint);

		Handles.color = respawnHandleColorBG;
		Handles.DrawSolidDisc(respawn, Vector3.forward, respawnHandleSize);

		Handles.color = respawnHandleColor;
		EditorGUI.BeginChangeCheck();
		{
			respawn = Handles.Slider2D(respawn, Vector3.forward, Vector2.up, Vector2.left, respawnHandleSize, Handles.CircleHandleCap, 0);
		}
		if (EditorGUI.EndChangeCheck()) {
			serializedObject.Update();
			respawnPointProp.vector2Value = targetRoot.transform.InverseTransformPoint(respawn);
			serializedObject.ApplyModifiedProperties();
			this.Repaint();
		}

		// DRAW CONNECTION POINT HANDLES

		Handles.color = connectionHandleColor;

		GUI.backgroundColor = connectionLableColorBG;
		foreach (SceneAdjacency adj in adjacentScenesList) {
			Vector2 point = adj.connectionPoint + (Vector2)targetRoot.transform.position;
			EditorGUI.BeginChangeCheck();
			{
				point = Handles.Slider2D(point, Vector3.forward, Vector2.up, Vector2.left, connectionHandleSize, Handles.DotHandleCap, 0);
			}
			if (EditorGUI.EndChangeCheck()) {
				adj.connectionPoint = point - (Vector2)targetRoot.transform.position;
				EditorUtility.SetDirty(sceneInfoInst);
				this.Repaint();
			}
			SceneInfo si;
			if (SceneInfo.scenesByBI.TryGetValue(adj.toBI, out si)) {
				Handles.Label(point + Vector2.up * 2, si.name, connectionLabelStyle);
			}
		}
	}

	private void AddAdjacentItemClick(object obj) {
		Undo.RecordObject(sceneInfoInst, "Add adjacent item");
		SceneInfo other = (SceneInfo)obj;

		adjacentScenesList.AddSorted(new SceneAdjacency(other.buildIndex, Vector2.zero));
		EditorUtility.SetDirty(sceneInfoInst);
		// TODO: Update the Adjacency list of the other scene, too.
	}
}
