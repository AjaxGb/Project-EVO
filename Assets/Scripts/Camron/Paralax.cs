using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour {

	CameraFollow camera;
	public float scale;
    Vector2 lastPos;

	// Use this for initialization
	void Start () {
        camera = SceneLoader.inst.cameraFollow;
        lastPos = camera.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = ((Vector2)camera.transform.position - lastPos) * scale;
	}



}
