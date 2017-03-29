using System;
using UnityEngine;

public class ControlsReal : IControls {

	public readonly string[] buttonMappings = new string[(int)ButtonId.length] { "Jump", "Glide", "Attack" };
	public readonly string[] axisMappings = new string[(int)AxisId.length] { "Horizontal", "Vertical" };

	public bool GetButtonDown(ButtonId id) {
		return Input.GetButtonDown(buttonMappings[(int)id]);
	}

	public bool GetButton(ButtonId id) {
		return Input.GetButton(buttonMappings[(int)id]);
	}

	public bool GetButtonUp(ButtonId id) {
		return Input.GetButtonDown(buttonMappings[(int)id]);
	}

	public float GetAxis(AxisId id) {
		return Input.GetAxis(axisMappings[(int)id]);
	}
}
