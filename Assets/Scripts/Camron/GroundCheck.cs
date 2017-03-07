using UnityEngine;
using System.Collections.Generic;

public class GroundCheck : MonoBehaviour {

    int collisions = 0;
    public List<string> collideWith;
	public bool InAir { get { return collisions == 0; } }

    // Use Edit > Project Settings > Physics 2D to control
    // which layers collide with GroundCheck.

    void OnTriggerEnter2D(Collider2D coll) {
        bool doesCollide = false;
        string colTag = coll.gameObject.tag;
        foreach (string s in collideWith) {
            if (s.Equals(colTag)) {
                doesCollide = true;
            }
        }
        if (doesCollide) {
            collisions++;
        }
	}

    void OnTriggerExit2D(Collider2D coll) {
        bool doesCollide = false;
        string colTag = coll.gameObject.tag;
        foreach (string s in collideWith) {
            if (s.Equals(colTag)) {
                doesCollide = true;
            }
        }
        if (doesCollide) {
            collisions--;
        }
    }


}
