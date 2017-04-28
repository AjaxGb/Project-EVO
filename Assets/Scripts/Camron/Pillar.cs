using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillar : MonoBehaviour {

    public bool unBroken = true;
    public float breakDistance = 25;
    public float breakSpeed;
    public float randomShake;
    public float explosionHeight = 1.0f;
    public float explosionRadius = 1.5f;
    public GameObject explosion;
    bool needToMove = false;
    public GameObject landingZone;

	private Rigidbody2D rb;

    Vector2 curV;
    Vector2 dest;
    // Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        //if dest is reached stop moving
        if (Vector2.Distance(transform.position, dest) < 0.25 && needToMove) {
            needToMove = false;
        } else if (needToMove) {
           rb.velocity = ((dest - (Vector2)transform.position).normalized * breakSpeed) + new Vector2(0, Random.Range(-randomShake, randomShake)) ;
        }
	}

    public void UnBreak() {
		if (unBroken) return;
        unBroken = true;
        needToMove = true;
        dest = new Vector2(transform.position.x, transform.position.y + breakDistance);
    }

    public void Break() {
		if (!unBroken) return;
        unBroken = false;
        needToMove = true;
        dest = new Vector2(transform.position.x, transform.position.y - breakDistance);
    }

    public void Blast(float damage) {
        //blast everything on the pillar
        Collider2D[] thingsHit;
        Instantiate(explosion, landingZone.transform);
        thingsHit = Physics2D.OverlapCircleAll(landingZone.transform.position + new Vector3(0, explosionHeight, 0), explosionRadius);
        foreach (Collider2D c in thingsHit) {
            if (c.gameObject.CompareTag("Player")) {
                c.gameObject.GetComponent<Player>().TakeDamage(damage);
            }
        }
    }

}
