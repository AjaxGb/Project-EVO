using UnityEngine;
using System.Collections;

public class Playercontrol : MonoBehaviour {

	Rigidbody2D rb;
	public float jumpforce=100f;
	bool isJumping = false;
	float horiz;
	public float moveForce = 50f;
	public float maxSpeed = 5f;

    bool IsGrounded;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)){
			isJumping = true; //If Space is pressed, Set isJumping to true

            //Want to refine this into a check for being grounded, allows application for grabbing and context sensitivity more cleanly later.
		}

		horiz = Input.GetAxis ("Horizontal"); //Use Horizontal Axis to determine direction as a value between -1 and 1, essentially left or right
	}

	void FixedUpdate(){
		if (isJumping) { //If is Jumping, add vertical force to propel character upward
			rb.AddForce(new Vector2(0f, jumpforce));
			isJumping = false; //Set is Jumping back to false. Another reason to refine to a grounded check
		}

		rb.AddForce (Vector2.right * horiz * moveForce); //Add a force to the rigid body equal to the horizontal direction and the movement force value of the player (effectively speed)
		if (Mathf.Abs (rb.velocity.x) > maxSpeed) { //Check to ensure player velocity does not exceed a maximum speed in either direction.
			rb.velocity = new Vector2(Mathf.Sign (rb.velocity.x)*maxSpeed, rb.velocity.y);
		}
	}
}
