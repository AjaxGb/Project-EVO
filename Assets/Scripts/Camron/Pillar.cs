using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pillar : MonoBehaviour {

    public bool unBroken = true;
    public float breakDistance = 25;
    public float breakTime = 4;
	public float unbreakTime = 6;
    public float randomShake = 0.1f;
    public float explosionHeight = 1.0f;
    public float explosionRadius = 1.5f;
    public GameObject explosion;
    bool needToMove = false;
    public GameObject landingZone;
	
	Vector2 startPos;
	float moveEndTime;
    // Use this for initialization
	void Start () {
		startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        //if dest is reached stop moving
        if (needToMove) {
			float percentDone = 1 - (moveEndTime - Time.time) / (unBroken ? unbreakTime : breakTime);
			if (percentDone >= 1) {
				percentDone = 1;
				needToMove = false;
			}
			float percentHeight = unBroken ? percentDone : 1 - percentDone;
			transform.position = new Vector2(
				startPos.x + Random.Range(-randomShake, randomShake),
				Mathf.Lerp(startPos.y - breakDistance, startPos.y, percentHeight));
        }
	}

    public void UnBreak() {
		if (unBroken) return;
        unBroken = true;
        needToMove = true;
		moveEndTime = Time.time + unbreakTime;
    }

    public void Break() {
		if (!unBroken) return;
        unBroken = false;
        needToMove = true;
		moveEndTime = Time.time + breakTime;
    }

    public void Blast(float damage) {
        //blast everything on the pillar
        Collider2D[] thingsHit;
        Destroy(Instantiate(explosion, landingZone.transform).gameObject, 4);
        thingsHit = Physics2D.OverlapCircleAll(landingZone.transform.position + new Vector3(0, explosionHeight, 0), explosionRadius);
        foreach (Collider2D c in thingsHit) {
            if (c.gameObject.CompareTag("Player")) {
                c.gameObject.GetComponent<Player>().TakeDamage(damage);
            }
        }
    }

}
