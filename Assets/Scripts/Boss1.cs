using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1 : MonoBehaviour, IKillable, IDamageable {

	public float health = 140;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public float TakeDamage(float amount) {
		if (amount >= health) {
			amount = health;
			health = 0;
			Kill();
			return amount;
		}
		health -= amount;
		return amount;
	}

	public void Kill() {
		Destroy(transform.gameObject);
	}
}
