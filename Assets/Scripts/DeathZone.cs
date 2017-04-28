using UnityEngine;

public class DeathZone : MonoBehaviour {

	public bool dieOnFallOnly;
	public float minFallSpeed = -2f;

	void OnTriggerEnter2D(Collider2D coll) {
		if (dieOnFallOnly && coll.attachedRigidbody && coll.attachedRigidbody.velocity.y > minFallSpeed) return;
		IKillable k = coll.GetComponent<IKillable>();
		if (k != null) {
			k.Kill();
		}
	}
}
