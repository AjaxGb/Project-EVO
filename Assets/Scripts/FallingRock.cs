using System;
using UnityEngine;

public class FallingRock : MonoBehaviour, IKillable {

	public enum State {
		FALL_IN,
		GROUNDED,
		DROPPING
	}

	public bool destructable = true;
	public State state = State.FALL_IN;
	public LayerMask groundLayers;

	public float damage = 20f;
	public float minFallSpeed = 5f;

	private Rigidbody2D rb;
	private new Collider2D collider;

	private Collider2D[] groundCollider;

	// Use this for initialization
	void Start() {
		rb = GetComponent<Rigidbody2D>();
		collider = GetComponent<Collider2D>();
		groundCollider = new Collider2D[1];
	}

	// Update is called once per frame
	void Update() {
		Vector2 baseCenter = transform.position;
		baseCenter.y -= collider.bounds.size.y;
		Vector2 baseSize = collider.bounds.size;
		baseSize.y = 0.1f;
		switch (state) {
			case State.FALL_IN:
				if (0 != Physics2D.OverlapBoxNonAlloc(baseCenter, baseSize, 0, groundCollider, groundLayers)) {
					state = State.GROUNDED;
				}
				break;
			case State.GROUNDED:
				if (0 == Physics2D.OverlapBoxNonAlloc(baseCenter, baseSize, 0, groundCollider, groundLayers)) {
					state = State.DROPPING;
				}
				break;
			case State.DROPPING:
				if (rb.velocity == Vector2.zero) {
					if (destructable) {
						Kill();
					} else {
						state = State.GROUNDED;
					}
				}
				break;
		}
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (state != State.GROUNDED
				&& coll.relativeVelocity.y > minFallSpeed
				&& coll.transform.position.y < this.transform.position.y) {
			IDamageable damageable = coll.gameObject.GetComponentInParent<IDamageable>();
			if (damageable != null) {
				damageable.TakeDamage(damage);
				Kill();
			}
		}
	}

	public void Kill() {
		Destroy(transform.gameObject);
	}
}