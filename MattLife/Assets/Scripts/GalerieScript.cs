using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GalerieScript : MonoBehaviour
{
	public List<GameObject> souvenirsList;
	public Sprite revealedBackground;
	public StreamSouvenir streamSouvenir;

	public RectTransform selector;
	public Scrollbar scrollBar;
	[SerializeField]
	private int currentSouvenirSelected = 0;
	private float timeToNextSlide;
	[SerializeField]
	private float timeBetweenSlide = 0.25f;

	private SouvenirsHolder souvenirHolder;

	private void Start()
	{
		souvenirHolder = GameObject.FindGameObjectWithTag("SouvenirsHolder").GetComponent<SouvenirsHolder>();
		foreach (GameObject button in souvenirsList)
		{
			button.GetComponent<Button>().onClick.AddListener(() => DisplayUnrevealedMessage());
		}
		foreach (Souvenir souvenir in souvenirHolder.souvenirsList)
		{
			if (souvenir.revealed)
			{
				RevealSouvenirs(souvenir);
			}
		}

		currentSouvenirSelected = 0;

		RectTransform souvenirParent = souvenirsList[currentSouvenirSelected].transform.parent.GetComponent<RectTransform>();
		selector.anchoredPosition = new Vector2(souvenirsList[currentSouvenirSelected].GetComponent<RectTransform>().anchoredPosition.x, souvenirParent.anchoredPosition.y);

		float lineIndex = 1 - (Mathf.Floor(currentSouvenirSelected / 5) / 6);
		scrollBar.value = lineIndex;
	}

	/*private void OnEnable()
	{
		currentSouvenirSelected = 0;

		RectTransform souvenirParent = souvenirsList[currentSouvenirSelected].transform.parent.GetComponent<RectTransform>();
		selector.anchoredPosition = new Vector2(souvenirsList[currentSouvenirSelected].GetComponent<RectTransform>().anchoredPosition.x, souvenirParent.anchoredPosition.y);

		float lineIndex = 1 - (Mathf.Floor(currentSouvenirSelected / 5) / 6);
		scrollBar.value = lineIndex;
		Debug.Log(lineIndex);
	}*/

	private void Update()
	{
		if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Return))
		{
			souvenirsList[currentSouvenirSelected].GetComponent<Button>().onClick.Invoke();
		}

		Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		Vector2 direction = Vector2.zero;
		if (directionalInput.x > 0)
		{
			direction = Vector2.right;
		}else if (directionalInput.x < 0)
		{
			direction = Vector2.left;
		}
		else if (directionalInput.y > 0)
		{
			direction = Vector2.up;
		}
		else if (directionalInput.y < 0)
		{
			direction = Vector2.down;
		}

		if (timeToNextSlide <= 0)
		{
			if (direction != Vector2.zero)
			{
				SwitchSelectedSouvenir(direction);
				timeToNextSlide = timeBetweenSlide;
			}
		}
		else
		{
			timeToNextSlide = timeToNextSlide - Time.unscaledDeltaTime;
		}
	}

	private void SwitchSelectedSouvenir(Vector2 dir)
	{
		if (dir.x > 0)
		{
			currentSouvenirSelected++;
		}else if (dir.x < 0)
		{
			currentSouvenirSelected--;
		}else if (dir.y > 0)
		{
			currentSouvenirSelected -= 5;
		}else if (dir.y < 0)
		{
			currentSouvenirSelected += 5;
		}

		if (currentSouvenirSelected < 0)
		{
			currentSouvenirSelected = souvenirsList.Count - 1;
		}else if (currentSouvenirSelected >= souvenirsList.Count)
		{
			currentSouvenirSelected = 0;
		}

		RectTransform souvenirParent = souvenirsList[currentSouvenirSelected].transform.parent.GetComponent<RectTransform>();
		selector.anchoredPosition = new Vector2(souvenirsList[currentSouvenirSelected].GetComponent<RectTransform>().anchoredPosition.x, souvenirParent.anchoredPosition.y);

		float lineIndex = 1 - (Mathf.Floor(currentSouvenirSelected / 5) / 6);
		scrollBar.value = lineIndex;
	}

	public void RevealSouvenirs(Souvenir souvenir)
	{
		souvenir.revealed = true;
		//Change the souvenirs background
		souvenirsList[souvenir.id].GetComponent<Image>().sprite = revealedBackground;
		RectTransform souvenirTransform = souvenirsList[souvenir.id].GetComponent<RectTransform>();

		//Remove the unerevealed message from the button and add the correct stream
		souvenirsList[souvenir.id].GetComponent<Button>().onClick.RemoveAllListeners();
		if (souvenir.video != null)
		{
			souvenirsList[souvenir.id].GetComponent<Button>().onClick.AddListener(() => streamSouvenir.StartVideoStream(souvenir, souvenirsList[souvenir.id].transform.position));
		}
		else
		{
			souvenirsList[souvenir.id].GetComponent<Button>().onClick.AddListener(() => streamSouvenir.StartPhotoStream(souvenir, souvenirsList[souvenir.id].transform.position));
		}

		GameObject souvenirs = new GameObject();
		Image souvenirsSprite = souvenirs.AddComponent<Image>();
		RectTransform rectTransform = souvenirs.GetComponent<RectTransform>();

		souvenirsSprite.sprite = souvenir.photo;
		souvenirsSprite.preserveAspect = true;
		rectTransform.SetParent(souvenirTransform);
		rectTransform.offsetMax = souvenirTransform.offsetMax;
		rectTransform.offsetMin = souvenirTransform.offsetMin;
		rectTransform.anchorMax = souvenirTransform.anchorMax;
		rectTransform.anchorMin = souvenirTransform.anchorMin;
		rectTransform.pivot = souvenirTransform.pivot;
		rectTransform.anchoredPosition = new Vector2(0f,0f);
	}

	public void DisplayUnrevealedMessage()
	{
		Debug.Log("You haven't revealed this souvenir yet");
	}

	public void OnPointerEnter(GameObject button)
	{
		currentSouvenirSelected = souvenirsList.IndexOf(button);

		RectTransform souvenirParent = souvenirsList[currentSouvenirSelected].transform.parent.GetComponent<RectTransform>();
		selector.anchoredPosition = new Vector2(souvenirsList[currentSouvenirSelected].GetComponent<RectTransform>().anchoredPosition.x, souvenirParent.anchoredPosition.y);

		float lineIndex = 1 - (Mathf.Floor(currentSouvenirSelected / 5) / 6);
		scrollBar.value = lineIndex;
	}
}
