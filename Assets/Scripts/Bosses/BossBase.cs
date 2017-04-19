using UnityEngine;

public abstract class BossBase : MonoBehaviour, IDamageable, IKillable {

	public static int highestKilled = 0;
	public abstract int BossOrderID { get; }

	public float health = 140;
	public UIAttributeBar healthBar;

	private void Start() {
		if (highestKilled < BossOrderID) {
			StartAlive();
		} else {
			StartDead();
		}
	}

	public abstract void StartAlive();
	public abstract void StartDead();

	public float TakeDamage(float amount) {
		if (amount > health) {
			amount = health;
		}
		health -= amount;
		healthBar.Amount = health;
		OnDamaged();
		if (health <= 0) {
			Kill();
		}
		return amount;
	}

	public virtual void OnDamaged() { }

	public void Kill() {
		health = 0;
		healthBar.Amount = health;
		int id = BossOrderID;
		if (id > highestKilled) {
			highestKilled = id;
		}
		OnKilled();
	}

	public virtual void OnKilled() { }
}
