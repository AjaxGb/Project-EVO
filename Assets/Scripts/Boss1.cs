using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1 : MonoBehaviour, IKillable, IDamageable {

	public LayerMask wallMask;
	public float health = 140;

	public float speed = 1f;
	public float chargeSpeed = 4f;

	public float touchDamage = 5; //Amount of damage this enemy inflicts to the player
	public float chargeDamage = 10; //Amount of damage inflicted by successful charge
	public float chargeSightRange = 3f; //Distance the enemy can see the player

	public float stunLength = 3f;
	public float stunTimer = 0f;

	public bool isCharging = false;
	[SerializeField]
	private bool _facingLeft = true;
	public bool FacingLeft {
		get { return _facingLeft; }
		private set {
			if (_facingLeft != value) {
				sprite.flipX = !sprite.flipX;
				_facingLeft = value;
			}
		}
	}
	public int FacingScale { get { return _facingLeft ? -1 : 1; } }
	public Rect AheadOfFaceRect {
		get {
			Rect b = collider.bounds.ToRect();
			Vector2 c = b.center;
			if (FacingLeft) {
				c.x -= b.width;
			} else {
				c.x += b.width;
			}
			b.center = c;
			return b;
		}
	}

	private Rigidbody2D rb;
	private new Collider2D collider;
	private SpriteRenderer sprite;
	private RaycastHit2D[] hitPlayer = new RaycastHit2D[1];

	public Vector2 FacePoint {
		get {
			Vector2 p = FacingLeft ? collider.bounds.min : collider.bounds.max;
			p.y = transform.position.y;
			return p;
		}
	}

	// Use this for initialization
	void Start() {
		rb = GetComponent<Rigidbody2D>();
		collider = GetComponent<Collider2D>();
		sprite = GetComponent<SpriteRenderer>();
	}

	void FixedUpdate() {
		if (Time.timeScale == 0 || SceneLoader.inst.currScene.buildIndex != this.gameObject.scene.buildIndex) return;

		if (stunTimer > 0) {
			stunTimer -= Time.fixedDeltaTime;
			return;
		}

		if (!isCharging) {
			Vector2 origin = FacePoint;
			Vector2 direction = (Vector2)transform.right * FacingScale;

			bool isBlocked = Physics2D.Raycast(origin, direction, 0.5f, wallMask).collider != null;
			Debug.DrawRay(origin, direction, isBlocked ? Color.red : Color.green);

			if (isBlocked) { //In this check, if line meets something indicating blockage, turn around
				FacingLeft = !FacingLeft;
			}

			Vector2 playerPos = SceneLoader.inst.player.transform.position;
			bool playerHeightGood = Math.Abs(playerPos.y - transform.position.y) < 3;
			bool playerInFront = FacingLeft ? playerPos.x < transform.position.x : playerPos.x > transform.position.x;

			if (playerInFront && playerHeightGood) {
				isCharging = true;
			}
		}

		//Movement of enemy every frame
		Vector2 enemyVelo = rb.velocity;
		enemyVelo.x = (isCharging ? chargeSpeed : speed) * FacingScale;
		rb.velocity = enemyVelo;

		Utilities.DebugDrawRect(AheadOfFaceRect, Color.blue);
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (isCharging && AheadOfFaceRect.Overlaps(coll.collider.bounds.ToRect())) {
			// Hit something while charging
			IDamageable hit = coll.gameObject.GetComponent<IDamageable>();
			if (hit != null) {
				hit.TakeDamage(chargeDamage);
				FacingLeft = !FacingLeft;
			} else {
				stunTimer = stunLength;
			}
			isCharging = false;
		} else if (coll.gameObject.tag == "Player") {

			coll.gameObject.GetComponent<Player>().TakeDamage(touchDamage);

			FacingLeft = !FacingLeft;
		}
	}

	public float TakeDamage(float amount) {
		if (amount >= health) {
			amount = health;
			health = 0;
			Kill();
			return amount;
		}
		health -= amount;
		return amount;
	}

	public void Kill() {
		Destroy(transform.gameObject);
	}
}
