using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillar : MonoBehaviour {

    public bool active = true;
    public float breakDistance = 25;
    public float breakSpeed;
    public float randomShake;
    bool needToMove = false;
    public GameObject landingZone;

    Vector2 curV;
    Vector2 dest;
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if dest is reached stop moving
        if (Vector2.Distance(transform.position, dest) < 0.25 && needToMove) {
            needToMove = false;
        } else if (needToMove) {
            GetComponent<Rigidbody2D>().velocity = ((dest - (Vector2)transform.position).normalized * breakSpeed) + new Vector2(0, Random.Range(-randomShake, randomShake)) ;
        }
	}

    public void UnBreak() {
        active = true;
        needToMove = true;
        dest = new Vector2(transform.position.x, transform.position.y + breakDistance);
    }

    public void Break() {
        active = false;
        needToMove = true;
        dest = new Vector2(transform.position.x, transform.position.y - breakDistance);
    }

    public void Blast(float damage) {

    }

}
