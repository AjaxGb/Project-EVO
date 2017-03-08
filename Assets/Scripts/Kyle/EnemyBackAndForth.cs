using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBackAndForth : MonoBehaviour {

    Rigidbody2D enemyRB;
    public LayerMask enemyMask; //Will need to give enemies a layer to work with enemyMask
    Transform enemyTransform;
    float enemyWidth, enemyHeight;
    public float speed = -1; //Movement speed of enemy, adjustable. Negative for left, positive for right. Default to moving left at 1 speed

    public float DamageStrength = 5; //Amount of damage this enemy inflicts to the player

	// Use this for initialization
	void Start () {
        enemyTransform = this.transform;
        enemyRB = this.GetComponent<Rigidbody2D>(); //Enemy has a Rigidbody and its transform for positioning checks

        SpriteRenderer enemySprite = this.GetComponent<SpriteRenderer>();
        enemyWidth = enemySprite.bounds.extents.x; //Find width of the enemy sprite and check against the horizontal bounds for edge detection on isGrounded
        enemyHeight = enemySprite.bounds.extents.y; //Height of the enemy for use in isBlocked
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate() {

        //Only move forward if there is ground ahead, ergo not at the edge
        Vector2 lineCastPos = enemyTransform.position.toVector2() - enemyTransform.right.toVector2() * enemyWidth + Vector2.up * enemyHeight; //Origin of Linecast will be top left corner. Cast lines from that position

        Debug.DrawLine(lineCastPos, lineCastPos + Vector2.down * 1.1f); //Debug statement, draw the line beneath the position
        bool isGrounded = Physics2D.Linecast(lineCastPos, lineCastPos + Vector2.down * 1.1f, enemyMask); //enemyMask ensures it won't check against the enemy's own collider, only the ground

        Debug.DrawLine(lineCastPos, lineCastPos - enemyTransform.right.toVector2() * 0.5f ); //Debug statement, draw the line horizontal from the position
        bool isBlocked = Physics2D.Linecast(lineCastPos, lineCastPos - enemyTransform.right.toVector2() * 0.5f, enemyMask); //Similar to above, now check for blocks such as walls

        //No ground ahead, turn enemy around
        if (!isGrounded) {
            Vector3 currRot = enemyTransform.eulerAngles;
            currRot.y += 180;
            enemyTransform.eulerAngles = currRot;
        }

        //Movement of enemy every frame
        Vector2 enemyVelo = enemyRB.velocity;
        enemyVelo.x = enemyTransform.right.x * speed;
        enemyRB.velocity = enemyVelo;
    }

    void OnCollisionEnter2D(Collision2D TouchedThing) {
        if (TouchedThing.gameObject.tag == "Player" ) {
            SceneLoader.inst.player.TakeDamage(DamageStrength);
        }
        
    }
}
