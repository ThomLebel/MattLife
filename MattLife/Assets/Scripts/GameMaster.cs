using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameMaster : MonoBehaviour
{
	public int levelIndex;
	public int goldLimit = 30;
	public int baseLife = 5;

	public GameObject menuPause;
	public GameObject galerie;
	public GameObject gameUI;
	public GameObject gameOverUI;
	public TextMeshProUGUI lifeText;
	public TextMeshProUGUI goldText;
	public TextMeshProUGUI scoreText;

	//public string state = "";
	public States state;
	public string musicName;
	public Vector3 playerSpawnPosition = Vector3.zero;
	public bool doubleJumpActive = false;
	public bool shootingActive = false;

	//[SerializeField]
	//private bool gamePaused = false;
	private GameObject player;
	private Player playerScript;
	[SerializeField]
	private float timeToMouseFadeOut = 2f;
	private float timeBeforeMouseFadeOut;
	private bool mouseInvisible = false;

	private Animator screenRevealAnimator;
	private StreamSouvenir streamSouvenir;

	private string[] gameOverPhrase = new string[] { "You shall not pass!", "You failed, Noob.", "So what? Gonna cry?", "Man up, big baby!" };
	private TextMeshProUGUI gameOverText;

	static public GameMaster Instance;

    // Start is called before the first frame update
    void Start()
    {
		Instance = this;

		streamSouvenir = GetComponent<StreamSouvenir>();

		screenRevealAnimator = GameObject.FindGameObjectWithTag("ScreenReveal").GetComponent<Animator>();
		screenRevealAnimator.Play("revealStart", -1, 0f);
		StartCoroutine("OnCompleteScreenReavealEndAnimation");

		player = GameObject.FindGameObjectWithTag("Player");
		playerScript = player.GetComponent<Player>();
		player.transform.position = playerSpawnPosition;
		ResetPlayer();

		AudioManager.instance.PlayMusic(musicName);
		AudioManager.instance.FadeFromMusic(musicName, 1.5f);

		gameOverText = gameOverUI.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

		state = States.game;
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

		if (state == States.game)
		{
			if(Input.GetButtonDown("Start"))
				PauseGame();
		}
		else
		{
			if (Input.GetButtonDown("Cancel") || Input.GetButtonDown("Start"))
			{
				if (state == States.souvenirs)
				{
					if (streamSouvenir.showTutorial)
					{
						streamSouvenir.HideTutorial();
					}
					else if(streamSouvenir.isReady)
					{
						streamSouvenir.StopStream();
					}
				}
				else if (state == States.galerie)
				{
					CloseGalerie();
				}
				else if (state == States.pause)
				{
					ResumeGame();
				}
			}
		}
    }

	public void ResetPlayer()
	{
		player.GetComponent<PlayerInput>().enabled = true;

		playerScript.SwapBody(levelIndex);
		playerScript.doubleJumpPower = doubleJumpActive;
		playerScript.shootPower = shootingActive;

		if (playerScript.playerLife <= 0)
		{
			playerScript.playerLife = baseLife;
		}

		lifeText.text = playerScript.playerLife.ToString();
		scoreText.text = playerScript.playerScore.ToString();

		string zero = "";
		if (playerScript.playerGold < 10)
		{
			zero = "0";
		}
		goldText.text = zero + playerScript.playerGold.ToString();
	}

	public void DisplayGalerie()
	{
		galerie.SetActive(true);
		menuPause.SetActive(false);
		state = States.galerie;
	}

	public void CloseGalerie()
	{
		galerie.SetActive(false);
		menuPause.SetActive(true);
		state = States.pause;
	}

	public void PauseGame()
	{
		Sound m = Array.Find(AudioManager.instance.musics, music => music.name == musicName);
		AudioManager.instance.FadeToMusic(m.name, 1f, m.gamePausedVolume);
		menuPause.SetActive(true);
		Time.timeScale = 0;
		state = States.pause;
	}

	public void ResumeGame()
	{
		Sound m = Array.Find(AudioManager.instance.musics, music => music.name == musicName);
		AudioManager.instance.FadeToMusic(m.name, 1f, m.volume);
		menuPause.SetActive(false);
		gameOverUI.SetActive(false);
		Time.timeScale = 1;
		state = States.game;
	}

	public void UpdateLife(int value)
	{
		playerScript.playerLife += value;
		if (playerScript.playerLife <= 0)
		{
			playerScript.playerLife = 0;
			state = States.gameOver;
			GameOver();
		}
		lifeText.text = playerScript.playerLife.ToString();
	}

	public void UpdateGold()
	{
		playerScript.playerGold++;
		if (playerScript.playerGold >= goldLimit)
		{
			playerScript.playerGold = 0;
			UpdateLife(1);
			AudioManager.instance.PlaySound("LifeUp");
			playerScript.LifeUp();
		}
		string zero = "";
		if (playerScript.playerGold < 10)
		{
			zero = "0";
		}
		goldText.text = zero + playerScript.playerGold.ToString();
	}

	public void UpdateScore(int value)
	{
		playerScript.playerScore += value;
		if (playerScript.playerScore <= 0)
		{
			playerScript.playerScore = 0;
		}
		scoreText.text = playerScript. playerScore.ToString();
	}

	public void GameOver()
	{
		state = States.gameOver;
		Time.timeScale = 0f;
		Sound m = Array.Find(AudioManager.instance.musics, music => music.name == musicName);
		AudioManager.instance.FadeToMusic(m.name, 1f, m.gamePausedVolume);
		AudioManager.instance.PlaySound("GameOver");

		int randText = UnityEngine.Random.Range(0, gameOverPhrase.Length);
		gameOverText.text = gameOverPhrase[randText];
		gameOverUI.SetActive(true);
		gameOverUI.GetComponent<Animator>().SetTrigger("reveal");
	}

	public void RestartLevel()
	{
		ResetPlayer();
		playerScript.Fall();
		playerScript.ActivateInvulnerability();
		ResumeGame();
	}

	public void QuitGame()
	{
		SouvenirsHolder souvenirsHolder = GameObject.FindGameObjectWithTag("SouvenirsHolder").GetComponent<SouvenirsHolder>();
		SaveSystem.SaveGame(playerScript, souvenirsHolder);

		Destroy(GameObject.FindGameObjectWithTag("SouvenirsHolder"));
		Destroy(player);
		
		AudioManager.instance.FadeToMusic(musicName, 1f);
		screenRevealAnimator.SetTrigger("End");
		Time.timeScale = 1;

		StartCoroutine("LoadMainMenu");
	}

	IEnumerator LoadMainMenu()
	{
		yield return new WaitForSecondsRealtime(1f);

		SceneManager.LoadSceneAsync("MainMenu");
	}

	IEnumerator OnCompleteScreenReavealEndAnimation()
	{
		while (screenRevealAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f)
			yield return null;

		// TODO: Do something when animation did complete
		state = States.game;
	}

	public enum States
	{
		menu,
		game,
		galerie,
		souvenirs,
		pause,
		gameOver
	}
}
 