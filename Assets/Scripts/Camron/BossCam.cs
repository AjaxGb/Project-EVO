using UnityEngine;

public class BossCam : MonoBehaviour {
	
	public Player player;

	public float zoom = 2f; // Zoom in/out by this amount
    public float sensivity = 0.2f;

	// Use this for initialization
	void Start () {
		if (player == null && SceneLoader.inst != null) player = SceneLoader.inst.player;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeScale == 0 || !SceneLoader.IsInCurrentScene(gameObject) || !player.IsAlive) return;
		CameraFollow.inst.SetFixedAngle(
			transform.position + (player.transform.position - transform.position) * sensivity,
			zoom);
	}
}
