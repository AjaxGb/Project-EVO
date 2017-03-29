using UnityEngine;

public class ControlsReal : IControls {

	public readonly string[] buttonMappings = new string[(int)ButtonId.length] { "Jump", "Glide" };
	public readonly string[] axisMappings = new string[(int)AxisId.length] { "Horizontal", "Vertical" };

	private struct ButtonState {
		public bool down;
		public bool up;
	}
	// We store button up/down for later; this keeps us from missing button presses if
	// multiple updates happen between fixedupdates.
	private ButtonState[] buttonStates = new ButtonState[(int)ButtonId.length];

	public void Update() {
		for (int i = buttonStates.Length; i >= 0; --i) {
			buttonStates[i].down = buttonStates[i].down || Input.GetButtonDown(buttonMappings[i]);
			buttonStates[i].up   = buttonStates[i].up   || Input.GetButtonUp(buttonMappings[i]);
		}
	}

	public bool GetButtonDown(ButtonId id) {
		return buttonStates[(int)id].down;
	}

	public bool GetButton(ButtonId id) {
		return Input.GetButton(buttonMappings[(int)id]);
	}

	public bool GetButtonUp(ButtonId id) {
		return buttonStates[(int)id].up;
	}

	public float GetAxis(AxisId id) {
		return Input.GetAxis(axisMappings[(int)id]);
	}

	public void ClearUpDown() {
		for (int i = buttonStates.Length; i >= 0; --i) {
			buttonStates[i].down = buttonStates[i].up = false;
		}
	}
}
