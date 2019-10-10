using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
	public string type;
	[SerializeField]
	protected bool open = false;

	private Animator animator;
	protected Camera cam;

	protected virtual void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Start()
	{
		cam = Camera.main;
	}

	public virtual void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player") && !open)
		{
			open = true;
			animator.SetTrigger("BlockOpen");
		}
	}
}
