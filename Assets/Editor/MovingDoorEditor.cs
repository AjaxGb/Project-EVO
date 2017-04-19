using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovingDoor))]
public class MovingDoorEditor : Editor {

	new MovingDoor target;
	Rect bounds;

	SerializedProperty openPosProp;
	SerializedProperty closedPosProp;

	void OnEnable() {
		target = (MovingDoor)base.target;

		Renderer r = target.gameObject.GetComponent<Renderer>();
		if (r != null) {
			bounds = target.transform.position.InverseTransformRect(r.bounds.ToRect());
		}

		openPosProp = serializedObject.FindProperty("openPos");
		closedPosProp = serializedObject.FindProperty("closedPos");
	}

	public static readonly Color openFillColor = new Color(0, 1, 1, 0.1f);
	public static readonly Color closedFillColor = new Color(1, 0, 0, 0.1f);
	public static readonly Color outlineColor = new Color(0, 0, 0, 0.5f);

	public void OnSceneGUI() {
		serializedObject.Update();

		Vector2 openPos = target.transform.parent.TransformPoint(openPosProp.vector2Value);
		Vector2 closedPos = target.transform.parent.TransformPoint(closedPosProp.vector2Value);

		Handles.DrawSolidRectangleWithOutline(openPos.TransformRect(bounds), openFillColor, outlineColor);
		Handles.DrawSolidRectangleWithOutline(closedPos.TransformRect(bounds), closedFillColor, outlineColor);

		EditorGUI.BeginChangeCheck();
		{
			openPos = Handles.PositionHandle(openPos, target.transform.rotation);
			closedPos = Handles.PositionHandle(closedPos, target.transform.rotation);
		}
		if (EditorGUI.EndChangeCheck()) {
			openPosProp.vector2Value = target.transform.parent.InverseTransformPoint(openPos);
			closedPosProp.vector2Value = target.transform.parent.InverseTransformPoint(closedPos);
			serializedObject.ApplyModifiedProperties();
			this.Repaint();
		}
		
	}

}