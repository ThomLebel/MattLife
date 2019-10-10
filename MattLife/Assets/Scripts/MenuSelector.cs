using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuSelector : MonoBehaviour
{
	public List<GameObject> btnList = new List<GameObject>();
	public RectTransform selector;

	private int currentBtnSelected = 0;
	private float timeToNextSlide;
	[SerializeField]
	private float timeBetweenSlide = 0.25f;

	// Start is called before the first frame update
	void Start()
    {
		selector.anchoredPosition = btnList[currentBtnSelected].GetComponent<RectTransform>().anchoredPosition;
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Return))
		{
			btnList[currentBtnSelected].GetComponent<Button>().onClick.Invoke();
		}

		float directionalInput = Input.GetAxisRaw("Vertical");

		if (timeToNextSlide <= 0)
		{
			if (directionalInput != 0)
			{
				SwitchSelectedButton(directionalInput);
				timeToNextSlide = timeBetweenSlide;
			}
		}
		else
		{
			timeToNextSlide = timeToNextSlide - Time.unscaledDeltaTime;
		}
	}

	private void SwitchSelectedButton(float direction)
	{
		if (direction > 0)
		{
			currentBtnSelected--;
		}
		else
		{
			currentBtnSelected++;
		}

		if (currentBtnSelected < 0)
		{
			currentBtnSelected = btnList.Count - 1;
		}else if (currentBtnSelected >= btnList.Count)
		{
			currentBtnSelected = 0;
		}

		selector.anchoredPosition = btnList[currentBtnSelected].GetComponent<RectTransform>().anchoredPosition;
	}

	public void OnPointerEnter(GameObject button)
	{
		currentBtnSelected = btnList.IndexOf(button);

		selector.anchoredPosition = btnList[currentBtnSelected].GetComponent<RectTransform>().anchoredPosition;
	}
}
