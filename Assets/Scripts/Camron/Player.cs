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
	protected float gravityScale;
    public float maxSpeed = 10f;
    public float walkingAcceleration;
    public float walkingDecceleration;
    public float climbSpeed;

    public GroundCheck groundCheck;
    public GroundCheck leftCheck;
    public GroundCheck rightCheck;

	public bool realControls = true;
	public IControls control;

	//other stuff
	public float DeathTime { get; protected set; }
	public bool IsAlive { get { return DeathTime == 0; } }
	public bool InAir { get { return groundCheck.InAir; } }
    public bool leftClimb { get { return !leftCheck.InAir; } }
    public bool rightClimb { get { return !rightCheck.InAir; } }
    public bool canJump = true;
    public bool isGliding;  //possibly change this to a state enum, especially if others like push/pull are going to be added
    public bool isClimbing;
	public PhysicsMaterial2D deathPhysMaterial;
	protected PhysicsMaterial2D alivePhysMaterial;
	protected Rigidbody2D body;
	protected new Collider2D collider;
	protected new SpriteRenderer renderer;
	protected Activatable currActivatable;

    //animation
	protected Animator animator;

    public UIAttributeBar HealthBar;
    private float _playerHealth; //Variable for the player's health total, ease updating of this and the UI at the same time
    public float PlayerHealth
    {
        get { return _playerHealth; }
        set
        {
            _playerHealth = value;
            if (HealthBar != null) HealthBar.Amount = value;
        }
    }
	private float _maxHealth;
	public float MaxHealth {
		get { return _maxHealth; }
		set {
			_maxHealth = value;
			if (HealthBar != null) HealthBar.MaxAmount = value;
		}
	}

	public UIAttributeBar ManaBar;
	private float _playerMana;
	public float PlayerMana {
		get { return _playerMana; }
		set {
			_playerMana = value;
			if (ManaBar != null) ManaBar.Amount = value;
		}
	}
	private float _maxMana;
	public float MaxMana {
		get { return _maxMana; }
		set {
			_maxMana = value;
			if (ManaBar != null) ManaBar.MaxAmount = value;
		}
	}

	private bool upAxisInUse;
	public virtual bool isRealPlayer { get { return true; } }

	// Use this for initialization
	protected void Start () {
		if (realControls) {
			control = new ControlsReal();
		}
		renderer = GetComponent<SpriteRenderer>();
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
			} else {
				DeadUpdate();
			}
		}
		
		//activate objects
		UpdateActivatable();
        if (currActivatable && control.GetAxis(AxisId.VERTICAL) > 0 && currActivatable.CanActivate && !upAxisInUse) {
            currActivatable.Activate(this);
            upAxisInUse = true;
        } else if (control.GetAxis(AxisId.VERTICAL) <= 0) {
			upAxisInUse = false;
		}

        //attack
        if (control.GetButtonDown(ButtonId.ATTACK) && hasAttack) {
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

	protected virtual void DeadUpdate() {}

	void FixedUpdate() {
        if (Time.timeScale == 0 || !IsAlive) return;

        if (!InAir) {
            canJump = true;
        }

		//double jump
        if (control.GetButtonDown(ButtonId.JUMP) && canJump && InAir && hasDoubleJump) {
            body.velocity = new Vector2(body.velocity.x, jumpForce);
            canJump = false;
        } else
        //first jump
        if (control.GetButton(ButtonId.JUMP) && (!InAir || isClimbing)  ) {
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
                startClimb();
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


    //===GLIDING===
    public void endGlide() {
        isGliding = false;
    }


    //===CLIMBING===
    public void endClimb() {
        isClimbing = false;
        body.gravityScale = gravityScale;
        animator.SetBool("isClimbing", false);
    }
    public void startClimb() {
        isClimbing = true;
        animator.SetBool("isClimbing", true);
    }


    //===NEW SKILLS===
    public void LearnClaws() {
        hasAttack = true;
        animator.SetTrigger("GainClaws");
    }

    public void LearnWings() {
        hasDoubleJump = true;
        hasGlide = true;
        animator.SetTrigger("GainWings");
    }

    public void LearnTime() {
        //hasTime = true;
        animator.SetTrigger("GainTime");
    }


    //===DAMAGE AND DEATH===
    public float TakeDamage(float d) {
		if (!IsAlive) return 0;
        d = Mathf.Min(d, PlayerHealth);
        PlayerHealth -= d;
		if (d > 0) {
			OnDamaged(d);
		}
        if (PlayerHealth <= 0) {
            OnDeath();
        }
        return d;
    }

	protected virtual void OnDamaged(float d) {
		ScreenTint.inst.StartFade(0, new Color(1, 0, 0, 0.2f), 0.3f, true, true);
	}

	public void Kill() {
		if (!IsAlive) return;
		PlayerHealth = 0;
		OnDeath();
	}

    public virtual void OnDeath() {
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

	public virtual void OnRespawn() {
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