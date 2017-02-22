using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform target;
	public float smoothTime = 0.15f;
	public float maxSpeed = 1000f;

	Vector2 currentVelocity;
	
	// Update is called once per frame
	void Update () {
		Vector3 newPos = Vector2.SmoothDamp(transform.position, target.position, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);
		newPos.z = transform.position.z;
		transform.position = newPos;
	}
}
