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

	public float flashTime = 0.7f;
	public float maxFlashAmount = 0.5f;
	private float flashEndTime;
	private int flashPropID;

	public SpriteRenderer[] renderers;
	private new Collider2D collider;
	
	Vector2 startPos;
	float moveEndTime;
    // Use this for initialization
	void Start () {
		startPos = transform.position;
		if (renderers == null || renderers.Length == 0) renderers = GetComponentsInChildren<SpriteRenderer>();
		collider = GetComponent<Collider2D>();
		flashEndTime = float.NegativeInfinity;
		flashPropID = Shader.PropertyToID("_FlashAmount");
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

		if (flashEndTime > float.NegativeInfinity) {

			MaterialPropertyBlock block = new MaterialPropertyBlock();

			if (flashEndTime > Time.time) {
				float flashAmount = (flashEndTime - Time.time) / flashTime * maxFlashAmount;

				foreach (SpriteRenderer r in renderers) {
					r.GetPropertyBlock(block);
					block.SetFloat(flashPropID, flashAmount);
					r.SetPropertyBlock(block);
				}
			} else {
				foreach (SpriteRenderer r in renderers) {
					r.GetPropertyBlock(block);
					block.SetFloat(flashPropID, 0);
					r.SetPropertyBlock(block);
				}

				flashEndTime = float.NegativeInfinity;
			}
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
        Destroy(Instantiate(explosion, landingZone.transform).gameObject, 4);

        List<Collider2D> thingsHit = new List<Collider2D>(
			Physics2D.OverlapCircleAll(landingZone.transform.position + new Vector3(0, explosionHeight, 0), explosionRadius));
		Vector2 touchingRange = new Vector2(0.05f, 0.05f);
		thingsHit.AddRange(
			Physics2D.OverlapAreaAll((Vector2)collider.bounds.min - touchingRange, (Vector2)collider.bounds.max + touchingRange));

        foreach (Collider2D c in thingsHit) {
            if (c.gameObject.CompareTag("Player")) {
                c.gameObject.GetComponent<Player>().TakeDamage(damage);
            }
        }

		flashEndTime = Time.time + flashTime;
    }

}
