using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Thought {
	public string text;
	public float duration;
	public Thought(string t, float d) {
		text = t;
		duration = d;
	}
}

public class ThoughtManager : MonoBehaviour {
	public static ThoughtManager Inst { get; private set; }

	enum State { BLANK, FADE_IN, STAY, FADE_OUT };

	public float defaultDuration = 2f;
	public float fadeInTime = 0.1f;
	public float fadeOutTime = 1f;

	private State state;
	private float endStateTime;
	private Thought currThought;
	private Queue<Thought> waitingThoughts = new Queue<Thought>();
	private Text text;

	// Use this for initialization
	void Start () {
		Inst = this;
		state = State.BLANK;
		text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		switch (state) {
			case State.BLANK:
				if (waitingThoughts.Count > 0) {
					currThought = waitingThoughts.Dequeue();
					text.text = currThought.text;
					text.color = new Color(1, 1, 1, 0);
					state = State.FADE_IN;
					endStateTime = Time.time + fadeInTime;
				}
				break;
			case State.FADE_IN:
				float inP = (endStateTime - Time.time) / fadeInTime;
				if (inP <= 0) {
					text.color = Color.white;
					state = State.STAY;
					endStateTime = Time.time + currThought.duration;
				} else {
					text.color = new Color(1, 1, 1, 1 - inP);
				}
				break;
			case State.STAY:
				if (endStateTime <= Time.time) {
					state = State.FADE_OUT;
					endStateTime = Time.time + fadeOutTime;
				}
				break;
			case State.FADE_OUT:
				float outP = (endStateTime - Time.time) / fadeInTime;
				if (outP <= 0) {
					text.color = new Color(1, 1, 1, 0);
					state = State.BLANK;
				} else {
					text.color = new Color(1, 1, 1, outP);
				}
				break;
		}
	}

	public void AddThought(Thought t) {
		waitingThoughts.Enqueue(t);
	}

	public void AddThought(string text, float duration) {
		AddThought(new Thought(text, duration));
	}

	public void AddThought(string text) {
		AddThought(text, defaultDuration);
	}
}
