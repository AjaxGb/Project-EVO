using UnityEngine;

/*
 *  Exactly one of these should be placed in every level, at the beginning.
 */
public class EvoCharacter : MonoBehaviour {
	
	public static EvoCharacter inst { get; private set; }

	struct PersistVar {
		public float maxHealth;
		public float currHealth;
	}
	private PersistVar persist;

	private Rigidbody2D rb;

	public bool OnGround { get; private set; }

	// Allow outside access (get only) of persistent variables
	public float MaxHealth { get { return persist.maxHealth; } }
	public float CurrHealth { get { return persist.currHealth; } }

	// Inspector variables
	public float walkSpeed = 500.0f;
	public float jumpForce = 200.0f;
	public float speedDamping = 2f;

	public Vector2 feetPos;
	public Collider2D feetCollider;
	public PhysicsMaterial2D walkingMaterial;
	public PhysicsMaterial2D stillMaterial;
	public float onGroundDistance = 0.1f;
	public LayerMask onGroundLayers;

	void Start () {
		if (inst == null) {
			// Set up persistent variables at start of game
			persist.maxHealth = 20f;
			persist.currHealth = persist.maxHealth;
		} else {
			// Copy persistent variables over from last scene
			persist = inst.persist;
		}
		inst = this;

		rb = GetComponent<Rigidbody2D>();
	}
	
	void Update () {
		if (Time.timeScale == 0) return;

		CheckGround();

		// TEST MOVEMENT
		float x = Input.GetAxis("Horizontal") * Time.deltaTime * (walkSpeed - rb.velocity.x);
		if (Mathf.Abs(x) > 0.1f) {
			feetCollider.sharedMaterial = walkingMaterial;
		} else {
			feetCollider.sharedMaterial = stillMaterial;
			x -= rb.velocity.x * speedDamping * Time.deltaTime;
		}
		float y = 0;
		if (OnGround && Input.GetButton("Jump")) {
			y = jumpForce;
		}
		rb.AddForce(new Vector2(x, y));
	}

	private void CheckGround() {
		Vector2 worldFeet = transform.TransformPoint(feetPos);
		OnGround = Physics2D.Raycast(worldFeet, -transform.up, onGroundDistance, onGroundLayers);
		Debug.DrawRay(worldFeet, -transform.up * onGroundDistance, OnGround ? Color.green : Color.red);
	}
}
