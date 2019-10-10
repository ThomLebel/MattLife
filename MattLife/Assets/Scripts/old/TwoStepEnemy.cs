/*
 
NOT USED ANYMORE !

 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoStepEnemy : EnemyScript
{
	public float launchSpeed;
	public Sprite baseSprite;
	public Sprite stunSprite;

	[SerializeField]
	private bool isStun = false;
	[SerializeField]
	private bool isLaunch = false;
	[SerializeField]
	private bool scoreGained = false;
	[SerializeField]
	private float stunTime = 3f;

	private IEnumerator stunCoroutine;


	public override void OnCollisionEnter2D(Collision2D collision)
	{
		//base.OnCollisionEnter2D(collision);
		if (collision.transform.tag == "Player")
		{
			Player playerScript = collision.transform.GetComponent<Player>();
			if (playerScript.isInvulnerable)
			{
				//Physics2D.IgnoreCollision(collision.collider, ownCollider);
				return;
			}

			Vector2 contact = collision.GetContact(0).point;

			if (contact.y >= transform.position.y)
			{
				AudioManager.instance.PlaySound(hitSound);
				Instantiate(effect, transform.position, Quaternion.identity);
				//On stun le monstre
				if (!isStun)
				{
					StunMonster();
				}
				//On lance le monstre sous forme de coquille
				else if (isStun && !isLaunch)
				{
					LaunchMonster(contact);
				}
				//On stop le monstre
				else if (isStun && isLaunch)
				{
					StopMonster();
				}

				//We bounce off the monster head
				playerScript.AllowBounceOffMonster();
			}
			else
			{
				if (isStun && !isLaunch)
				{
					LaunchMonster(contact);
				}
				else
				{
					playerScript.Hit();
				}
			}

			playerScript.Bounce();
		}
		else if (collision.transform.tag == "Enemy" && isLaunch)
		{
			Instantiate(effect, transform.position, Quaternion.identity);
			//collision.transform.GetComponent<EnemyScript>().Kill();
			collision.transform.GetComponent<Enemy>().Kill();
		}
	}

	private void StunMonster()
	{
		isStun = true;
		Debug.Log("Stun this monster !");
		animator.enabled = false;
		spriteRenderer.sprite = stunSprite;
		speed = 0f;
		stunCoroutine = StunTimer();
		StartCoroutine(stunCoroutine);
		if (!scoreGained)
		{
			scoreGained = true;
			GameMaster.Instance.UpdateScore(score);
		}
	}

	private void LaunchMonster(Vector2 contact)
	{
		isLaunch = true;
		gameObject.layer = LayerMask.NameToLayer("MovingShells");

		StopCoroutine(stunCoroutine);
		switch (direction)
		{
			case 1:
				if (contact.x > transform.position.x)
				{
					Flip();
				}
				break;
			case -1:
				if (contact.x < transform.position.x)
				{
					Flip();
				}
				break;
		}
		speed = launchSpeed;
	}

	private void StopMonster()
	{
		isLaunch = false;
		gameObject.layer = LayerMask.NameToLayer("Enemies");
		speed = 0f;
		stunCoroutine = StunTimer();
		StartCoroutine(stunCoroutine);
	}

	private IEnumerator StunTimer()
	{
		yield return new WaitForSeconds(stunTime);
		isStun = false;
		spriteRenderer.sprite = baseSprite;
		animator.enabled = true;
		speed = walkSpeed;
	}
}
