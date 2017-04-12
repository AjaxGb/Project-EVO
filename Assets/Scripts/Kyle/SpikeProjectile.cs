using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeProjectile : MonoBehaviour {

    public float DamageStrength = 5f; //Amount of damage this enemy inflicts to the player

    public float ShotSpeed = 10f;
	// Use this for initialization
	void Start () {
        Destroy(gameObject, 2); //Destroy the Projectile after 2 seconds if it has not already been destroyed. Ensures destruction if it goes off a bottomless pit
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D TouchedThing)
    {
        if (TouchedThing.gameObject.tag == "Player")
        {
            SceneLoader.inst.player.TakeDamage(DamageStrength); //Call TakeDamage function on player to deal damage                      
        }

        //Any animation on the object vanishing goes here. If there is a distinct one from damaging the player, make this an "else" statement for the above

        Destroy(gameObject); //Destroy the projectile if it collides with anything.
    }
}
