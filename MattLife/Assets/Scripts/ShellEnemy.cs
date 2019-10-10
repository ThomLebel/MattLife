using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ShellEnemy : Enemy
{
	public float launchSpeed;
	public Sprite baseSprite;
	public Sprite stunSprite;

	[SerializeField]
	private bool scoreGained = false;
	[SerializeField]
	private float stunTime = 3f;
	[SerializeField]
	private float timeBetweenPlayerCollisions = 0.05f;
	private float timeBeforeNextPlayerCollisions;

	private IEnumerator stunCoroutine;

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		
		CollideWithEnemy();
	}

	protected override void CollideWithPlayer()
	{
		if (timeBeforeNextPlayerCollisions > 0)
		{
			timeBeforeNextPlayerCollisions -= Time.deltaTime;
			return;
		}

		Collider2D targetCollider = null;
		Player playerScript = null;
		bool attack = false;
		bool beingAttacked = false;

		if (collisions.below && collisions.belowCollider.CompareTag("Player"))
		{
			targetCollider = collisions.belowCollider;
			if(state != State.stun)
				attack = true;
		}
		else if (collisions.above && collisions.aboveCollider.CompareTag("Player"))
		{
			targetCollider = collisions.aboveCollider;
			beingAttacked = true;
		}
		else if (collisions.left && collisions.leftCollider.CompareTag("Player"))
		{
			targetCollider = collisions.leftCollider;
			if (state != State.stun)
				attack = true;
			else
				beingAttacked = true;
		}
		else if (collisions.right && collisions.rightCollider.CompareTag("Player"))
		{
			targetCollider = collisions.rightCollider;
			if (state != State.stun)
			{
				attack = true;
			}
			else
			{
				beingAttacked = true;
			}
		}

		if (targetCollider == null)
		{
			return;
		}

		playerScript = targetCollider.GetComponent<Player>();

		if (playerScript == null)
		{
			return;
		}
		if (playerScript.isInvulnerable)
		{
			collisions.ignoredCollider = targetCollider;
			return;
		}
		else
		{
			collisions.ignoredCollider = null;
		}

		if (beingAttacked)
		{
			beingAttacked = false;
			playerScript.AllowBounceOffMonster();
			UnderAttack(targetCollider);
		}
		else if (attack)
		{
			attack = false;
			playerScript.Hit();
		}

		playerScript.Bounce();
		timeBeforeNextPlayerCollisions = timeBetweenPlayerCollisions;
	}

	private void CollideWithEnemy()
	{
		if (state != State.launched)
		{
			return;
		}

		Enemy enemyScript = null;
		Collider2D target = null;

		if (collisions.below && (collisions.belowCollider.CompareTag("Enemy") || collisions.belowCollider.CompareTag("DivingEnemy")))
		{
			target = collisions.belowCollider;
		}
		else if (collisions.above && (collisions.aboveCollider.CompareTag("Enemy") || collisions.aboveCollider.CompareTag("DivingEnemy")))
		{
			target = collisions.aboveCollider;
		}
		else if (collisions.left && (collisions.leftCollider.CompareTag("Enemy") || collisions.leftCollider.CompareTag("DivingEnemy")))
		{
			target = collisions.leftCollider;
		}
		else if (collisions.right && (collisions.rightCollider.CompareTag("Enemy") || collisions.rightCollider.CompareTag("DivingEnemy")))
		{
			target = collisions.rightCollider;
		}

		if (target != null)
		{

			enemyScript = target.GetComponent<Enemy>();
		}

		if (enemyScript != null)
		{
			collisions.rightCollider = null;
			collisions.right = false;
			Instantiate(deathEffect, transform.position, Quaternion.identity);

			if (enemyScript.state == State.launched)
			{
				Kill();
			}

			enemyScript.Kill();
		}
	}

	private void UnderAttack(Collider2D targetCollider)
	{
		AudioManager.instance.PlaySound(hitSound);
		Instantiate(deathEffect, transform.position, Quaternion.identity);
		switch (state)
		{
			case State.launched:
				StopMonster();
				break;
			case State.walking:
				StunMonster();
				break;
			case State.stun:
				LaunchMonster(targetCollider);
				break;
		}
	}

	private void StunMonster()
	{
		state = State.stun;
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

	private void LaunchMonster(Collider2D targetCollider)
	{
		state = State.launched;
		enemiesMask |= (1 << LayerMask.NameToLayer("Enemies"));
		StopCoroutine(stunCoroutine);

		if (targetCollider.transform.position.x > transform.position.x && facingRight)
		{
			Flip();
		}
		else if(targetCollider.transform.position.x < transform.position.x && !facingRight)
				{
			Flip();
		}

		speed = launchSpeed;
	}

	private void StopMonster()
	{
		state = State.stun;
		enemiesMask &= ~(1 << LayerMask.NameToLayer("Enemies"));
		speed = 0f;
		stunCoroutine = StunTimer();
		StartCoroutine(stunCoroutine);
	}

	private IEnumerator StunTimer()
	{
		yield return new WaitForSeconds(stunTime);
		state = State.walking;
		spriteRenderer.sprite = baseSprite;
		animator.enabled = true;
		speed = walkSpeed;
	}
}
