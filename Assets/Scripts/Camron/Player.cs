using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    //upgrades
    public bool hasDoubleJump;
    public bool hasGlide;

    //movement variables
    public float inAirSlow;
    public float glideSlow;
    public float glideFallSpeed;
    public float jumpForce;
    public float maxSpeed;
    public float walkingAcceleration;
    public float walkingDecceleration;

    //other stuff
    public bool canJump = true;
    public bool inAir;
    public bool isGliding;
    private Rigidbody2D body;
    public GroundCheck gc;

	// Use this for initialization
	void Start () {
        body = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate() {

        if (isGliding && body.velocity.y < -glideFallSpeed) {
            body.velocity = new Vector2(body.velocity.x, -glideFallSpeed);
        }

        //double jump
        if (Input.GetKeyDown(KeyCode.Space) && canJump && inAir && hasDoubleJump) {
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            canJump = false;
            
        } 
        //first jump
        else if (Input.GetKey(KeyCode.Space) && !inAir) {
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            canJump = hasDoubleJump;
        }

        //left and right movement
        //right
        if (Input.GetKey(KeyCode.D)) {
            if (body.velocity.x < maxSpeed) {
                if (inAir) {
                    body.velocity = new Vector2(body.velocity.x + (walkingAcceleration * Time.fixedDeltaTime), body.velocity.y);
                } else if (isGliding) {
                    body.velocity = new Vector2(body.velocity.x + (walkingAcceleration * Time.fixedDeltaTime * glideSlow), body.velocity.y);
                } else {
                    body.velocity = new Vector2(body.velocity.x + (walkingAcceleration * Time.fixedDeltaTime * inAirSlow), body.velocity.y);
                }
            }
        }
        //left
        if (Input.GetKey(KeyCode.A)) {
            if (-body.velocity.x < maxSpeed) {
                if (inAir) {
                    body.velocity = new Vector2(body.velocity.x - (walkingAcceleration * Time.fixedDeltaTime), body.velocity.y);
                } else if (isGliding) {
                    body.velocity = new Vector2(body.velocity.x - (walkingAcceleration * Time.fixedDeltaTime * glideSlow), body.velocity.y);
                } else {
                    body.velocity = new Vector2(body.velocity.x - (walkingAcceleration * Time.fixedDeltaTime * inAirSlow), body.velocity.y);
                }
            }
        }

        if (  !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !inAir ) {
            //if player is moving left, add velocity to the right, but stop at 0
            if (body.velocity.x < 0) {
                body.velocity = new Vector2(body.velocity.x + walkingDecceleration * Time.fixedDeltaTime, body.velocity.y);
                if (body.velocity.x > 0) {
                    body.velocity = new Vector2(0, body.velocity.y);
                }
            //if player is moving right, add velocity to the left, but stop at 0
            } else if (body.velocity.x > 0) {
                body.velocity = new Vector2(body.velocity.x - walkingDecceleration * Time.fixedDeltaTime, body.velocity.y);
                if (body.velocity.x < 0) {
                    body.velocity = new Vector2(0, body.velocity.y);
                }
            }
        }

        //glide
        if (Input.GetKey(KeyCode.LeftShift) && inAir) {
            //if the glide has just started
            if (!isGliding) {
                
            }

            isGliding = true;
        } else {
            //if the glide has just ended:
            if (isGliding) {
                
            }

            isGliding = false;
        }

    }

}
