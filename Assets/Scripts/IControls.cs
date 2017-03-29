
public enum ButtonId {
	JUMP,
	GLIDE,
	length
}

public enum AxisId {
	HORIZONTAL,
	VERTICAL,
	length
}

public interface IControls {
	void Update();

	bool GetButtonDown(ButtonId id);
	bool GetButton(ButtonId id);
	bool GetButtonUp(ButtonId id);

	float GetAxis(AxisId id);

	void ClearUpDown();
}
