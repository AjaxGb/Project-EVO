using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

	public const int buildIndex = 0;

	enum MenuState {
		MAIN = 0,
		LOAD = 1,
		NAME_GAME = 2,
		CREDITS = 3,
	}

	public Animator menuAnimator;
	private int menuStateID;
	
	public SceneInfo newGameScene;
	public Vector2 newGamePosition;

	public InputField nameGame;

	public RectTransform saveGameArea;
	public SavedGameMenuItem saveGameMenuPrefab;

	// Use this for initialization
	void Start () {
		menuStateID = Animator.StringToHash("MenuState");
		
		foreach (SaveState state in SaveManager.inst) {
			Instantiate(saveGameMenuPrefab, saveGameArea).RepresentedState = state;
		}
	}

	public void StartNewGame() {
		SaveState state = new SaveState(nameGame.text, newGameScene.buildIndex, newGamePosition, 0);
		SaveManager.inst.AddSave(state);
		SaveManager.inst.LoadState(state);
	}

	public void OpenMainMenu() {
		menuAnimator.SetInteger(menuStateID, (int)MenuState.MAIN);
	}

	public void OpenGameLoadingMenu() {
		menuAnimator.SetInteger(menuStateID, (int)MenuState.LOAD);
	}

	public void OpenGameNamingMenu() {
		menuAnimator.SetInteger(menuStateID, (int)MenuState.NAME_GAME);
	}

	public void OpenCreditsMenu() {
		menuAnimator.SetInteger(menuStateID, (int)MenuState.CREDITS);
	}

	public void QuitGame() {
		Application.Quit();
	}
}
