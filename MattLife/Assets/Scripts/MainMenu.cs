using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public GameObject playerPrefab;
	public GameObject galerie;
	public GameObject blackScreen;
	public GameObject gameLauncher;

	public string firstLevel;
	public string musicName;

	public State state;

	private GameObject player;
	private string levelToLoad;

	[SerializeField]
	private float timeToMouseFadeOut = 2f;
	private float timeBeforeMouseFadeOut;
	private bool mouseInvisible = false;

	private MenuSelector menuPauseScript;
	private GameData gameData;
	private SouvenirsHolder souvenirsHolder;

	private void Awake()
	{
		menuPauseScript = gameObject.GetComponent<MenuSelector>();
	}

	// Start is called before the first frame update
	void Start()
    {
		state = State.menu;
		souvenirsHolder = GameObject.FindGameObjectWithTag("SouvenirsHolder").GetComponent<SouvenirsHolder>();
		gameData = SaveSystem.LoadGame();

		if (gameData != null)
		{
			for (int i = 0; i < gameData.souvenirsRevealed.Length; i++)
			{
				souvenirsHolder.souvenirsList[i].revealed = gameData.souvenirsRevealed[i];
			}
		}

		levelToLoad = firstLevel;

		AudioManager.instance.PlayMusic(musicName);
		AudioManager.instance.FadeFromMusic(musicName, 1.5f);
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
		{
			timeBeforeMouseFadeOut = timeToMouseFadeOut;
			Cursor.visible = true;
			mouseInvisible = false;
		}
		else
		{
			timeBeforeMouseFadeOut -= Time.unscaledDeltaTime;
		}

		if (timeBeforeMouseFadeOut <= 0 && !mouseInvisible)
		{
			mouseInvisible = true;
			Cursor.visible = false;
		}

		if (Input.GetButtonDown("Cancel"))
		{
			if (state == State.galerie)
			{
				CloseGalerie();
			}
			else if (state == State.stream)
			{
				gameObject.GetComponent<StreamSouvenir>().StopStream();
			}
		}
	}

	public void GameLauncher()
	{
		if (gameData != null)
		{
			gameLauncher.SetActive(true);
			menuPauseScript.enabled = false;
			state = State.gameLauncher;
		}
		else
		{
			NewGame();
		}
	}

	public void ContinueSavedGame()
	{
		player = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity);
		Player playerScript = player.GetComponent<Player>();
		if (gameData != null)
		{
			playerScript.playerLife = gameData.lives;
			playerScript.playerGold = gameData.gold;
			playerScript.playerScore = gameData.score;
			levelToLoad = gameData.currentLevel;
		}
		player.SetActive(false);

		StartGame();
	}

	public void NewGame()
	{
		player = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity);
		player.SetActive(false);

		StartGame();
	}

	public void CloseGameLauncher()
	{
		gameLauncher.SetActive(false);
		menuPauseScript.enabled = true;
		state = State.menu;
	}

	private void StartGame()
	{
		blackScreen.SetActive(true);
		blackScreen.GetComponent<Animator>().Play("BlackScreen", -1, 0f);

		AudioManager.instance.FadeToMusic(musicName, 1.5f);
		StartCoroutine("LoadLevel");
	}

	IEnumerator LoadLevel()
	{
		yield return new WaitForSecondsRealtime(1.8f);

		player.SetActive(true);

		SceneManager.LoadScene(levelToLoad);
	}

	public void DisplayGalerie()
	{
		galerie.SetActive(true);
		menuPauseScript.enabled = false;
		state = State.galerie;
	}

	public void CloseGalerie()
	{
		galerie.SetActive(false);
		menuPauseScript.enabled = true;
		state = State.menu;
	}

	public void QuitGame()
	{
		Debug.Log("quit game");
		Application.Quit();
	}

	public enum State
	{
		menu,
		galerie,
		gameLauncher,
		stream
	}
}
