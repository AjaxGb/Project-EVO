using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SceneInfo))]
public class SceneInfoEditor : Editor {

	SerializedProperty buildIndexProp;
	SerializedProperty sceneNameProp;
	SerializedProperty boundsProp;
	SerializedProperty adjacentScenesProp;

	private void OnEnable() {
		buildIndexProp = serializedObject.FindProperty("buildIndex");
		sceneNameProp = serializedObject.FindProperty("sceneName");
		boundsProp = serializedObject.FindProperty("bounds");
		adjacentScenesProp = serializedObject.FindProperty("adjacentScenes");
	}

	public override void OnInspectorGUI() {
		EditorGUILayout.LabelField(new GUIContent("Build Index"), new GUIContent(buildIndexProp.intValue.ToString()));
		EditorGUILayout.LabelField(new GUIContent("Scene Name"), new GUIContent(sceneNameProp.stringValue));
		EditorGUILayout.LabelField(new GUIContent("Bounds"), new GUIContent(boundsProp.rectValue.ToString()));
		EditorGUILayout.PrefixLabel("Adjacent Scenes (" + adjacentScenesProp.arraySize + "):");
		foreach (SerializedProperty p in adjacentScenesProp) {
			SceneInfo adj = (SceneInfo)p.objectReferenceValue;
			if (adj == null) {
				EditorGUILayout.LabelField("    NULL (WILL CAUSE ERRORS!)");
			} else {
				EditorGUILayout.LabelField("    " + adj.ToString());
			}
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
	SerializedProperty adjacentScenesProp;
	ReorderableList adjacentList;


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
		adjacentScenesProp = sceneInfoObject.FindProperty("adjacentScenes");

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

		adjacentList = new ReorderableList(sceneInfoObject, adjacentScenesProp, true, true, true, true);
		adjacentList.drawHeaderCallback = DrawAdjacentHeader;
		adjacentList.drawElementCallback = DrawAdjacentItem;
		adjacentList.onAddDropdownCallback = AddAdjacentItemMenu;

		serializedObject.ApplyModifiedPropertiesWithoutUndo();
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

		adjacentList.DoLayoutList();

		serializedObject.ApplyModifiedProperties();
		sceneInfoObject.ApplyModifiedProperties();
	}

	public static readonly Color boundsHandleColor   = new Color(0, 0.5f, 1, 1.0f);
	public static readonly Color boundsHandleColorBG = new Color(0, 0.5f, 1, 0.1f);
	public const float boundHandleSize = 0.05f;

	public static readonly Color respawnHandleColor   = new Color(1, 0, 0, 1.0f);
	public static readonly Color respawnHandleColorBG = new Color(1, 0, 0, 0.1f);
	public const float respawnHandleSize = 1f;

	public void OnSceneGUI() {

		// DRAW BOUNDS HANDLES
		Rect rect = sceneInfoInst.bounds;

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
			boundsProp.rectValue = rect;
			sceneInfoObject.ApplyModifiedProperties();
			this.Repaint();
		}

		// DRAW RESPAWN POINT HANDLE
		Vector2 respawn = targetRoot.respawnPoint;

		Handles.color = respawnHandleColorBG;
		Handles.DrawSolidDisc(respawn, Vector3.forward, respawnHandleSize);

		Handles.color = respawnHandleColor;
		EditorGUI.BeginChangeCheck();
		{
			respawn = Handles.Slider2D(respawn, Vector3.forward, Vector2.up, Vector2.left, respawnHandleSize, Handles.CircleHandleCap, 0);
		}
		if (EditorGUI.EndChangeCheck()) {
			serializedObject.Update();
			respawnPointProp.vector2Value = respawn;
			serializedObject.ApplyModifiedProperties();
			this.Repaint();
		}
	}
	
	public void DrawAdjacentHeader(Rect rect) {
		EditorGUI.LabelField(rect, "Adjacent Scenes");
	}

	public void DrawAdjacentItem(Rect rect, int index, bool isActive, bool isFocused) {
		SerializedProperty prop = adjacentScenesProp.GetArrayElementAtIndex(index);
		rect.y += 2;
		EditorGUI.ObjectField(
			new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), prop, typeof(SceneInfo));
		// TODO: Update the Adjacency list of the other scene, too.
	}

	public void AddAdjacentItemMenu(Rect rect, ReorderableList list) {
		GenericMenu menu = new GenericMenu();
		foreach (SceneInfo si in SceneInfo.allScenes) {
			if (si == sceneInfoInst) {
				menu.AddDisabledItem(new GUIContent(si.ToString()));
			} else if (si != null) {
				menu.AddItem(new GUIContent(si.ToString()), false, AddAdjacentItemClick, si);
			}
		}
		menu.ShowAsContext();
	}

	public void AddAdjacentItemClick(object obj) {
		sceneInfoObject.Update();
		SceneInfo other = (SceneInfo)obj;

		int index = adjacentList.index;
		if (adjacentList.count == 0) {
			index = 0;
		} else if (index < 0) {
			index += adjacentList.count;
		}

		adjacentScenesProp.InsertArrayElementAtIndex(index);
		adjacentScenesProp.GetArrayElementAtIndex(index).objectReferenceValue = other;
		// TODO: Update the Adjacency list of the other scene, too.
		sceneInfoObject.ApplyModifiedProperties();
	}
}
