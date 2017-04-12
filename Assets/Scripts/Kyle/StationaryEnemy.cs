using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryEnemy : MonoBehaviour {

    Rigidbody2D enemyRB;
    public LayerMask enemyMask; //Will need to give enemies a layer to work with enemyMask
    Transform enemyTransform;

    //combat stats
    public float DamageStrength = 5; //Amount of damage this enemy inflicts to the player
    public float maxHP = 1;
    public float curHP = 1;

    Collider2D EnemyColl;

    public float SightRange = 7f; //Distance the enemy can see the player

    Transform playerTransform;

    bool CanShoot = true;
    public float ShotDelay = 1f; //Time waited between firing projectiles
    public GameObject Projectile;

    // Use this for initialization
    void Start () {
        enemyTransform = this.transform;
        enemyRB = this.GetComponent<Rigidbody2D>(); //Enemy has a Rigidbody and its transform for positioning checks

        SpriteRenderer enemySprite = this.GetComponent<SpriteRenderer>();
        EnemyColl = this.GetComponent<Collider2D>();

        playerTransform = SceneLoader.inst.player.transform; //Get the Player's Transform
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate() {

        //Debug.DrawRay(enemyTransform.position, (playerTransform.position - enemyTransform.position), Color.black); //Draws ray from enemy to player

        if (Vector3.Distance(enemyTransform.position, playerTransform.position) <= SightRange && CanShoot) //Distance between player and Enemy is less than the SightRange, ergo player is in the enemy's view, and the enemy can shoot
        {
            RaycastHit2D[] findPlayer = new RaycastHit2D[1];
            if (EnemyColl.Raycast(playerTransform.position - enemyTransform.position, findPlayer, SightRange) != 0 && findPlayer[0].collider.gameObject.IsChildOf(SceneLoader.inst.player.gameObject)) //Raycast from enemy to player, see if first thing intersected is the player
            {
                //Cameron: This is where it will shoot a projectile if it's allowed to shoot at the time
                GameObject Shot;
                Shot = Instantiate(Projectile, transform.position, transform.rotation);
                Shot.GetComponent<Rigidbody2D>().velocity = (playerTransform.position - transform.position).normalized * Shot.GetComponent<SpikeProjectile>().ShotSpeed;

                //Debug.Log("Spotted Player");
                CanShoot = false;
                Invoke("WaitNextShot", ShotDelay); //Invoke the Function to let it shoot (CanShoot = true) again after the Delay passes

            }
        }
    }

    //simple taking damage
    public float TakeDamge(float amount)
    {
        if (amount > curHP){
            amount = curHP;
        }
        curHP -= amount;
        if (curHP <= 0) {
            Kill();
        }

        return amount;
    }

    public void Kill()
    {
        // TODO: health, particle effects, etc.
        Destroy(gameObject);
    }

    public void WaitNextShot() {
        CanShoot = true;
    }
}
