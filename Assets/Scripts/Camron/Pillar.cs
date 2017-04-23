using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillar : MonoBehaviour {

    public bool active = true;
    public float breakDistance = 25;
    public float breakSpeed;
    bool needToMove = false;

    Vector2 curV;
    Vector2 dest;
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if dest is reached stop moving
        if (Vector2.Distance(transform.position, dest) < 0.25) {
            needToMove = false;
        }
        //if the pillar needs to move, move it
        if (!active && needToMove) {
            Vector2.SmoothDamp(transform.position, dest, ref curV, breakSpeed, 1000, Time.deltaTime);
        } else if (needToMove) {
            Vector2.SmoothDamp(transform.position, dest, ref curV, breakSpeed, 1000, Time.deltaTime);
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

}
