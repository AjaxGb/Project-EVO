using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePlayer : Player {

	public override bool isRealPlayer { get { return false; } }

	public override void OnDeath() {
		DeathTime = 3.0f; // Wait 3 seconds before respawn
		animator.SetBool("isidle", true);
		body.freezeRotation = false;
		body.gravityScale = gravityScale;
		collider.sharedMaterial = deathPhysMaterial;
		collider.enabled = false;
		collider.enabled = true;
	}

	protected override void OnDamaged(float d) {
		// TODO: Make player tint red, instead of screen.
	}

	protected override void DeadUpdate() {
		// TODO: Make player fade away
		//renderer.color = new Color(1, 1, 1);
	}

	public override void OnRespawn() {
		Destroy(this.gameObject);
	}
}
