using System;
using System.Collections.Generic;
using UnityEngine;

public class Activatable : MonoBehaviour {

	public float highlightDistance = 3f;
	public Animator highlightAnimator;
	public float deactivateDistance = 5f;

	private Player currWatchingPlayer;

	private static int animationShowHighlight = Animator.StringToHash("Show Highlight");
	private static int animationShowText = Animator.StringToHash("Show Text");

	public static List<Activatable> allInScene = new List<Activatable>();

	public bool Highlighted {
		get { return highlightAnimator.GetBool(animationShowHighlight); }
		set { highlightAnimator.SetBool(animationShowHighlight, value); }
	}

	public bool ShowingText {
		get { return highlightAnimator.GetBool(animationShowText); }
		set { highlightAnimator.SetBool(animationShowText, value); }
	}

	public bool CanActivate { get { return !ShowingText; } }

	public void Activate(Player p) {
		ShowingText = true;
		currWatchingPlayer = p;
	}

	// Use this for initialization
	void Start() {
		allInScene.Add(this);
	}

	void Update() {
		if (currWatchingPlayer == null || Time.timeScale == 0) return;
		Vector2 distToPlayer = (Vector2)currWatchingPlayer.transform.position - (Vector2)transform.position;
		if (distToPlayer.sqrMagnitude > deactivateDistance * deactivateDistance) {
			ShowingText = false;
			currWatchingPlayer = null;
		}
	}

	// Cleanup when destroyed
	void OnDestroy() {
		allInScene.Remove(this);
	}
}
