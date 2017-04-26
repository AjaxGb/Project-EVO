using UnityEngine;

public class BreakableFloor : MonoBehaviour {

	public GameObject brokenPrefab;
	public GameObject prebrokenPrefab;
	public float despawnTime = 3f;

	public SceneInfo startBrokenIfSavedIn;
	public int startBrokenIfKilledBoss;

	public void Start() {
		if (startBrokenIfKilledBoss <= BossBase.highestKilled
			|| SaveManager.inst.currentSave.currSceneBI == startBrokenIfSavedIn.buildIndex)
		{
			Destroy(this.gameObject);
			Instantiate(prebrokenPrefab, transform.position, transform.rotation, transform.parent);
		}
	}

	public void OnCollisionEnter2D(Collision2D col) {
		FallingRock rock = col.gameObject.GetComponent<FallingRock>();
		if (rock) {
			Destroy(this.gameObject);
			Destroy(
				Instantiate(brokenPrefab, transform.position, transform.rotation, transform.parent),
				despawnTime);
		}
	}
}
