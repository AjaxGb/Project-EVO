using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utilities {

	public static void DebugDrawRect(Rect rect, Color color) {
		Vector2 topL = new Vector2(rect.xMin, rect.yMin);
		Vector2 botL = new Vector2(rect.xMin, rect.yMax);
		Vector2 topR = new Vector2(rect.xMax, rect.yMin);
		Vector2 botR = new Vector2(rect.xMax, rect.yMax);

		Debug.DrawLine(topL, topR, color);
		Debug.DrawLine(topL, botL, color);
		Debug.DrawLine(topR, botR, color);
		Debug.DrawLine(botL, botR, color);
	}

	public static int RandomWeighted(float[] weights) {
		float randTarget = UnityEngine.Random.Range(0f, weights.Sum());
		float total = 0;
		int result;
		for (result = 0; result < weights.Length; result++) {
			total += weights[result];
			if (total >= randTarget) break;
		}
		return result;
	}

	public static Rect TransformRect(this Vector3 origin, Rect rect) {
		return TransformRect((Vector2)origin, rect);
	}
	public static Rect TransformRect(this Vector2 origin, Rect rect) {
		rect.position += origin;
		return rect;
	}

	public static Rect InverseTransformRect(this Vector3 origin, Rect rect) {
		return InverseTransformRect((Vector2)origin, rect);
	}
	public static Rect InverseTransformRect(this Vector2 origin, Rect rect) {
		rect.position -= origin;
		return rect;
	}

	public static Rect ToRect(this Bounds bounds) {
		return new Rect(bounds.min, bounds.size);
	}

	public static Bounds ToBounds(this Rect rect) {
		return new Bounds(rect.center, rect.size);
	}

	public static void AddSorted<T>(this List<T> list, T item) where T : IComparable<T> {
		if (list.Count == 0 || list[list.Count - 1].CompareTo(item) <= 0) {
			list.Add(item);
			return;
		}
		if (list[0].CompareTo(item) >= 0) {
			list.Insert(0, item);
			return;
		}
		int index = list.BinarySearch(item);
		if (index < 0) index = ~index;
		list.Insert(index, item);
	}

	public static bool RemoveSorted<T>(this List<T> list, T item) where T : IComparable<T> {
		int index = list.BinarySearch(item);
		if (index >= 0) {
			list.RemoveAt(index);
			return true;
		}
		return false;
	}

	public static bool ContainsSorted<T>(this List<T> list, T item) where T : IComparable<T> {
		return list.BinarySearch(item) >= 0;
	}

	public static bool IsChildOf(this GameObject child, GameObject parent)
    {
		return child.transform.IsChildOf(parent.transform);
    }
	
	public static string Readable(this TimeSpan ts) {
		float seconds = (float)ts.TotalSeconds;

		if (seconds < 60)
			return Quantify("second", ts.Seconds);
		else if (seconds < 60 * 60)
			return Quantify("minute", ts.Minutes);
		else if (seconds < 60 * 60 * 24)
			return Quantify("hour", ts.Hours);
		else if (seconds < 60 * 60 * 24 * 7)
			return Quantify("day", ts.Days);
		else
			return Quantify("week", ts.Days / 7);
	}

	public static string Quantify(string thing, int num) {
		if (num == 1) {
			return string.Format("1 {0}", thing);
		}
		return string.Format("{0} {1}s", num, thing);
	}
}
