using UnityEngine;
using UnityEngine.UI;

public class ActivatableText : Activatable {

	[System.Serializable]
	public class TextEntry {
		public string text = "Some text.";
		public float duration = 3f;
	}

	public float deactivateDistance = 7f;
	public TextEntry[] script;

	private Text text;
	private Player currWatchingPlayer;
	private float timeToGo;
	private int _currTextEntry;
	public int CurrTextEntry {
		get { return _currTextEntry; }
		set {
			_currTextEntry = value;
			TextEntry te = script[value];
			text.text = te.text;
			timeToGo = te.duration;
		}
	}
    
    public AudioClip readSound;

	protected static int animationShowText = Animator.StringToHash("Show Text");

	public bool ShowingText {
		get { return highlightAnimator.GetBool(animationShowText); }
		set {
			highlightAnimator.SetBool(animationShowText, value);
			if (!value) currWatchingPlayer = null;
		}
	}

	public override bool CanActivate { get { return true; } }

	public override void Activate(Player p) {
		if (!ShowingText) {
			CurrTextEntry = 0;
			ShowingText = true;
			currWatchingPlayer = p;
		} else {
			NextTextEntry();
		}
	}

	public void NextTextEntry() {
		if (CurrTextEntry < script.Length - 1) {
			++CurrTextEntry;
            audioSource.clip = readSound;
            audioSource.Play();
        } else {
			ShowingText = false;
		}
	}

	protected override void Start() {
		base.Start();
		text = GetComponentInChildren<Text>();
	}

	void Update() {
		if (currWatchingPlayer == null || Time.timeScale == 0) return;
		
		Vector2 distToPlayer = (Vector2)currWatchingPlayer.transform.position - (Vector2)transform.position;
		if (distToPlayer.sqrMagnitude > deactivateDistance * deactivateDistance) {
			ShowingText = false;
			return;
		}

		timeToGo -= Time.deltaTime;
		if (timeToGo <= 0) {
			NextTextEntry();
        }
	}
}
