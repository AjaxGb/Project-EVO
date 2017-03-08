using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform target;
	public float smoothTime = 0.15f;
	public float maxSpeed = 1000f;

	public bool smooth = true;

	Vector2 currentVelocity;
	
	public void WarpToTarget() {
		if (target != null) {
			Vector3 newPos = target.position;
			newPos.z = transform.position.z;
			transform.position = newPos;
		}
		currentVelocity = Vector2.zero;
	}

	// Update is called once per frame
	void Update() {
		Vector3 newPos;
		if (target == null) {
			newPos = transform.position;
		} else if (smooth) {
			newPos = Vector2.SmoothDamp(transform.position, target.position, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);
		} else {
			newPos = target.position;
		}
		newPos.z = transform.position.z;
		transform.position = newPos;
	}
}
