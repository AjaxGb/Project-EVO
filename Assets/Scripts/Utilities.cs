using System;
using System.Collections.Generic;
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
        for (Transform curr = child.transform; curr != null; curr = curr.transform.parent)
        {
            if (curr == parent.transform) return true;
        }
        return false;
    }
}
