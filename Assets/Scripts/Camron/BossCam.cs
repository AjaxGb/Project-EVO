using UnityEngine;

public class BossCam : MonoBehaviour {
	
	public Transform playerTransform;

	public float zoom = 2f; // Zoom in/out by this amount
    public float sensivity = 0.2f;

	// Use this for initialization
	void Start () {
		if (playerTransform == null && SceneLoader.inst != null) playerTransform = SceneLoader.inst.player.transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeScale == 0 || !SceneLoader.IsInCurrentScene(gameObject)) return;
		CameraFollow.inst.SetFixedAngle(
			transform.position + (playerTransform.position - transform.position) * sensivity,
			zoom);
	}
}
