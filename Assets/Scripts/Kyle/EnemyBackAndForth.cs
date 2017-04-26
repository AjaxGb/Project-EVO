using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBackAndForth : MonoBehaviour, IKillable, IDamageable {

    Rigidbody2D enemyRB;
    public LayerMask enemyMask; //Will need to give enemies a layer to work with enemyMask
    Transform enemyTransform;
    float enemyWidth, enemyHeight;

    //combat stats
    public float speed = -1; //Movement speed of enemy, adjustable. Negative for left, positive for right. Default to moving left at 1 speed
    public float DamageStrength = 5; //Amount of damage this enemy inflicts to the player
    public float maxHP = 1;
    public float curHP = 1;

    Collider2D EnemyColl;

    public float ChargeSightRange = 3f; //Distance the enemy can see the player

    // Use this for initialization
    void Start () {
        enemyTransform = this.transform;
        enemyRB = this.GetComponent<Rigidbody2D>(); //Enemy has a Rigidbody and its transform for positioning checks

        SpriteRenderer enemySprite = this.GetComponent<SpriteRenderer>();
        enemyWidth = enemySprite.bounds.extents.x; //Find width of the enemy sprite and check against the horizontal bounds for edge detection on isGrounded
        enemyHeight = enemySprite.bounds.extents.y; //Height of the enemy for use in isBlocked
        
        EnemyColl = this.GetComponent<Collider2D>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate() {

        //Only move forward if there is ground ahead, ergo not at the edge
        Vector2 lineCastPos = enemyTransform.position.toVector2() - enemyTransform.right.toVector2() * enemyWidth; //Origin of Linecast will be top left corner. Cast lines from that position

        Debug.DrawLine(lineCastPos, lineCastPos + Vector2.down * enemyHeight); //Debug statement, draw the line beneath the position
        bool isGrounded = Physics2D.Linecast(lineCastPos, lineCastPos + Vector2.down * enemyHeight, enemyMask); //enemyMask ensures it won't check against the enemy's own collider, only the ground

        Debug.DrawLine(lineCastPos, lineCastPos - enemyTransform.right.toVector2() * 0.1f ); //Debug statement, draw the line horizontal from the position
        bool isBlocked = Physics2D.Linecast(lineCastPos, lineCastPos - enemyTransform.right.toVector2() * 0.1f, enemyMask); //Similar to above, now check for blocks such as walls. Much shorter line
		
        Debug.DrawRay(transform.position, transform.right * Mathf.Sign(speed) * ChargeSightRange); //Draws ray for enemy's "sight" to charge at player

		//No ground ahead underneath, turn enemy around nd revert to "normal" speed
        if (!isGrounded || isBlocked) { //In this check, if line meets something indicating blockage, turn around
			speed = -1;
            Vector3 currRot = enemyTransform.eulerAngles;
            currRot.y += 180;
            enemyTransform.eulerAngles = currRot;
        }

        RaycastHit2D[] hitPlayer = new RaycastHit2D[1]; //If enemy spots the player, call the function to charge at them
		//Debug.Log(hitPlayer[0]);
        if (EnemyColl.Raycast(enemyTransform.right*speed, hitPlayer, ChargeSightRange) != 0
			&& hitPlayer[0].collider.gameObject.IsChildOf(SceneLoader.inst.player.gameObject)) //Check if Raycast intersects the player collider
        {
            //Debug.Log(hitPlayer[0].collider);
            ChargeAtPlayer();
        }

        //Movement of enemy every frame
        Vector2 enemyVelo = enemyRB.velocity;
        enemyVelo.x = enemyTransform.right.x * speed;
        enemyRB.velocity = enemyVelo;

    }

    void OnCollisionEnter2D(Collision2D TouchedThing) {
        if (TouchedThing.gameObject.tag == "Player" ) {

            //TouchedThing.gameObject.GetComponent<Player>().TakeDamage(DamageStrength); //Simpler call on Player component in absence of Sceneloader during testing

            SceneLoader.inst.player.TakeDamage(DamageStrength); //Call TakeDamage function on player to deal damage equal to Enemy Strength

            speed = -1; //Revert speed and turn enemy around after damaging player. Speed reversion to account for charge attack
            Vector3 currRot = enemyTransform.eulerAngles;
            currRot.y += 180;
            enemyTransform.eulerAngles = currRot;
        }
    }

    //simple taking damage
    public float TakeDamage(float amount) {
        if (amount > curHP) {
            amount = curHP;
        }
        curHP -= amount;
        if (curHP <= 0) {
            Kill();
        }

        return amount;
    }

	public void Kill() {
		// TODO: health, particle effects, etc.
		Destroy(gameObject);
	}

    public void ChargeAtPlayer() {
        speed = -4; //Make enemy move faster towards the player

        //Trying to figure out a way to "stun" enemy briefly if it collides with something 
    }
}
