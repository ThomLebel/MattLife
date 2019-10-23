using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
	public SpriteRenderer flag;
	public Sprite flagOn;

	private bool isRevealed = false;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!collision.CompareTag("Player") || isRevealed)
		{
			return;
		}

		isRevealed = true;
		flag.sprite = flagOn;
		GameMaster.Instance.UpdateLife(1);
		AudioManager.instance.PlaySound("LifeUp");
		collision.GetComponent<Player>().LifeUp();
	}
}
