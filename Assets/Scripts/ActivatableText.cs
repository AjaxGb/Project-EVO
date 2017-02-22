using UnityEngine;

public class ActivatableText : Activatable {

	public float deactivateDistance = 5f;

	private Player currWatchingPlayer;

	protected static int animationShowText = Animator.StringToHash("Show Text");

	public bool ShowingText {
		get { return highlightAnimator.GetBool(animationShowText); }
		set { highlightAnimator.SetBool(animationShowText, value); }
	}

	public override bool CanActivate { get { return !ShowingText; } }

	public override void Activate(Player p) {
		ShowingText = true;
		currWatchingPlayer = p;
	}

	void Update() {
		if (currWatchingPlayer == null || Time.timeScale == 0) return;
		Vector2 distToPlayer = (Vector2)currWatchingPlayer.transform.position - (Vector2)transform.position;
		if (distToPlayer.sqrMagnitude > deactivateDistance * deactivateDistance) {
			ShowingText = false;
			currWatchingPlayer = null;
		}
	}
}
