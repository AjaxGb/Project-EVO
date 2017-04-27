using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour {

	public GameObject pauseMenu;
	public Text timeText;
	private string timeTextFormat;

	private bool _open;
	public bool Open {
		set {
			_open = value;
			pauseMenu.SetActive(value);
			if (value) {
				TimeSpan timeSinceSaved = TimeSpan.FromSeconds(Time.unscaledTime - SaveManager.inst.currentSave.lastSaved);
				timeText.text = timeTextFormat.Replace("<time>", timeSinceSaved.Readable());
				Time.timeScale = 0;
			} else {
				Time.timeScale = 1;
			}
		}
		get { return _open; }
	}

	// START
	private void Start() {
		timeTextFormat = timeText.text;
	}
	
	// UPDATE
	private void Update () {
		if (Input.GetButtonDown("Menu")) {
			Open = !Open;
		}
	}

	// Buttons

	public void QuitToMenu() {
		Time.timeScale = 1;
		SceneManager.LoadScene(MainMenuManager.buildIndex);
	}

	public void QuitToDesktop() {
		Application.Quit();
	}

	public void Cancel() {
		Open = false;
	}
}
