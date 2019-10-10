using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTrigger : MonoBehaviour
{
	public GameObject splashEffect;
	public AudioClip inSound;
	public AudioClip outSound;

	private AudioSource source;
	private Vector3 splashPosition;

	private void Awake()
	{
		source = GetComponent<AudioSource>();
		splashPosition = new Vector3(transform.position.x, transform.position.y + transform.GetComponent<Collider2D>().bounds.size.y / 2, transform.position.z);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		switch (collision.transform.tag)
		{
			case "Player":
				collision.transform.GetComponent<Player>().Hit();
				collision.transform.GetComponent<Player>().Fall();
				break;
			case "Enemy":
				collision.transform.GetComponent<Enemy>().DeleteMonster();
				break;
		}
		

		Instantiate(splashEffect, collision.transform.position, Quaternion.identity);
		source.clip = inSound;
		source.Play();
	}


	private void OnTriggerExit2D(Collider2D collision)
	{
		Instantiate(splashEffect, splashPosition, Quaternion.identity);
		source.clip = outSound;
		source.Play();
	}
}
