using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Menu")) {
			SaveManager.inst.UpdateSave(SceneLoader.inst.player.transform.position);
			SceneManager.LoadScene(MainMenuManager.buildIndex);
		}
	}
}
