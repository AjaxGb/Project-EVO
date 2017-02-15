using UnityEngine;

public class UIAttributeBarTest : MonoBehaviour {

	public UIAttributeBar[] bars;
	private bool[] increasing;

	private void Start() {
		increasing = new bool[bars.Length];
	} 
	
	private void Update() {
		for (int i = 0; i < bars.Length; i++) {
			UIAttributeBar bar = bars[i];
			bar.Amount += (Random.value - 0.2f) * (increasing[i] ? 2 : -2);
			if (bar.Amount == bar.MinAmount) {
				increasing[i] = true;
			} else if (bar.Amount == bar.MaxAmount) {
				increasing[i] = false;
			}
		}
	}
}
