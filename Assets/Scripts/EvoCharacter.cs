using UnityEngine;

/*
 *  Exactly one of these should be placed in every level, at the beginning.
 */
public class EvoCharacter : MonoBehaviour {
	
	public static EvoCharacter inst { get; private set; }

	struct PersistVar {
		public float maxHealth;
		public float currHealth;
	}
	private PersistVar persist;

	// Allow outside access (get only) of persistent variables
	public float MaxHealth { get { return persist.maxHealth; } }
	public float CurrHealth { get { return persist.currHealth; } }

	void Start () {
		if (inst == null) {
			// Set up persistent variables at start of game
			persist.maxHealth = 20f;
			persist.currHealth = persist.maxHealth;
		} else {
			// Copy persistent variables over from last scene
			persist = inst.persist;
		}
		inst = this;
	}
	
	void Update () {
	
	}
}
