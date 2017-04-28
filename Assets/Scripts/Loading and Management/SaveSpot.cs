using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSpot : Activatable {

	[Serializable]
	public struct Star {
		public Transform star;
		public float rotationSpeed;
	}
	public Star[] stars;
	public ParticleSystem particles;

	public CanvasRenderer text;
	public AnimationCurve textFadeCurve;
	public float textFadeTime = 3f;
	private float currFadeTimer;
	
	public override bool CanActivate { get { return true; } }

	public override void Activate(Player p) {
		p.PlayerHealth = p.MaxHealth;
		p.PlayerMana = p.MaxMana;
		SaveManager.inst.UpdateSave(transform.position);
		particles.Play();
		currFadeTimer = textFadeTime;
		text.SetAlpha(1);
	}

	protected override void Start() {
		base.Start();
		text.SetAlpha(0);
	}

	private void Update() {
		if (Highlighted) {
			foreach (Star s in stars) {
				s.star.rotation = s.star.rotation * Quaternion.AngleAxis(Time.deltaTime * s.rotationSpeed, Vector3.forward);
			}
		}
		if (currFadeTimer > 0) {
			currFadeTimer -= Time.deltaTime;
			if (currFadeTimer <= 0) {
				currFadeTimer = 0;
				text.SetAlpha(0);
			} else {
				text.SetAlpha(textFadeCurve.Evaluate(currFadeTimer));
			}
		}
	}
}
