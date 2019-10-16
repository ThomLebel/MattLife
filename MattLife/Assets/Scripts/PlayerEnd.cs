using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnd : MonoBehaviour
{
	public void PlayWalkSound()
	{
		AudioManager.instance.PlaySound("PlayerWalk");
	}
}
