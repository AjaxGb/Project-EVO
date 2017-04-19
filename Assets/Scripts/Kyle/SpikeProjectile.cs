using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeProjectile : MonoBehaviour {

    public float DamageStrength = 5f; //Amount of damage this enemy inflicts to the player

    public float ShotSpeed = 15f;
	// Use this for initialization
	void Start () {
        Destroy(gameObject, 2); //Destroy the Projectile after 2 seconds if it has not already been destroyed. Ensures destruction if it goes off a bottomless pit
	}
	
	// Update is called once per frame
	void Update () {

        //Ensure that the object rotates according to its velocity
        Vector2 dir = transform.GetComponent<Rigidbody2D>().velocity; //Get Velocity as a Vector2 for direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; //Get Tangent of Y/X for direction Vector2 and convert to degrees
        //Subtract 90 degrees to ensure top is pointed towards Velocity rather than the right side
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); //Rotate the object along the Forward vector by the angle determined above
    }

    void OnCollisionEnter2D(Collision2D TouchedThing)
    {
        if (TouchedThing.gameObject == SceneLoader.inst.player.gameObject)
        {
            SceneLoader.inst.player.TakeDamage(DamageStrength); //Call TakeDamage function on player to deal damage                      
        }

        //Any animation on the object vanishing goes here. If there is a distinct one from damaging the player, make this an "else" statement for the above

        Destroy(gameObject); //Destroy the projectile if it collides with anything.
    }
}
