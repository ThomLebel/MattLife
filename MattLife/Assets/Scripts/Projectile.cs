using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public int force;
	public float lifeSpan;
	public Vector2 direction;
	public bool startMove = false;

	private IEnumerator coroutine;
	private Rigidbody2D rb2d;

	private void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
	}

	private void Start()
	{
		coroutine = WaitAndDie();
		StartCoroutine(coroutine);
		rb2d.AddForce(direction * force);
	}

	private void Update()
	{
		if (startMove)
		{
			startMove = false;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.transform.CompareTag("Enemy"))
		{
			collision.transform.GetComponent<Enemy>().Kill();
			StopCoroutine(coroutine);
			KillProjectile();
		}

		if (collision.transform.CompareTag("DivingEnemy"))
		{
			collision.transform.GetComponent<DivingEnemy>().Kill();
			StopCoroutine(coroutine);
			KillProjectile();
		}
	}

	IEnumerator WaitAndDie()
	{
		yield return new WaitForSecondsRealtime(lifeSpan);
		KillProjectile();
	}

	public void KillProjectile()
	{
		Destroy(gameObject);
	}
}
