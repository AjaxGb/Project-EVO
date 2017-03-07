using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour {

    public Rigidbody2D cBody;
    public float scale;
    private Rigidbody2D body;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        body = GetComponent<Rigidbody2D>();	
	}

    void FixedUpdate() {
        body.velocity = new Vector2(-scale * cBody.velocity.x, -scale * cBody.velocity.y);
    }

}
