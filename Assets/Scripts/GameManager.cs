using UnityEngine;
using System.Collections;
using UnityEngine.UI; // include UI namespace so can reference UI elements
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	// static reference to game manager so can be called from other scripts directly (not just through gameobject component)
	public static GameManager gm;

	// levels to move to on victory and lose
	public string levelAfterVictory;
	public string levelAfterGameOver;

	// game performance
	public int score = 0;
	public int highscore = 0;
	public int startLives = 3;
	public int lives = 3;

	[Range(0.0f,1.0f)]
	public float slowmoFactor = 0.7f;
	[Range(0.0f,1.0f)]
	public float slowmoTime =0.2f;

	// UI elements to control
	public Text UIScore;
	public Text UIHighScore;
	public Text UILevel;
	public GameObject[] UIExtraLives;
	public GameObject UIGamePaused;

	// private variables
	GameObject _player;
	Vector3 _spawnLocation;

	// set things up here
	void Awake () {
		// setup reference to game manager
		if (gm == null)
			gm = this.GetComponent<GameManager>();

		// setup all the variables, the UI, and provide errors if things not setup properly.
		setupDefaults();
	}

	// game loop
	void Update() {
		// if ESC pressed then pause the game
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (Time.timeScale > 0f) {
				UIGamePaused.SetActive(true); // this brings up the pause UI
				Time.timeScale = 0f; // this pauses the game action
			} else {
				Time.timeScale = 1f; // this unpauses the game action (ie. back to normal)
				UIGamePaused.SetActive(false); // remove the pause UI
			}
		}
	}

	// setup all the variables, the UI, and provide errors if things not setup properly.
	void setupDefaults() {
		// setup reference to player
		if (_player == null)
			_player = GameObject.FindGameObjectWithTag("Player");
		
		if (_player==null)
			Debug.LogError("Player not found in Game Manager");
		
		// get initial _spawnLocation based on initial position of player
		_spawnLocation = _player.transform.position;

		// if levels not specified, default to current level
		if (levelAfterVictory=="") {
			Debug.LogWarning("levelAfterVictory not specified, defaulted to current level");
			//levelAfterVictory = Application.loadedLevelName;
			levelAfterVictory = SceneManager.GetActiveScene().name;
		}
		
		if (levelAfterGameOver=="") {
			Debug.LogWarning("levelAfterGameOver not specified, defaulted to current level");
			//levelAfterGameOver = Application.loadedLevelName;
			levelAfterVictory = SceneManager.GetActiveScene().name;
		}

		// friendly error messages
		if (UIScore==null)
			Debug.LogError ("Need to set UIScore on Game Manager.");
		
		if (UIHighScore==null)
			Debug.LogError ("Need to set UIHighScore on Game Manager.");
		
		if (UILevel==null)
			Debug.LogError ("Need to set UILevel on Game Manager.");
		
		if (UIGamePaused==null)
			Debug.LogError ("Need to set UIGamePaused on Game Manager.");
		
		// get stored player prefs
		refreshPlayerState();

		// get the UI ready for the game
		refreshGUI();
	}

	// get stored Player Prefs if they exist, otherwise go with defaults set on gameObject
	void refreshPlayerState() {
		lives = PlayerPrefManager.GetLives();

		// special case if lives <= 0 then must be testing in editor, so reset the player prefs
		if (lives <= 0) {
			PlayerPrefManager.ResetPlayerState(startLives,false);
			lives = PlayerPrefManager.GetLives();
		}
		score = PlayerPrefManager.GetScore();
		highscore = PlayerPrefManager.GetHighscore();

		// save that this level has been accessed so the MainMenu can enable it
		PlayerPrefManager.UnlockLevel();
	}

	// refresh all the GUI elements
	void refreshGUI() {
		// set the text elements of the UI
		UIScore.text = "Score: "+score.ToString();
		UIHighScore.text = "Highscore: "+highscore.ToString ();
		//UILevel.text = Application.loadedLevelName;
		UILevel.text = SceneManager.GetActiveScene().name;
		
		// turn on the appropriate number of life indicators in the UI based on the number of lives left
		for(int i=0;i<UIExtraLives.Length;i++) {
			if (i<(lives-1)) { // show one less than the number of lives since you only typically show lifes after the current life in UI
				UIExtraLives[i].SetActive(true);
			} else {
				UIExtraLives[i].SetActive(false);
			}
		}
	}

	// public function to add points and update the gui and highscore player prefs accordingly
	public void AddPoints(int amount)
	{
		// increase score
		score+=amount;

		// update UI
		UIScore.text = "Score: "+score.ToString();

		// if score>highscore then update the highscore UI too
		if (score>highscore) {
			highscore = score;
			UIHighScore.text = "Highscore: "+score.ToString();
		}
	}

	// public function to remove player life and reset game accordingly
	public void ResetGame() {
		// remove life and update GUI
		lives--;
		refreshGUI();

		if (lives<=0) { // no more lives
			// save the current player prefs before going to GameOver
			PlayerPrefManager.SavePlayerState(score,highscore,lives);

			// load the gameOver screen
			SceneManager.LoadScene(levelAfterGameOver);
			//Application.LoadLevel (levelAfterGameOver);
		} else { // tell the player to respawn
			_player.GetComponent<CharacterController2D>().Respawn(_spawnLocation);
		}
	}

	public void SetCheckpoint(Vector3 position)
	{
		_spawnLocation = position;
	}

	// public function for level complete
	public void LevelCompete() {
		// save the current player prefs before moving to the next level
		PlayerPrefManager.SavePlayerState(score,highscore,lives);

		// use a coroutine to allow the player to get fanfare before moving to next level
		StartCoroutine(LoadNextLevel());
	}

	//Slows down time by a set amount for a set period of time
	//Used for collision impact
	public void FixedSlowmo()
	{
		//Debug.Log ("SLOWMO ENGAGED");
		StartCoroutine (SlowDownTime ());
	}



	// load the nextLevel after delay
	IEnumerator SlowDownTime()
	{
		//Slow down time by a predefine factor
		Time.timeScale = slowmoFactor;
		//Debug.Log ("SLOWMO ENGAGED");

		//Wait a set amount of time before going back to normal
		yield return new WaitForSeconds(slowmoTime); 

		//Debug.Log ("SLOWMO Done");
		//Go back to normal
		Time.timeScale = 1.0F;
		Time.fixedDeltaTime = 0.02F * Time.timeScale;
	}

	// load the nextLevel after delay
	IEnumerator LoadNextLevel() {
		yield return new WaitForSeconds(3.5f); 
		//Application.LoadLevel (levelAfterVictory);
		SceneManager.LoadScene (levelAfterVictory);
	}


	public void FixedFlashingSprites(SpriteRenderer[] sprites, int numTimes, float delay, bool disable = false)
	{
		StartCoroutine(FlashSprites(sprites, numTimes, delay, disable));
	}


	/// <summary>
	///  Coroutine to create a flash effect on all sprite renderers passed in to the function
	/// </summary>
	/// <param name="sprites">Sprites.</param>
	/// <param name="numTimes">Number times to flash</param>
	/// <param name="delay">Delay between flashes</param>
	/// <param name="disable">if you want to disable the renderer instead of change alpha</param>
	IEnumerator FlashSprites(SpriteRenderer[] sprites, int numTimes, float delay, bool disable) {
		// number of times to loop
		for (int loop = 0; loop < numTimes; loop++) {            
			// cycle through all sprites
			for (int i = 0; i < sprites.Length; i++) {                
				if (disable) {
					// for disabling
					sprites[i].enabled = false;
				} else {
					// for changing the alpha
					sprites[i].color = new Color(sprites[i].color.r, sprites[i].color.g, sprites[i].color.b, 0.5f);
				}
			}

			// delay specified amount
			yield return new WaitForSeconds(delay);

			// cycle through all sprites
			for (int i = 0; i < sprites.Length; i++) {
				if (disable) {
					// for disabling
					sprites[i].enabled = true;
				} else {
					// for changing the alpha
					sprites[i].color = new Color(sprites[i].color.r, sprites[i].color.g, sprites[i].color.b, 1);
				}
			}

			// delay specified amount
			yield return new WaitForSeconds(delay);
		}
	}


}
