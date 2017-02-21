using System;
using UnityEngine;

public class HighlightedActivatable : MonoBehaviour {

	public float highlightDistance = 3f;
	public Animator highlightAnimator;

	private static int animationShowId = Animator.StringToHash("Shown");
	
	public bool Highlighted {
		get { return highlightAnimator.GetBool(animationShowId); }
		set { highlightAnimator.SetBool(animationShowId, value); }
	}

	public void Activate() {
		throw new NotImplementedException();
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		float sqrHighlightDist = highlightDistance * highlightDistance;
		Vector3 distToPlayer = EvoCharacter.inst.transform.position - transform.position;
		Highlighted = distToPlayer.sqrMagnitude < sqrHighlightDist;
		Debug.DrawRay(transform.position, distToPlayer.normalized * highlightDistance, Color.white);
	}
}
