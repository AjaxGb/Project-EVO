using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour {
    
    public Transform[] Waypoints; //The Waypoints the enemy normally flies between
    public int WPIndex = 0; //Index in the waypoint array
    public float speed = 2f; //The speed the enemy moves at
    public float accel = 1f;
    public float waypointRange = 2f;

    public float SightRange = 0f;
    public bool Patrolling;

    Transform playerTransform;
    Rigidbody2D rb;
    new SpriteRenderer renderer;

	// Use this for initialization
	void Start () {
        Patrolling = true;
        playerTransform = SceneLoader.inst.player.transform; //Get the Player's Transform
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        Debug.DrawLine(transform.position, transform.position + transform.up, Color.red);
        if (Input.GetKeyDown("m")) {
            IncrementWPIndexLoop();
        }

        if (Vector3.Distance(transform.position, playerTransform.position) <= SightRange && Patrolling) //Distance between player and Enemy is less than the SightRange, ergo player is in the enemy's view, and the enemy is in patrolling behavior
        {
            RaycastHit2D[] findPlayer = new RaycastHit2D[1];
            if (GetComponent<BoxCollider2D>().Raycast(playerTransform.position - transform.position, findPlayer, SightRange) != 0 && findPlayer[0].collider.gameObject.IsChildOf(SceneLoader.inst.player.gameObject)) //Raycast from enemy to player, see if first thing intersected is the player
            {
                //Debug.Log("Spotted Player");
                
                //Patrolling = false; //Enemy is attacking, not patrolling
                //Swoop(); //Swoop at the player
            }
        }
        else if (Patrolling) { //If the enemy does not see the player, do this if it's Patrolling
            //Move towards Waypoints[WPIndex]

            Vector2 targetVel = Waypoints[WPIndex].position - transform.position;
            // Check dist to waypoint
            if (targetVel.sqrMagnitude < waypointRange * waypointRange)
            {
                // Change target to next waypoint
                IncrementWPIndexLoop();
                targetVel = Waypoints[WPIndex].position - transform.position;
            }

            if (targetVel.sqrMagnitude > speed * speed)
            {
                targetVel.Normalize();
                targetVel *= speed;
            }
            if (targetVel.x != 0)
            {
                renderer.flipX = targetVel.x > 0;
            }

            Vector2 targetAcc = targetVel - rb.velocity;
            if (targetAcc.sqrMagnitude > accel * accel)
            {
                targetAcc.Normalize();
                targetAcc *= accel;
            }
            rb.AddForce(targetAcc);
        }
    }

    void IncrementWPIndexLoop() {
        WPIndex = (++WPIndex) % Waypoints.Length;
    }
}
