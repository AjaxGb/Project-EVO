using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour, IKillable, IDamageable {

    //upgrades
    public bool hasDoubleJump;
    public bool hasGlide;
    public bool hasAttack;

    //movement variables
    public float inAirSlow = 0.5f;
    public float boulderPushSlow = 0.56f;
    public float glideSlow = 0.3f;
    public float glideFallSpeed = 2f;
    public float jumpForce = 7;
    //public float jumpForceStay = 0.2f;
	protected float gravityScale;
    public float maxSpeed = 10f;
    public float walkingAcceleration;
    public float walkingDecceleration;
    public float climbSpeed;
    public float offWallJumpDelay = 0.4f;
    private float lastJumpTime = 0.0f;
    public float damage = 25;

    //checks around player and related
    public GroundCheck groundCheck;
    public GroundCheck leftCheck;
    public GroundCheck rightCheck;
    private FallingRock boulder; //the attached boulder if pushing/pulling
	private DistanceJoint2D boulderJoint;
	public float maxPullForce = 70f;
    public float PlatformDisableTime = 0.3f;
    private int layerBits;
    private Queue<GameObject> disabledPlatforms = new Queue<GameObject>();
    private Queue<float> disableTime = new Queue<float>();

    //control system
	public bool realControls = true;
	public IControls control;

	//other stuff
	public float DeathTime { get; protected set; }
	public bool IsAlive { get { return DeathTime == 0; } }
	public bool InAir { get { return groundCheck.InAir; } }
    public bool leftClimb { get { return !leftCheck.InAir; } }
    public bool rightClimb { get { return !rightCheck.InAir; } }
    public bool canJump = true;
    public enum States {NEUTRAL, CLIMB, GLIDE, PULL};
    public States actionState = States.NEUTRAL;
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

    public AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip hitAttackSound;
    public AudioClip missedAttackSound;
    public AudioClip deathSound;

    // Use this for initialization
    protected void Start () {
		if (realControls) {
			control = new ControlsReal();
		}
        layerBits = 1 << LayerMask.NameToLayer("Player");
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
		
		//===ACTIVATE OBJECTS===
		UpdateActivatable();
        if (currActivatable && control.GetAxis(AxisId.VERTICAL) > 0 && currActivatable.CanActivate && !upAxisInUse) {
            currActivatable.Activate(this);
            upAxisInUse = true;
        } else if (control.GetAxis(AxisId.VERTICAL) <= 0) {
			upAxisInUse = false;
		}

        //===ATTACK===
        if (control.GetButtonDown(ButtonId.ATTACK) && hasAttack && !InAir && actionState == States.NEUTRAL) {
            //attack here
            animator.SetTrigger("Attack");

            
            Collider2D[] thingsHit;
            if (renderer.flipX) {
                //attack to the left
                thingsHit = Physics2D.OverlapCircleAll(transform.position + new Vector3(0.75f, 0, 0), 0.5f);
            } else {
                //attack to the right
                thingsHit = Physics2D.OverlapCircleAll(transform.position + new Vector3(-0.75f, 0, 0), 0.5f);
            }
            foreach (Collider2D c in thingsHit) {
                //deal damage to anything in claw range that is damageable and is not the player
                if (c.gameObject.GetComponent<IDamageable>() != null && !c.gameObject.CompareTag("Player"))
                {
                    c.gameObject.GetComponent<IDamageable>().TakeDamage(damage);
                    audioSource.clip = hitAttackSound;
                    audioSource.Play();
                }
                else
                {
                    audioSource.clip = missedAttackSound; //Still play a sound for attack even if nothing hit
                    audioSource.Play();
                }
            }

            //cancels glide
            if (actionState == States.GLIDE)
                actionState = States.NEUTRAL;
        }
	}

	protected virtual void DeadUpdate() {}

	void FixedUpdate() {
        //skip this if game is paused
        if (Time.timeScale == 0 || !IsAlive) return;

        //check inair
        animator.SetBool("InAir", InAir);
        if (InAir) {
            
        } else {
            canJump = true;
        }

        //if there are disabled platforms, reinable them
        if (disabledPlatforms.Count > 0) {
            for (int i = 0; i < disabledPlatforms.Count && disableTime.Peek() < Time.time; i++) {
                GameObject g = disabledPlatforms.Dequeue();
                disableTime.Dequeue();
                g.GetComponent<PlatformEffector2D>().colliderMask |= layerBits;
            }
        }

        //===JUMPING===
		//double jump
        if (control.GetButtonDown(ButtonId.JUMP) && canJump && InAir && hasDoubleJump) {
            if(actionState == States.PULL)
                endPull();
            animator.SetTrigger("jump");
            body.velocity = new Vector2(body.velocity.x, jumpForce);// + body.velocity.y * jumpForceStay);
            canJump = false;
        } else
        //first jump
        if (control.GetButton(ButtonId.JUMP) && (!InAir || actionState == States.CLIMB)  ) {
            if (actionState == States.PULL)
                endPull();
            //drop thru platform
            if (control.GetButtonDown(ButtonId.JUMP) && control.GetAxis(AxisId.VERTICAL) < 0) {
                animator.SetTrigger("jump");                
                List<Collider2D> groundCols = groundCheck.GetCollisions();
                //get the list of all colliders the bottom ground check is interacting with, then diable them and add them to a list to be reinabled soon
                foreach (Collider2D c in groundCols) {
                    if (c.gameObject.CompareTag("Platform")) {
                        c.gameObject.GetComponent<PlatformEffector2D>().colliderMask &= ~layerBits;
                        disabledPlatforms.Enqueue(c.gameObject);
                        disableTime.Enqueue(Time.time + PlatformDisableTime);
                    }
                }
                canJump = hasDoubleJump;
            } else {
                if (actionState == States.CLIMB) {
                    endClimb();
                    lastJumpTime = Time.time;
                }
                body.velocity = new Vector2(body.velocity.x, jumpForce);// + body.velocity.y * jumpForceStay);
                animator.SetTrigger("jump");
                canJump = hasDoubleJump;
            }
        }

        //===MOVEMENT===
        //right
        if (control.GetAxis(AxisId.HORIZONTAL) > 0) {
            animator.SetBool("isidle", false);
            if(actionState != States.CLIMB && actionState != States.PULL)
                renderer.flipX = false;
            if ((boulder == null && body.velocity.x < maxSpeed)  ||  (boulder != null && body.velocity.x < maxSpeed * boulderPushSlow)) {
                float scaledAccel = walkingAcceleration * Time.fixedDeltaTime; 
                //inair and glide slow
                if (InAir) {
                    scaledAccel *= inAirSlow;
                } else if (actionState == States.GLIDE) {
                    scaledAccel *= glideSlow;
                }
                body.velocity = new Vector2(body.velocity.x + scaledAccel, body.velocity.y);
                
            }
        }
        //left
        if (control.GetAxis(AxisId.HORIZONTAL) < 0) {
            animator.SetBool("isidle", false);
            if (actionState != States.CLIMB && actionState != States.PULL)
                renderer.flipX = true;
            if ((boulder == null && body.velocity.x > -maxSpeed)  ||  (boulder != null && body.velocity.x > -maxSpeed * boulderPushSlow)) {
                float scaledAccel = walkingAcceleration * Time.fixedDeltaTime;  
                //inair and glide slow
                if (InAir) {
                    scaledAccel *= inAirSlow;
                } else if (actionState == States.GLIDE) {
                    scaledAccel *= glideSlow;
                }
                body.velocity = new Vector2(body.velocity.x - scaledAccel, body.velocity.y);
            }
        }

        if (control.GetAxis(AxisId.HORIZONTAL) == 0 && !InAir) {
            animator.SetBool("isidle", true);
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

        //===PULL===
        if (control.GetButton(ButtonId.GLIDE) && !InAir) {
            if (boulder == null) { //if not pulling
                List<Collider2D> cols;
                //check if leftcheck is touching a boulder if the character is facing left
                cols = renderer.flipX ? leftCheck.GetCollisions() : rightCheck.GetCollisions();
				foreach (Collider2D c in cols) {
					if (c.gameObject.tag == "Boulder") {
						startPull(c.GetComponent<FallingRock>());
					}
				}
			} else { //if boulder pulling is already set
                
            }
        } else if (actionState == States.PULL) {
            endPull();
        }

        //===SHIFT KEY ACTIONS===
        //===CLIMBING===
        if (control.GetButton(ButtonId.GLIDE) && Time.time > lastJumpTime + offWallJumpDelay && actionState != States.PULL &&
                ((leftClimb && (control.GetAxis(AxisId.VERTICAL) != 0 || control.GetAxis(AxisId.HORIZONTAL) < 0 || actionState == States.CLIMB)) 
                || (rightClimb && (control.GetAxis(AxisId.VERTICAL) != 0 || control.GetAxis(AxisId.HORIZONTAL) > 0 || actionState == States.CLIMB)))  ) {
            
            //if the climb just started
            if (actionState != States.CLIMB && actionState != States.PULL) {
                body.gravityScale = 0;
                startClimb();
                canJump = hasDoubleJump;
            }
            //climb up or down
            if (control.GetAxis(AxisId.VERTICAL) > 0) {
                animator.SetBool("ActiveClimb", true);
                body.velocity = new Vector2(0, climbSpeed);
            } else if (control.GetAxis(AxisId.VERTICAL) < 0) {
                body.velocity = new Vector2(0, -climbSpeed);
                animator.SetBool("ActiveClimb", true);
            } else {
                body.velocity = new Vector2(0, 0);
                animator.SetBool("ActiveClimb", false);
            }
        } else if (actionState == States.CLIMB){
            endClimb();
        }



        //===GLIDE=== 
        if (control.GetButton(ButtonId.GLIDE) && InAir && hasGlide && PlayerMana > 0 && actionState != States.CLIMB && Time.time > lastJumpTime + offWallJumpDelay) {
            //if the glide has just started
            if (actionState != States.GLIDE) {
                startGlide();
            }
            if (body.velocity.y < -glideFallSpeed) {
                body.velocity = new Vector2(body.velocity.x, -glideFallSpeed);
            }
			PlayerMana -= 0.5f;
		} else if(actionState == States.GLIDE) {
            //glide ended
            endGlide();
        }
        // Check if key is still held to prevent flickering
        if (!(control.GetButton(ButtonId.GLIDE) && InAir) || actionState == States.CLIMB) PlayerMana += 0.1f;
    }


    //====ACTIVATED OBJECTS===
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


    //===GLIDING HELPERS===
    public void startGlide() {
        actionState = States.GLIDE;
        animator.SetBool("IsGliding", true);
    }
    public void endGlide() {
        actionState = States.NEUTRAL;
        animator.SetBool("IsGliding", true);
    }


    //===CLIMBING HELPERS===
    public void startClimb() {
        actionState = States.CLIMB;
        animator.SetBool("isClimbing", true);
    }
    public void endClimb() {
        actionState = States.NEUTRAL;
        body.gravityScale = gravityScale;
        animator.SetBool("isClimbing", false);
    }

    //===PUSH/PULL HELPERS===
    public void startPull(FallingRock rock) {
		if (!rock || rock.state != FallingRock.State.GROUNDED) return;

		boulder = rock;
		boulder.rb.mass = boulder.mobileMass;
        actionState = States.PULL;
		// Move out of its scene, so it doesn't unload
		boulder.transform.parent = transform;
		boulder.transform.parent = null;

		if (boulderJoint) {
			Destroy(boulderJoint);
		}
		boulderJoint = rock.gameObject.AddComponent<DistanceJoint2D>();
		boulderJoint.connectedBody = this.body;
		boulderJoint.maxDistanceOnly = true;
		boulderJoint.enableCollision = true;
		boulderJoint.breakForce = maxPullForce;
    }
    public void endPull() {
		// Move into current scene, so it does unload
		boulder.transform.parent = SceneLoader.inst.currScene.root.transform;

		boulder.rb.mass = boulder.immobileMass;

		boulder = null;
        actionState = States.NEUTRAL;
		Destroy(boulderJoint);
    }

    //===NEW SKILL TRANSITIONS===
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
        audioSource.clip = hurtSound;
        audioSource.Play();
	}

	public void Kill() {
		if (!IsAlive) return;
		PlayerHealth = 0;
        audioSource.clip = deathSound;
        audioSource.Play();
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
		SaveManager.inst.LoadCurrentSave();
	}

}