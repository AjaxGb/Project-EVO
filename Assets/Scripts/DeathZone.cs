using UnityEngine;

public class DeathZone : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D coll) {
		IKillable k = coll.GetComponent<IKillable>();
		if (k != null) {
			k.Kill();
		}
	}
}
