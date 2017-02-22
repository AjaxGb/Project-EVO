using UnityEngine;
using System.Collections.Generic;

public class GroundCheck : MonoBehaviour {

    int collisions = 0;

    List<PlatformEffector2D> platforms;

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
