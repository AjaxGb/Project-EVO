using System.Collections.Generic;
using UnityEngine;

public static class Utilities {

	public static void DebugDrawRect(Rect rect, Color color) {
		Vector2 topL = new Vector2(rect.xMin, rect.yMin);
		Vector2 botL = new Vector2(rect.xMin, rect.yMax);
		Vector2 topR = new Vector2(rect.xMax, rect.yMin);
		Vector2 botR = new Vector2(rect.xMax, rect.yMin);

		Debug.DrawLine(topL, topR, color);
		Debug.DrawLine(topL, botL, color);
		Debug.DrawLine(topR, botR, color);
		Debug.DrawLine(botL, botR, color);
	}
}
