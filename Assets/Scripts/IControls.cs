
public enum ButtonId {
	JUMP,
	GLIDE,
	ATTACK,
	length
}

public enum AxisId {
	HORIZONTAL,
	VERTICAL,
	length
}

public interface IControls {
	bool GetButtonDown(ButtonId id);
	bool GetButton(ButtonId id);
	bool GetButtonUp(ButtonId id);

	float GetAxis(AxisId id);
}
