using System;
using UnityEngine;

public delegate void LearnSkill();

public abstract class BossBase : MonoBehaviour, IDamageable, IKillable {

	public static int highestKilled = 0;
	public static readonly LearnSkill[] learnSkills = {
		Player.LearnClaws, // Boss 1
		Player.LearnWings, // Boss 2
		Player.LearnTime,  // Boss 3
	};
	public abstract int BossOrderID { get; }

	public float maxHealth = 140;
    public float CurrHealth { get; private set; }
	public UIAttributeBar healthBar;

    public AudioSource audioSource;
    public AudioClip deathSound;
    public AudioClip hurtSound;

    private void Start() {
        healthBar.MaxAmount = maxHealth;
        CurrHealth = maxHealth;
        healthBar.Amount = CurrHealth;
		if (highestKilled < BossOrderID) {
			StartAlive();
		} else {
			StartDead();
		}
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
		CurrHealth = 0;
		healthBar.Amount = CurrHealth;
		int id = BossOrderID;
		if (id > 0 && id - 1 < learnSkills.Length) {
			learnSkills[id - 1]();
		}
		if (id > highestKilled) {
			highestKilled = id;
		}
        audioSource.clip = deathSound;
        audioSource.Play();
        OnKilled();
	}

	public virtual void OnKilled() {}
}
