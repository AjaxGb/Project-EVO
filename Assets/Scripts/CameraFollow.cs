using UnityEngine;

public class CameraFollow : MonoBehaviour {

	new public Camera camera;
	private float baseZoom;

	public Transform target;
	public float zoomTarget;
	public float smoothTime = 0.15f;
	public float maxSpeed = 1000f;
	public float zoomSmoothTime = 0.15f;
	public float zoomMaxSpeed = 1000f;

	private bool useFixedAngle;
	private Vector2 fixedTarget;
	private float fixedZoomTarget = 0f;

	public bool smooth = true;

	public float shakeAmount = 0f;
	public float shakeFalloff = 1f;

	Vector2 currentVelocity;
	float zoomVelocity;

	public static CameraFollow inst { get; private set; }
	void Start() {
		baseZoom = camera.orthographicSize;
		inst = this;
	}
	
	public void WarpToTarget() {
		if (useFixedAngle) {
			Vector3 newPos = fixedTarget;
			newPos.z = transform.position.z;
			transform.position = newPos;
			camera.orthographicSize = baseZoom + fixedZoomTarget;
		} else if (target != null) {
			Vector3 newPos = target.position;
			newPos.z = transform.position.z;
			transform.position = newPos;
			camera.orthographicSize = baseZoom;
		}
		currentVelocity = Vector2.zero;
		zoomVelocity = 0;
		shakeAmount = 0;
	}

	// Must be called every frame, or camera returns to normal.
	public void SetFixedAngle(Vector2 position, float zoom = 0) {
		fixedTarget = position;
		fixedZoomTarget = zoom;
		useFixedAngle = true;
	}

	// Update is called once per frame
	void LateUpdate() {
		if (target == null && !useFixedAngle) return;

		Vector3 newPos;
		float newZoom = baseZoom;
		if (useFixedAngle) {
			newPos = fixedTarget;
			newZoom += fixedZoomTarget;
			useFixedAngle = false;
		} else {
			newPos = target.position;
			newZoom += zoomTarget;
		}
		
		if (smooth) {
			newPos = Vector2.SmoothDamp(transform.position, newPos, ref currentVelocity, smoothTime, maxSpeed, Time.deltaTime);
			camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, newZoom, ref zoomVelocity, zoomSmoothTime, zoomMaxSpeed, Time.deltaTime);
		} else {
			camera.orthographicSize = newZoom;
		}
		newPos.z = transform.position.z;
		transform.position = newPos;

		if (shakeAmount > 0f) {
			camera.transform.localPosition = Random.insideUnitCircle * shakeAmount;
			shakeAmount -= Time.deltaTime * shakeFalloff;
			if (shakeAmount <= 0f) {
				shakeAmount = 0f;
				camera.transform.localPosition = Vector2.zero;
			}
		}
	}
}
