using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingDoor : MonoBehaviour {

	[SerializeField]
	private bool _opened;
	public bool IsOpened {
		get { return _opened; }
		set {
			if (_opened != value) {
				_opened = value;
				timeLeft = movementDuration;
			}
		}
	}
	
	public float movementDuration = 3f; // Time for the door to fully move;
	public float timeLeft = 0f;

	public Vector2 closedPos;
	public Vector2 openPos;
	
	// Update is called once per frame
	void FixedUpdate () {
		if (timeLeft > 0f) {
			timeLeft -= Time.deltaTime;
			float t = timeLeft / movementDuration;
			if (_opened) t = 1 - t;
			transform.localPosition = Vector2.Lerp(closedPos, openPos, t);
			if (timeLeft <= 0f) {
				timeLeft = 0f;
			}
		}
	}

	public void SkipAnimation() {
		timeLeft = 0f;
		transform.localPosition = _opened ? openPos : closedPos;
	}
}
