
public class ControlsFake : IControls {
	
	public void Update() {
		//TODO
	}

	public bool GetButtonDown(ButtonId id) {
		return false;
	}

	public bool GetButton(ButtonId id) {
		return false;
	}

	public bool GetButtonUp(ButtonId id) {
		return false;
	}

	public float GetAxis(AxisId id) {
		return 0;
	}

	public void ClearUpDown() {
		// TODO
	}
}
