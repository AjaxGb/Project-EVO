using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class GroundCheck : MonoBehaviour {

    HashSet<Collider2D> collisions = new HashSet<Collider2D>();
    public List<string> collideWith;
	public Rigidbody2D playerRB;
	public bool onlyWhenDown;
	public bool InAir { get { return collisions.Count == 0; } }

    // Use Edit > Project Settings > Physics 2D to control
    // which layers collide with GroundCheck.

    void OnTriggerEnter2D(Collider2D coll) {
		if (playerRB != null && onlyWhenDown) {
			float relativeVelocity = playerRB.velocity.y;
			Rigidbody2D otherRB = coll.GetComponent<Rigidbody2D>();
			if (otherRB != null) {
				relativeVelocity -= otherRB.velocity.y;
			}
			if (relativeVelocity > 0) {
				return;
			}
		}
        string colTag = coll.gameObject.tag;
        foreach (string s in collideWith) {
            if (s.Equals(colTag)) {
				collisions.Add(coll);
				break;
			}
        }
	}

    void OnTriggerExit2D(Collider2D coll) {
		collisions.Remove(coll);
    }

    //returns a list of all objects the check is currently colliding with
    public List<Collider2D> GetCollisions() {
        return collisions.ToList();
    }
}
