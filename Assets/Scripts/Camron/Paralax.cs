using System;
using UnityEngine;

public class Paralax : MonoBehaviour, FadeSceneryManager.IFadeable {

	private SpriteRenderer[] renderers;
	public Vector2 movementScale;

	new CameraFollow camera;
    Vector2 startPos;

	public int uniqueToSceneID = -1;
	public int UniqueID { get { return uniqueToSceneID; } }
	private int sceneID;
	public int SceneID { get { return sceneID; } }

	public Transform Transform { get { return transform; } }
	public float Alpha {
		get {
			return renderers[0].color.a;
		}
		set {
			foreach (SpriteRenderer r in renderers) {
				Color c = r.color;
				c.a = value;
				r.color = c;
			}
		}
	}

	// Use this for initialization
	void Start () {
        camera = SceneLoader.inst.cameraFollow;
        startPos = transform.position;
		renderers = GetComponentsInChildren<SpriteRenderer>();
		if (uniqueToSceneID == -1) Debug.LogWarning("Be sure to set the unique ID!");
		sceneID = gameObject.scene.buildIndex;
		FadeSceneryManager.Inst.CheckIn(this);
	}
	
	// Update is called once per frame
	void LateUpdate () {
        transform.position = new Vector2(
			startPos.x + (camera.transform.position.x - startPos.x) * movementScale.x,
			startPos.y + (camera.transform.position.y - startPos.y) * movementScale.y);
    }
}
