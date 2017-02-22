using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    //upgrades
    public bool hasDoubleJump;
    public bool hasGlide;

    //movement variables
    public float inAirSlow = 0.5f;
    public float glideSlow = 0.3f;
    public float glideFallSpeed = 2f;
    public float jumpForce = 10f;
    public float maxSpeed = 10f;
    public float walkingAcceleration = 40f;
    public float walkingDecceleration = 20f;

	public float groundDistance = 1.0f;
	public LayerMask groundLayers;

    //other stuff
    public bool canJump = true;
    public bool inAir;
    public bool isGliding;
    private Rigidbody2D body;

    private float _playerHealth; //Variable for the player's health total, ease updating of this and the UI at the same time
    public float PlayerHealth
    {
        get { return _playerHealth; }
        set
        {
            _playerHealth = value;
            HealthBar.Amount = value;
        }
    }
    public UIAttributeBar HealthBar;

	// Use this for initialization
	void Start () {
        body = GetComponent<Rigidbody2D>();

        PlayerHealth = 100f;

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate() {
		if (Time.timeScale == 0) return;

		// TODO: Add more raycasts to sides.
		inAir = Physics2D.Raycast(transform.position, Vector2.down, groundDistance, groundLayers);

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
				float scaledAccel = walkingAcceleration * Time.fixedDeltaTime;
				if (inAir) {
					scaledAccel *= inAirSlow;
                } else if (isGliding) {
					scaledAccel *= glideSlow;
                }
				body.velocity = new Vector2(body.velocity.x + scaledAccel, body.velocity.y);
			}
        }
        //left
        if (Input.GetKey(KeyCode.A)) {
            if (body.velocity.x > -maxSpeed) {
				float scaledAccel = walkingAcceleration * Time.fixedDeltaTime;
				if (inAir) {
					scaledAccel *= inAirSlow;
				} else if (isGliding) {
					scaledAccel *= glideSlow;
				}
				body.velocity = new Vector2(body.velocity.x - scaledAccel, body.velocity.y);
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

    //Enemy Contact

    void OnCollisionEnter2D(Collision2D collThing) {
        if (collThing.gameObject.tag == "Enemy") { //If the thing collided with is tagged as an enemy

            //Subtract one health from the player and update the UI Amount value
            PlayerHealth -= 1;
        }
    }

}
