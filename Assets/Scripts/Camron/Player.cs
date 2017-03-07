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
    private float gravityScale;
    public float maxSpeed = 10f;
    public float walkingAcceleration;
    public float walkingDecceleration;
    public float climbSpeed;

    public GroundCheck groundCheck;
    public GroundCheck leftCheck;
    public GroundCheck rightCheck;

    //other stuff
	public bool InAir { get { return groundCheck.InAir; } }
    public bool leftClimb { get { return !leftCheck.InAir; } }
    public bool rightClimb { get { return !rightCheck.InAir; } }
    public bool canJump = true;
    public bool isGliding;
    public bool isClimbing;
    private Rigidbody2D body;
	private Activatable currActivatable;
    int playerLayer;

    public UIAttributeBar HealthBar;
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

	public UIAttributeBar ManaBar;
	private float _playerMana;
	public float PlayerMana {
		get { return _playerMana; }
		set {
			_playerMana = value;
			ManaBar.Amount = value;
		}
	}

	// Use this for initialization
	void Start () {
        body = GetComponent<Rigidbody2D>();
        gravityScale = body.gravityScale;
		groundCheck = groundCheck ? groundCheck : GetComponentInChildren<GroundCheck>();
        playerLayer = LayerMask.NameToLayer("Player");
        PlayerHealth = 100f;
		PlayerMana = 100f;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeScale == 0) return;

		UpdateActivatable();
		if (currActivatable && Input.GetAxis("Vertical") > 0 && currActivatable.CanActivate) {
			currActivatable.Activate(this);

		}
	}

    void FixedUpdate() {
        if (Time.timeScale == 0) return;

        //double jump
        if (Input.GetButtonDown("Jump") && canJump && InAir && hasDoubleJump) {
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            canJump = false;

        }
        //first jump
        else if (Input.GetButton("Jump") && (!InAir || isClimbing)  ) {
            if (Input.GetAxis("Vertical") < 0) {
                //drop thru platform
                canJump = hasDoubleJump;
            } else {
                if (isClimbing) {
                    endClimb();
                }
                body.velocity = new Vector2(body.velocity.x, jumpForce);
                canJump = hasDoubleJump;
            }
        }

        //left and right movement
        //right
        if (Input.GetAxis("Horizontal") > 0) {
            if (body.velocity.x < maxSpeed) {
                float scaledAccel = walkingAcceleration * Time.fixedDeltaTime;
                if (InAir) {
                    scaledAccel *= inAirSlow;
                } else if (isGliding) {
                    scaledAccel *= glideSlow;
                }
                body.velocity = new Vector2(body.velocity.x + scaledAccel, body.velocity.y);
            }
        }
        //left
        if (Input.GetAxis("Horizontal") < 0) {
            if (body.velocity.x > -maxSpeed) {
                float scaledAccel = walkingAcceleration * Time.fixedDeltaTime;
                if (InAir) {
                    scaledAccel *= inAirSlow;
                } else if (isGliding) {
                    scaledAccel *= glideSlow;
                }
                body.velocity = new Vector2(body.velocity.x - scaledAccel, body.velocity.y);
            }
        }

        if (Input.GetAxis("Horizontal") == 0 && !InAir) {
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

        //wall climb
        if (  Input.GetButton("Glide") && ((leftClimb && (Input.GetAxis("Horizontal") < 0 || isClimbing)) || (rightClimb && (Input.GetAxis("Horizontal") > 0 || isClimbing)))  ) {
            //if the climb just started
            if (!isClimbing) {
                body.gravityScale = 0;
                isClimbing = true;
            }

            //climb up or down
            if (Input.GetAxis("Vertical") > 0) {
                body.velocity = new Vector2(0/*body.velocity.x*/, climbSpeed);
            } else if (Input.GetAxis("Vertical") < 0) {
                body.velocity = new Vector2(0, -climbSpeed);
            } else {
                body.velocity = new Vector2(0, 0);
            }
        } else if (isClimbing){
            endClimb();
        }

        //in air gliding 
        if (  Input.GetButton("Glide") && InAir && hasGlide && PlayerMana > 0 && !isClimbing  ) {
            //if the glide has just started
            if (!isGliding) {
                isGliding = true;
            }
            if (body.velocity.y < -glideFallSpeed) {
                body.velocity = new Vector2(body.velocity.x, -glideFallSpeed);
                PlayerMana -= 0.5f;
            }
        } else if(isGliding) {
            //glide ended
            endGlide();
        }
        // Check if key is still held to prevent flickering
        if (!(Input.GetButton("Glide") && InAir)) PlayerMana += 0.1f;
    }

	private void UpdateActivatable() {
		// Check all activators, highlight nearest one in range
		Activatable nearest = null;
		float sqrDistToNearest = Mathf.Infinity;
		foreach (Activatable a in Activatable.allInScene) {
			if (!a.CanActivate) continue;
			float sqrHighlightDist = a.highlightDistance * a.highlightDistance;
			float sqrDistToPlayer = ((Vector2)this.transform.position - (Vector2)a.transform.position).sqrMagnitude;
			if (sqrDistToPlayer < sqrHighlightDist && sqrDistToPlayer < sqrDistToNearest) {
				nearest = a;
				sqrDistToNearest = sqrDistToPlayer;
			}
		}
		if (nearest != currActivatable) {
			if (currActivatable != null) {
				currActivatable.Highlighted = false;
			}
			currActivatable = nearest;
			if (currActivatable != null) {
				currActivatable.Highlighted = true;
			}
		}
	}

    //Enemy Contact

    void OnCollisionEnter2D(Collision2D collThing) {
        if (collThing.gameObject.tag == "Enemy") { //If the thing collided with is tagged as an enemy

            //Subtract one health from the player and update the UI Amount value
            PlayerHealth -= 1;
        }
    }

    public void endGlide() {
        isGliding = false;
    }

    public void endClimb() {
        isClimbing = false;
        body.gravityScale = gravityScale;
    }

}