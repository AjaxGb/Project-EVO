using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryEnemy : MonoBehaviour, IKillable, IDamageable {

    Rigidbody2D enemyRB;
    public LayerMask enemyMask; //Will need to give enemies a layer to work with enemyMask

    //combat stats
    public float DamageStrength = 5; //Amount of damage this enemy inflicts to the player
    public float maxHP = 1;
    public float curHP = 1;

    Collider2D EnemyColl;

    public float SightRange = 7f; //Distance the enemy can see the player

    Collider2D playerCollider;

    bool CanShoot = true;
    public float ShotDelay = 1f; //Time waited between firing projectiles
    public SpikeProjectile Projectile;

    // Use this for initialization
    void Start () {
        enemyRB = this.GetComponent<Rigidbody2D>(); //Enemy has a Rigidbody and its transform for positioning checks

        SpriteRenderer enemySprite = this.GetComponent<SpriteRenderer>();
        EnemyColl = this.GetComponent<Collider2D>();

        playerCollider = SceneLoader.inst.player.GetComponent<Collider2D>(); //Get the Player's Transform
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate() {

		if (Time.timeScale == 0) return;

		//Debug.DrawRay(enemyTransform.position, (playerTransform.position - enemyTransform.position), Color.black); //Draws ray from enemy to player

		Vector2 playerCenterOfMass = playerCollider.bounds.center;
		Vector2 dirToPlayer = playerCenterOfMass - (Vector2)transform.position;

		if (dirToPlayer.sqrMagnitude <= SightRange * SightRange && CanShoot) //Distance between player and Enemy is less than the SightRange, ergo player is in the enemy's view, and the enemy can shoot
        {
            RaycastHit2D[] findPlayer = new RaycastHit2D[1];
            if (EnemyColl.Raycast(dirToPlayer, findPlayer, SightRange) != 0 && findPlayer[0].collider.gameObject.IsChildOf(SceneLoader.inst.player.gameObject)) //Raycast from enemy to player, see if first thing intersected is the player
            {
                //Cameron: This is where it will shoot a projectile if it's allowed to shoot at the time
                SpikeProjectile Shot = Instantiate(Projectile, transform.position, transform.rotation);
                Shot.GetComponent<Rigidbody2D>().velocity = (dirToPlayer).normalized * Shot.ShotSpeed;
                
                //Shot.transform.up = playerTransform.position - transform.position; //Turn the top of the sprite to face the player position when fired

                //Debug.Log("Spotted Player");
                CanShoot = false;
                Invoke("WaitNextShot", ShotDelay); //Invoke the Function to let it shoot (CanShoot = true) again after the Delay passes

            }
        }
    }

    //simple taking damage
    public float TakeDamage(float amount)
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
