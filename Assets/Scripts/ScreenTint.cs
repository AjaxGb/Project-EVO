using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenTint : MonoBehaviour {
	public static ScreenTint inst { get; private set; }

	public Graphic tinterPrefab;
	
	private Dictionary<ulong, Graphic> tints = new Dictionary<ulong, Graphic>();
	private class Fade {
		public Fade(Color start, Color end, float duration, bool pingpong, bool destroy) {
			this.start = start;
			this.end = end;
			this.progress = 0;
			this.duration = duration;
			this.pingpong = pingpong;
			this.destroy = destroy;
		}
		public Color start;
		public Color end;
		public float progress;
		public float duration;
		public bool pingpong;
		public bool destroy;
	}
	private Dictionary<ulong, Fade> fades = new Dictionary<ulong, Fade>();
	
	ulong lastID = 0;
	private ulong NextID() {
		return ++lastID;
	}

	private void Start() {
		inst = this;
	}

	private Graphic MakeTintGraphic(Color color) {
		Graphic g = Instantiate(tinterPrefab, this.transform);
		g.rectTransform.offsetMax = Vector2.zero;
		g.rectTransform.offsetMin = Vector2.zero;
		g.color = color;
		g.canvasRenderer.SetAlpha(color.a);
		return g;
	}

	public ulong AddTint(Color color) {
		ulong id = NextID();
		tints.Add(id, MakeTintGraphic(color));
		return id;
	}

	public void SetTint(ulong id, Color color) {
		Graphic g;
		if (tints.TryGetValue(id, out g)) {
			g.color = color;
		} else {
			throw new ArgumentException("Tint ID not in use", "id");
		}
	}

	public ulong StartFade(Color start, Color end, float time, bool pingPong = false, bool removeWhenDone = false) {
		ulong id = NextID();
		Graphic g = MakeTintGraphic(start);
		tints.Add(id, g);
		fades.Add(id, new Fade(start, end, pingPong ? time / 2 : time, pingPong, removeWhenDone));
		return id;
	}

	public ulong StartFade(float startAlpha, Color end, float time, bool pingPong = false, bool removeWhenDone = false) {
		Color start = end;
		start.a = startAlpha;
		return StartFade(start, end, pingPong ? time / 2 : time, pingPong, removeWhenDone);
	}

	public ulong StartFade(Color start, float endAlpha, float time, bool pingPong = false, bool removeWhenDone = false) {
		Color end = start;
		end.a = endAlpha;
		return StartFade(start, end, pingPong ? time / 2 : time, pingPong, removeWhenDone);
	}

	public void FadeTint(ulong id, Color target, float time, bool pingPong = false, bool removeWhenDone = false) {
		Graphic g;
		if (tints.TryGetValue(id, out g)) {
			fades[id] = new Fade(g.color, target, pingPong ? time / 2 : time, pingPong, removeWhenDone);
		} else {
			throw new ArgumentException("Tint ID not in use", "id");
		}
	}

	public void FadeTint(ulong id, float targetAlpha, float time, bool pingPong = false, bool removeWhenDone = false) {
		Graphic g;
		if (tints.TryGetValue(id, out g)) {
			Color target = g.color;
			target.a = targetAlpha;
			fades[id] = new Fade(g.color, target, pingPong ? time / 2 : time, pingPong, removeWhenDone);
		} else {
			throw new ArgumentException("Tint ID not in use", "id");
		}
	}

	public bool StopFade(ulong id) {
		return fades.Remove(id);
	}

	private bool RemoveTintNotFade(ulong id) {
		Graphic g;
		bool found = tints.TryGetValue(id, out g);
		if (found) {
			Destroy(g.gameObject);
			tints.Remove(id);
		}
		return found;
	}

	public bool RemoveTint(ulong id) {
		fades.Remove(id);
		return RemoveTintNotFade(id);
	}

	public void RemoveAllTints() {
		fades.Clear();
		foreach (Graphic g in tints.Values) {
			Destroy(g.gameObject);
		}
		tints.Clear();
	}

	private List<ulong> endedFades = new List<ulong>();
	// Update is called once per frame
	private void Update() {
		foreach (var kvp in fades) {
			ulong id = kvp.Key;
			Fade f = kvp.Value;
			Graphic g = tints[id];

			// Unity will "forget" to update color if it started with alpha == 0.
			bool stopUnityBeingStupid = g.color.a == 0;

			f.progress += Time.deltaTime;
			if (f.progress >= f.duration) {
				if (f.pingpong) {
					g.color = f.end;
					Color temp = f.start;
					f.start = f.end;
					f.end = temp;
					f.progress = 0;
					f.pingpong = false;
					continue;
				}
				if (f.destroy) {
					RemoveTintNotFade(id);
				} else {
					g.color = f.end;
				}
				endedFades.Add(id);
			} else {
				float t = f.progress / f.duration;
				g.color = Color.Lerp(f.start, f.end, t);
				if (stopUnityBeingStupid) {
					g.enabled = false;
					g.enabled = true;
				}
			}
		}
		foreach (ulong id in endedFades) {
			fades.Remove(id);
		}
		endedFades.Clear();
	}
}
