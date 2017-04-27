using System;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour, IDamageable, IKillable {
    
    public enum State
    {
        PATROLLING,
        SURPRISED,
        ATTACKING
    }

    public float maxHP = 2f;
    public float currHP = 2f;

    public Transform[] Waypoints; //The Waypoints the enemy normally flies between
    public int WPIndex = 0; //Index in the waypoint array

	public readonly int animStateKey = Animator.StringToHash("State");
	private Animator animator;

	private State _state;
    public State CurrState {
		set {
			_state = value;
			animator.SetInteger(animStateKey, (int)value);
		}
		get { return _state; }
	}

    public float patrolSpeed = 2f; //The speed the enemy moves at normally
    public float patrolAccel = 1f;
    public float coolDownRequirement = 3f; //Time to wait before attacking the player again
    public float coolDownTimer; //The actual amount of time the enemy has waited since attacking
    public float nextWaypointRange = 2f; // Distance to waypoint before changing target

    public float DamageStrength = 10f; //The damage this enemy deals to the player
    public float SightRange = 4f;
    public float attackSpeed = 6f; //Speed when charging at the player
    public float attackAccel = 12f;
    public float attackDuration = 5f; //Time spent attacking player before having to initiate a new check
    public float attackTimer;
    Vector2 attackDirection; //Velocity with which to attack player when spotted.

    public float surpriseDuration = 1f;
    public float surpriseTimer;
    public float surpriseDecel = 25f;

    Transform playerTransform;
    Rigidbody2D rb;
    new SpriteRenderer renderer;
	ParticleSystem deathParticles;

	// Use this for initialization
	void Start () {
		playerTransform = SceneLoader.inst.player.transform; //Get the Player's Transform
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		deathParticles = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (Time.deltaTime == 0 || currHP <= 0) return;

        switch (CurrState)
        {
            case State.PATROLLING:
                //Check if the player is in the enemy's range of detection.
                if (coolDownTimer <= 0f && (transform.position - playerTransform.position).sqrMagnitude <= SightRange * SightRange)
                {
                    //Distance between player and Enemy is less than the SightRange, ergo player is in the enemy's view, and the enemy is in patrolling behavior
                    RaycastHit2D[] findPlayer = new RaycastHit2D[1];
                    if (GetComponent<BoxCollider2D>().Raycast(playerTransform.position - transform.position, findPlayer, SightRange) != 0 && findPlayer[0].collider.gameObject.IsChildOf(SceneLoader.inst.player.gameObject)) //Raycast from enemy to player, see if first thing intersected is the player
                    {
						//Debug.Log("Spotted Player");

						CurrState = State.SURPRISED; //Enemy is attacking, not patrolling
                        surpriseTimer = surpriseDuration;
                    }
                }
                else
                {
                    coolDownTimer -= Time.deltaTime; //Add the time that has passed since the last frame to the cooldownTimer

                    //Move towards Waypoints[WPIndex]
                    MoveTowardsTarget(Waypoints[WPIndex].position, patrolSpeed, patrolAccel);
                    if ((Waypoints[WPIndex].position - transform.position).sqrMagnitude <= nextWaypointRange * nextWaypointRange)
                    {
                        IncrementWPIndexLoop();
                    }
                }
                break;
            case State.SURPRISED:
                rb.velocity = Vector2.zero;

                surpriseTimer -= Time.deltaTime;
                if (surpriseTimer <= 0 && rb.velocity.sqrMagnitude < 0.01f)
                {
					CurrState = State.ATTACKING;
                    attackDirection = playerTransform.position - transform.position;
                    if (attackDirection.sqrMagnitude > attackSpeed * attackSpeed)
                    {
                        attackDirection.Normalize();
                        attackDirection *= attackSpeed;
                    }
                    attackTimer = attackDuration;
                    coolDownTimer = coolDownRequirement;
                }
                break;
            case State.ATTACKING:
                attackTimer -= Time.deltaTime; //Increase the attack timer if not patrolling

                if (attackTimer > 0f)
                { //If the attackTimer is less than the attackDuration, ergo the window for an attack is not done. 
                    MoveWithVelocity(attackDirection, attackAccel);
                }
                else
                { //If the alloted amount of time has been spent on an attack
					CurrState = State.PATROLLING; //go back to Patrolling
                    WPIndex = GetNearestWaypoint();
                }
                break;
        }
    }

    void IncrementWPIndexLoop() {
        WPIndex = (++WPIndex) % Waypoints.Length;
    }

    void MoveTowardsTarget(Vector2 target, float maxSpeed, float maxAccel) {

        Vector2 targetVel = target - (Vector2)transform.position; //Target the position the player was spotted at
        if (targetVel.sqrMagnitude > maxSpeed * maxSpeed)
        {
            targetVel.Normalize();
            targetVel *= maxSpeed;
        }
        MoveWithVelocity(targetVel, maxAccel);
    }

    void MoveWithVelocity(Vector2 targetVel, float maxAccel)
    {
        if (targetVel.x != 0)
        {
            renderer.flipX = targetVel.x > 0;
        }

        Vector2 targetAcc = targetVel - rb.velocity;
        if (targetAcc.sqrMagnitude > maxAccel * maxAccel)
        {
            targetAcc.Normalize();
            targetAcc *= maxAccel;
        }
        rb.AddForce(targetAcc);
    }

    int GetNearestWaypoint()
    {
        float sqrShortestDist = float.PositiveInfinity;
        int indexShortest = 0;
        for (int i = 0; i < Waypoints.Length; ++i)
        {
            float sqrCurrDist = (Waypoints[i].position - transform.position).sqrMagnitude;
            if (sqrCurrDist < sqrShortestDist)
            {
                sqrShortestDist = sqrCurrDist;
                indexShortest = i;
            }
        }
        return indexShortest;
    }

    void OnCollisionEnter2D(Collision2D TouchedThing) {

        if (TouchedThing.gameObject == SceneLoader.inst.player.gameObject)
        {
            SceneLoader.inst.player.TakeDamage(DamageStrength); //Call TakeDamage function on player to deal damage                      
        }

        if (CurrState == State.ATTACKING)
        {
			//Debug.Log("Returning to Patrol");
			CurrState = State.PATROLLING; //Set the enemy back to patrolling mode
            rb.velocity = Vector2.zero;
            WPIndex = GetNearestWaypoint();
        }
    }

    public float TakeDamage(float amount)
    {
        if (amount > currHP)
        {
            amount = currHP;
        }
        currHP -= amount;
        if (currHP <= 0)
        {
            Kill();
        }

        return amount;
    }

    public void Kill()
    {
		rb.velocity = Vector2.up;
		rb.gravityScale = 1;
		rb.constraints = RigidbodyConstraints2D.None;
		foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
		{
			c.enabled = false;
		}
		deathParticles.Play();
        Destroy(transform.parent.gameObject, 3);
    }
}
