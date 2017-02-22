using System.Collections.Generic;
using UnityEngine;

public abstract class Activatable : MonoBehaviour {

	public float highlightDistance = 3f;
	public Animator highlightAnimator;

	protected static int animationShowHighlight = Animator.StringToHash("Show Highlight");

	public static List<Activatable> allInScene = new List<Activatable>();

	public bool Highlighted {
		get { return highlightAnimator.GetBool(animationShowHighlight); }
		set { highlightAnimator.SetBool(animationShowHighlight, value); }
	}

	public abstract bool CanActivate { get; }

	public abstract void Activate(Player p);

	// Use this for initialization
	protected virtual void Start() {
		allInScene.Add(this);
	}

	// Cleanup when destroyed
	protected virtual void OnDestroy() {
		allInScene.Remove(this);
	}
}
