using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour {

    int collisions = 0;
    public Player p;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.tag == "Ground") {
            collisions++;
            p.canJump = true;

            if (collisions == 0)
                p.inAir = true;
            else
                p.inAir = false;

        }
    }
    void OnTriggerExit2D(Collider2D coll) {
        if (coll.tag == "Ground") {
            collisions--;

            if (collisions == 0)
                p.inAir = true;
            else
                p.inAir = false;

        }
    }

}
