using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuButtonLoadLevel : MonoBehaviour {

	public void loadLevel(string levelToLoad)
	{
		//Application.LoadLevel (leveltoLoad);
		SceneManager.LoadScene (levelToLoad);
	}
}
