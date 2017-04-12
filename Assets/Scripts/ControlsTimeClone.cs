using System;
using System.Collections.Generic;
using UnityEngine;

public struct ButtonState {
	public bool down;
	public bool held;
	public bool up;
}

public struct InputFrame {
	public float time;
	public ButtonState[] buttons;
	public float[] axes;
}

public class ControlsTimeClone : IControls {

	private IList<InputFrame> recorded;
	private int currFrame = 0;
	private float startTime;
	private float lastUpdateTime;
	public bool IsDone { get; private set; }

	public ControlsTimeClone(IList<InputFrame> recorded) {
		this.recorded = recorded;
		if (recorded.Count == 0) {
			IsDone = true;
		} else {
			IsDone = false;
			// Ensure that timings in the recording are relative to
			// the beginning of the recording.
			float recordStartTime = recorded[0].time;
			for (int i = recorded.Count; i >= 0; --i) {
				var temp = recorded[i];
				temp.time -= recordStartTime;
				recorded[i] = temp;
			}
		}
	}

	public void Start() {
		startTime = Time.time;
	}

	public void Update() {
		float currTime = Time.time - startTime;
		while (currTime < recorded[currFrame].time) {
			currFrame++;
		}
	}

	public bool GetButtonDown(ButtonId id) {
		Update();
		return recorded[currFrame].buttons[(int)id].down;
	}

	public bool GetButton(ButtonId id) {
		Update();
		return recorded[currFrame].buttons[(int)id].held;
	}

	public bool GetButtonUp(ButtonId id) {
		Update();
		return recorded[currFrame].buttons[(int)id].up;
	}

	public float GetAxis(AxisId id) {
		Update();
		return recorded[currFrame].axes[(int)id];
	}
}
