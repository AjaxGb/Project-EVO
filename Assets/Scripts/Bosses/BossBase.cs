using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class BossBase : MonoBehaviour, IDamageable, IKillable {

	public static int highestKilled = 0;
	public abstract int BossOrderID { get; }

	public float maxHealth = 140;
    public float CurrHealth { get; private set; }
	public bool IsDead { get; private set; }
	public UIAttributeBar healthBar;

    public AudioSource audioSource;
    public AudioClip deathSound;
    public AudioClip hurtSound;
	public UnityEvent deathEvent = new UnityEvent();

    private void Start() {
        healthBar.MaxAmount = maxHealth;
        CurrHealth = maxHealth;
        healthBar.Amount = CurrHealth;
		if (highestKilled < BossOrderID) {
			StartAlive();
		} else {
			StartDead();
		}
		SceneLoader.inst.player.LearnSkillForBoss(BossOrderID - 1);
	}

	public abstract void StartAlive();
	public abstract void StartDead();

	public float TakeDamage(float amount) {
		if (amount > CurrHealth) {
			amount = CurrHealth;
		}
		CurrHealth -= amount;
		healthBar.Amount = CurrHealth;
        audioSource.clip = hurtSound;
        audioSource.Play();
        OnDamaged();
		if (CurrHealth <= 0) {
			Kill();
		}
		return amount;
	}

	public virtual void OnDamaged() {}

	public void Kill() {
		if (IsDead) return;
		IsDead = true;
		CurrHealth = 0;
		healthBar.Amount = CurrHealth;
		int id = BossOrderID;
		SceneLoader.inst.player.LearnSkillForBoss(id);
		if (id > highestKilled) {
			highestKilled = id;
		}
        audioSource.clip = deathSound;
        audioSource.Play();
        OnKilled();
		deathEvent.Invoke();
	}

	public virtual void OnKilled() {}
}
