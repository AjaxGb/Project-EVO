using UnityEngine;

public class Player : MonoBehaviour, IKillable {

    //upgrades
    public bool hasDoubleJump;
    public bool hasGlide;
    public bool hasAttack;

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

	public bool fakeControls = false;
	public IControls control;

	//other stuff
	public float DeathTime { get; private set; }
	public bool IsAlive { get { return DeathTime == 0; } }
	public bool InAir { get { return groundCheck.InAir; } }
    public bool leftClimb { get { return !leftCheck.InAir; } }
    public bool rightClimb { get { return !rightCheck.InAir; } }
    public bool canJump = true;
    public bool isGliding;  //possibly change this to a state enum, especially if others like push/pull are going to be added
    public bool isClimbing;
	public PhysicsMaterial2D deathPhysMaterial;
	private PhysicsMaterial2D alivePhysMaterial;
    private Rigidbody2D body;
	private new Collider2D collider;
	private Activatable currActivatable;
	private Animator animator;

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
	private float _maxHealth;
	public float MaxHealth {
		get { return _maxHealth; }
		set {
			_maxHealth = value;
			HealthBar.MaxAmount = value;
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
	private float _maxMana;
	public float MaxMana {
		get { return _maxMana; }
		set {
			_maxMana = value;
			ManaBar.MaxAmount = value;
		}
	}

	private bool upAxisInUse;

	// Use this for initialization
	void Start () {
		control = (fakeControls ? null : new ControlsReal());
		body = GetComponent<Rigidbody2D>();
		collider = GetComponent<Collider2D>();
		alivePhysMaterial = body.sharedMaterial;
		animator = GetComponent<Animator>();
        gravityScale = body.gravityScale;
		groundCheck = groundCheck ? groundCheck : GetComponentInChildren<GroundCheck>();
		MaxHealth = 100f;
		MaxMana = 100f;
		PlayerHealth = MaxHealth;
		PlayerMana = MaxMana;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeScale == 0) return;
		if (!IsAlive) {
			DeathTime -= Time.deltaTime;
			if (DeathTime <= 0) {
				DeathTime = 0;
				OnRespawn();
			}
		}
		
		//activate objects
		UpdateActivatable();
        if (currActivatable && Input.GetAxis("Vertical") > 0 && currActivatable.CanActivate && !upAxisInUse) {
            currActivatable.Activate(this);
            upAxisInUse = true;
        } else if (control.GetAxis(AxisId.VERTICAL) <= 0) {
			upAxisInUse = false;
		}

        //attack
        if (Input.GetButtonDown("Attack") && hasAttack) {
            //attack here
            animator.SetTrigger("Attack");

            if (GetComponent<SpriteRenderer>().flipX) {
                //attack to the left
            } else {
                //attack to the right
            }


            //cancels glide
            if (isGliding)
                isGliding = false;
        }

	}

    void FixedUpdate() {
        if (Time.timeScale == 0 || !IsAlive) return;

        if (!InAir) {
            canJump = true;
        }

        //double jump
        if (control.GetButtonDown(ButtonId.JUMP) && canJump && InAir && hasDoubleJump) {
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            canJump = false;

        }
        //first jump
        else if (control.GetButton(ButtonId.JUMP) && (!InAir || isClimbing)  ) {
            if (control.GetAxis(AxisId.VERTICAL) < 0) {
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
        if (control.GetAxis(AxisId.HORIZONTAL) > 0) {
            animator.SetBool("isidle", false);
            GetComponent<SpriteRenderer>().flipX = false;
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
        if (control.GetAxis(AxisId.HORIZONTAL) < 0) {
            animator.SetBool("isidle", false);
            GetComponent<SpriteRenderer>().flipX = true;
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

        if (control.GetAxis(AxisId.HORIZONTAL) == 0 && !InAir) {
			animator.SetBool("isidle",true);
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
        if (control.GetButton(ButtonId.GLIDE) && ((leftClimb && (control.GetAxis(AxisId.HORIZONTAL) < 0 || isClimbing)) || (rightClimb && (control.GetAxis(AxisId.HORIZONTAL) > 0 || isClimbing)))  ) {
            //if the climb just started
            if (!isClimbing) {
                body.gravityScale = 0;
                isClimbing = true;
                canJump = hasDoubleJump;
            }

            //climb up or down
            if (control.GetAxis(AxisId.VERTICAL) > 0) {
                body.velocity = new Vector2(0/*body.velocity.x*/, climbSpeed);
            } else if (control.GetAxis(AxisId.VERTICAL) < 0) {
                body.velocity = new Vector2(0, -climbSpeed);
            } else {
                body.velocity = new Vector2(0, 0);
            }
        } else if (isClimbing){
            endClimb();
        }

        //in air gliding 
        if (control.GetButton(ButtonId.GLIDE) && InAir && hasGlide && PlayerMana > 0 && !isClimbing  ) {
            //if the glide has just started
            if (!isGliding) {
                isGliding = true;
            }
            if (body.velocity.y < -glideFallSpeed) {
                body.velocity = new Vector2(body.velocity.x, -glideFallSpeed);
            }
			PlayerMana -= 0.5f;
		} else if(isGliding) {
            //glide ended
            endGlide();
        }
        // Check if key is still held to prevent flickering
        if (!(control.GetButton(ButtonId.GLIDE) && InAir) || isClimbing) PlayerMana += 0.1f;
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

    public void endGlide() {
        isGliding = false;
    }

    public void endClimb() {
        isClimbing = false;
        body.gravityScale = gravityScale;
    }

    public float TakeDamage(float d) {
		if (!IsAlive) return 0;
        d = Mathf.Min(d, PlayerHealth);
        PlayerHealth -= d;
		if (d > 0) {
			ScreenTint.inst.StartFade(0, new Color(1, 0, 0, 0.2f), 0.3f, true, true);
		}
        if (PlayerHealth <= 0) {
            OnDeath();
        }
        return d;
    }

	public void Kill() {
		if (!IsAlive) return;
		PlayerHealth = 0;
		OnDeath();
	}

    public void OnDeath() {
		DeathTime = 3.0f; // Wait 3 seconds before respawn
		ScreenTint.inst.StartFade(0, new Color(1, 0, 0, 0.4f), 1);
		SceneLoader.inst.cameraFollow.target = null;
		if (currActivatable != null) {
			currActivatable.Highlighted = false;
			currActivatable = null;
		}
		animator.SetBool("isidle", true);
		body.freezeRotation = false;
		body.gravityScale = gravityScale;
		collider.sharedMaterial = deathPhysMaterial;
		collider.enabled = false;
		collider.enabled = true;
	}

	public void OnRespawn() {
		body.freezeRotation = true;
		body.rotation = 0;
		body.velocity = Vector2.zero;
		body.angularVelocity = 0;
		collider.sharedMaterial = alivePhysMaterial;
		collider.enabled = false;
		collider.enabled = true;
		PlayerHealth = MaxHealth;
		PlayerMana = MaxMana;
		transform.position = SceneLoader.inst.currScene.root.WorldSpaceRespawnPoint;
		SceneLoader.inst.cameraFollow.target = this.transform;
		SceneLoader.inst.cameraFollow.WarpToTarget();
		ScreenTint.inst.RemoveAllTints();
	}

}