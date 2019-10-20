using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
	public Souvenir[] souvenirs;
	public GameObject photoProjector;
	public GameObject menuPause;
	public AudioSource source;
	public bool isPaused = false;

	private Image photo;

	public float timeBetweenSlide = 2f;
	private float timeBeforeNextSlide;

	[SerializeField]
	private int currentIndex = 0;
	private int maxIndex;
	[SerializeField]
	private bool isActive = false;

	[Tooltip("Color to fade to")]
	[SerializeField]
	private Color EndColor = Color.white;

	[Tooltip("Color to fade from")]
	[SerializeField]
	private Color StartColor = Color.clear;

	private void OnEnable()
	{
		timeBeforeNextSlide = timeBetweenSlide;
		maxIndex = souvenirs.Length;
		photo = photoProjector.GetComponent<Image>();

		isActive = true;
	}

	// Update is called once per frame
	void Update()
    {
		if (Input.GetButtonDown("Start"))
		{
			if (!isPaused)
			{
				PauseGame();
			}
			else
			{
				ResumeGame();
			}
		}

		if (isPaused && Input.GetButtonDown("Cancel"))
		{
			ResumeGame();
		}

		if (currentIndex >= maxIndex-1 || !isActive)
		{
			return;
		}

		if (timeBeforeNextSlide <= 0)
		{
			currentIndex++;

			iTween.ValueTo(this.gameObject, iTween.Hash(
				"from", 1f,
				"to", 0f,
				"time", 0.8f,
				"onupdate", "updateColor",
				"oncomplete", "SwitchSouvenir"
			));

			timeBeforeNextSlide = timeBetweenSlide + 1.6f;
		}
		else
		{
			timeBeforeNextSlide -= Time.deltaTime;
		}
    }

	public void SwitchSouvenir()
	{
		photo.sprite = souvenirs[currentIndex].photo;
		photo.preserveAspect = true;
		photo.enabled = true;

		iTween.ValueTo(this.gameObject, iTween.Hash(
			"from", 0f,
			"to", 1f,
			"time", 0.8f,
			"onupdate", "updateColor"
		));

	}

	public void updateColor(float val)
	{
		photo.color = ((1f - val) * StartColor) + (val * EndColor);
	}

	public void PauseGame()
	{
		isPaused = true;
		menuPause.SetActive(true);
		Time.timeScale = 0;
		source.Pause();
	}

	public void ResumeGame()
	{
		isPaused = false;
		menuPause.SetActive(false);
		Time.timeScale = 1;
		source.Play();
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
