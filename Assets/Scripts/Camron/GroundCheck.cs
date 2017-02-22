using UnityEngine;

public class GroundCheck : MonoBehaviour {

    int collisions = 0;

	public bool InAir { get { return collisions == 0; } }

	// Use Edit > Project Settings > Physics 2D to control
	// which layers collide with GroundCheck.

    void OnTriggerEnter2D(Collider2D coll) {
		collisions++;
	}

    void OnTriggerExit2D(Collider2D coll) {
        collisions--;
    }
}
