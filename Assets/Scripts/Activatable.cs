using System.Collections.Generic;
using UnityEngine;

public abstract class Activatable : MonoBehaviour {

	public float highlightDistance = 3f;
	public Animator highlightAnimator;

    public AudioSource audioSource; //Audio Source to play sounds on activatables
    public AudioClip previewSound; //Sound to play when the object is highlighted
    public AudioClip deactivateSound; //Sound to play when object stops being highlighted

    protected static int animationShowHighlight = Animator.StringToHash("Show Highlight");

	public static List<Activatable> allInScene = new List<Activatable>();

	public bool Highlighted {
		get { return highlightAnimator.GetBool(animationShowHighlight); }
		set {
            if (value != highlightAnimator.GetBool(animationShowHighlight)) {
                highlightAnimator.SetBool(animationShowHighlight, value);
                audioSource.clip = value ? previewSound : deactivateSound; //Use the sound appropriate to whether the object is highlighted or un-highlighted on this change in value
                audioSource.Play(); //Play the clip
            }
        }
	}

	public abstract bool CanActivate { get; }

	public abstract void Activate(Player p);

	// Use this for initialization
	protected virtual void Start() {
		allInScene.Add(this);
	}

	// Cleanup when destroyed
	protected virtual void OnDestroy() {
		allInScene.Remove(this);
	}
}
