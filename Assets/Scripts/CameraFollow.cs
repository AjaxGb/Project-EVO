using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform target;
	public float smoothTime = 0.15f;
	public float maxSpeed = 1000f;

	public bool smooth = true;

	Vector2 currentVelocity;
	
	public void WarpToTarget() {
		Vector3 newPos = target.position;
		newPos.z = transform.position.z;
		transform.position = newPos;
		currentVelocity = Vector2.zero;
	}

	// Update is called once per frame
	void Update() {
		Vector3 newPos = smooth ?
			(Vector3) Vector2.SmoothDamp(transform.position, target.position, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime)
			: target.position;
		newPos.z = transform.position.z;
		transform.position = newPos;
	}
}
