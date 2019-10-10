using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleBlock : MonoBehaviour
{
	public Animator coinAnimator;

	public int nbrCollectible = 1;
	[SerializeField]
	private int opened = 0;

	[SerializeField]
	private bool open = false;

	private Animator animator;


	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public virtual void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			BounceBlock();
		}
	}

	private void BounceBlock()
	{
		if (open)
		{
			return;
		}

		if (opened < nbrCollectible - 1)
		{
			animator.SetTrigger("BlockBounce");
		}
		else
		{
			open = true;
			animator.SetTrigger("BlockOpen");
		}

		AudioManager.instance.PlaySound("CoinSound");
		coinAnimator.SetTrigger("CoinBounce");
		opened++;
		GameMaster.Instance.UpdateGold();
	}
}
