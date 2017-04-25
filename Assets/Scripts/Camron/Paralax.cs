using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour {

	new CameraFollow camera;
    public bool useXY = false;
	public float scale;
    public float scaleX;
    public float scaleY;
    Vector2 startPos;

	// Use this for initialization
	void Start () {
        camera = SceneLoader.inst.cameraFollow;
        startPos = camera.transform.position - transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () { 
        if (!useXY)
            transform.position = ((Vector2)camera.transform.position - startPos) * scale;
        else
            transform.position = new Vector2((camera.transform.position.x - startPos.x) * scaleX, (camera.transform.position.y - startPos.y) * scaleY);
    }



}
