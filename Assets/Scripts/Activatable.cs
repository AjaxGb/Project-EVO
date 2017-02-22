using System;
using System.Collections.Generic;
using UnityEngine;

public class Activatable : MonoBehaviour {

	public float highlightDistance = 3f;
	public Animator highlightAnimator;

	private static int animationShowId = Animator.StringToHash("Shown");
	public static List<Activatable> allInScene = new List<Activatable>();
	
	public bool Highlighted {
		get { return highlightAnimator.GetBool(animationShowId); }
		set { highlightAnimator.SetBool(animationShowId, value); }
	}

	public void Activate() {
		throw new NotImplementedException();
	}

	// Use this for initialization
	void Start() {
		allInScene.Add(this);
	}

	// Cleanup when destroyed
	void OnDestroy() {
		allInScene.Remove(this);
	}
}
