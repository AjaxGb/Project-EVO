using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour {

    int collisions = 0;
    public Player p;

    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.tag == "Ground") {
            collisions++;
            p.canJump = true;

			p.inAir = (collisions == 0);
        }
    }

    void OnTriggerExit2D(Collider2D coll) {
        if (coll.tag == "Ground") {
            collisions--;

			p.inAir = (collisions == 0);
		}
    }
}
