using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tiling : MonoBehaviour
{
	public int offsetX = 2;					//The offset so that we don't get any errors

	//These are made for checking if we need to instantiate stuff
	public bool hasARightBuddy = false;
	public bool hasALeftBuddy = false;

	public bool reverseScale = false;		//used if the object is not tilable

	private float spriteWidth = 0f;
	private Camera cam;
	private Transform myTransform;

	private void Awake()
	{
		cam = Camera.main;
		myTransform = transform;
	}

	// Start is called before the first frame update
	void Start()
    {
		SpriteRenderer sRenderer = GetComponent<SpriteRenderer>();
		spriteWidth = sRenderer.sprite.bounds.size.x * Mathf.Abs(myTransform.localScale.x);
    }

    // Update is called once per frame
    void Update()
    {
		if (!hasALeftBuddy || !hasARightBuddy)
		{
			//Calculate the camera extends (half the width) of what the camera can see in world coordinates
			float camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;

			//Calculate the x position where the camera can see the edge of the sprite
			float edgeVisiblePositionRight = (myTransform.position.x + spriteWidth / 2) - camHorizontalExtend;
			float edgeVisiblePositionLeft = (myTransform.position.x - spriteWidth / 2) - camHorizontalExtend;

			//Checking if we can see the edge of the element and then calling MakeNewBuddy if we can
			if (cam.transform.position.x >= edgeVisiblePositionRight - offsetX && !hasARightBuddy)
			{
				MakeNewBuddy(1);
				hasARightBuddy = true;
			}
			else if (cam.transform.position.x <= edgeVisiblePositionLeft + offsetX && !hasALeftBuddy)
			{
				MakeNewBuddy(-1);
				hasALeftBuddy = true;
			}
		}
    }

	//Function that create a buddy of the side required
	private void MakeNewBuddy(int rightOrLeft)
	{
		Vector3 newPosition = new Vector3(myTransform.position.x + spriteWidth * rightOrLeft, myTransform.position.y, myTransform.position.z);
		Transform newBuddy = Instantiate(myTransform, newPosition, myTransform.rotation) as Transform;

		//If not tilable let's reverse the x size of our object to get rid of ugly seams
		if (reverseScale)
		{
			newBuddy.localScale = new Vector3(newBuddy.localScale.x * -1, newBuddy.localScale.y, newBuddy.localScale.z);
		}

		newBuddy.parent = myTransform.parent;
		if (rightOrLeft > 0)
		{
			newBuddy.GetComponent<Tiling>().hasALeftBuddy = true;
		}
		else
		{
			newBuddy.GetComponent<Tiling>().hasARightBuddy = true;
		}
	}
}
