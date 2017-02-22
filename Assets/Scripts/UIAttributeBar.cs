using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIAttributeBar : MonoBehaviour {

	private Text label;
	private Image[] images;
	private Image fill;

	private float maxFillWidth;
	public float BarWidth {
		get { return maxFillWidth; }
		set {
			float delta = value - maxFillWidth;
			maxFillWidth = value;
			foreach (Image i in images) {
				i.rectTransform.sizeDelta += new Vector2(delta, 0);
			}
			UpdateFillWidth();
		}
	}

	[Tooltip("{0} is current amount, {1} and {2} are min/max amounts.\nUses C# string formatting codes.")]
	public string labelFormat = "{0:F0}/{2:F0}";

	[SerializeField]
	private float minAmount = 0;
	public float MinAmount {
		get { return minAmount; }
		set { SetBounds(value, maxAmount); }
	}

	[SerializeField]
	private float maxAmount = 100;
	public float MaxAmount {
		get { return maxAmount; }
		set { SetBounds(minAmount, value); }
	}

	[SerializeField]
	private float amount = 100;
	public float Amount {
		get { return amount; }
		set {
			amount = Mathf.Clamp(value, minAmount, maxAmount);
			UpdateFillWidth();
			UpdateLabel();
		}
	}

	void Start () {
		label = GetComponentInChildren<Text>();
		images = GetComponentsInChildren<Image>();
		fill = images.Where(i => i.name == "Fill").First();

		maxFillWidth = fill.rectTransform.sizeDelta.x;
		SetBounds(minAmount, maxAmount);
		UpdateLabel();
	}

	public void SetBounds(float min, float max) {
		if (min > max) {
			throw new ArgumentException("Cannot set min bound (" + min + ") larger than max bound (" + max + ")!");
		}
		minAmount = min;
		maxAmount = max;

		amount = Mathf.Clamp(amount, min, max);
		UpdateFillWidth();
	}
	
	private void UpdateFillWidth() {
		if (fill == null) return;
		Vector2 fillSize = fill.rectTransform.sizeDelta;
		fillSize.x = Mathf.InverseLerp(minAmount, maxAmount, amount) * maxFillWidth;
		fill.rectTransform.sizeDelta = fillSize;
	}

	private void UpdateLabel() {
		if (label == null) return;
		label.text = string.Format(labelFormat, amount, minAmount, maxAmount);
	}
}
