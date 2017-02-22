using UnityEngine;

public class GroundCheck : MonoBehaviour {

    int collisions = 0;

	public bool InAir { get { return collisions == 0; } }

    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.tag == "Ground") {
            collisions++;
        }
    }

    void OnTriggerExit2D(Collider2D coll) {
        if (coll.tag == "Ground") {
            collisions--;
		}
    }
}
