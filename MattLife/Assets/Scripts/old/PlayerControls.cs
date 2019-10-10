/*
 
NOT USED ANYMORE !

 */



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
	public Transform groundCheck;
	public float speed;
	public float jumpTakeOff;
	public float bounceTakeOff;
	public LayerMask groundLayerMask;
	public LayerMask jumpThroughLayerMask;

	private float horizontal;
	private float vertical;
	private float safeSpot = 0.2f;
	[SerializeField]
	private float bounceTimerWindow = 0.2f;

	const float groundedRadius = .2f;

	[SerializeField]
	private bool grounded = false;
	[SerializeField]
	private bool isJumping = false;
	[SerializeField]
	private bool isFalling = false;
	[SerializeField]
	private bool isJumpingDown = false;
	[SerializeField]
	private bool facingRight = true;
	private bool canBounce = false;

	private IEnumerator bounceOnMonster;

	private Rigidbody2D rb2d;
	private Collider2D platformCollider;
	private CapsuleCollider2D playerCollider;

	private void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
		playerCollider = GetComponent<CapsuleCollider2D>();
	}

	// Start is called before the first frame update
	void Start()
    {
		Debug.Log("We need to add the crouch control");
    }

    // Update is called once per frame
    void Update()
    {
		grounded = false;
		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, groundLayerMask);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
				grounded = true;
		}

		horizontal = Input.GetAxisRaw("Horizontal");
		vertical = Input.GetAxisRaw("Vertical");

		if (Input.GetButtonDown("Jump"))
		{
			//Jump from the ground
			if (grounded && !isJumping && !isFalling && !isJumpingDown)
			{
				isJumping = true;
				if (vertical < safeSpot * -1)
				{
					RaycastHit2D ray = Physics2D.Linecast(transform.position, groundCheck.position, jumpThroughLayerMask);
					if (ray.collider != null)
					{
						platformCollider = ray.collider;
						Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
						isJumpingDown = true;
					}
				}
				else
				{
					rb2d.AddForce(new Vector2(0f, jumpTakeOff));
				}
			}
			//Jump from the head of a monster
			if (canBounce)
			{
				rb2d.AddForce(new Vector2(0f, jumpTakeOff+bounceTakeOff));
			}
		}

		if (Input.GetButtonUp("Jump"))
		{
			isJumping = false;
			if (!grounded && rb2d.velocity.y > 0)
			{
				rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y * 0.5f);
			}
		}

		if (isJumpingDown)
		{
			if (transform.position.y < platformCollider.transform.position.y)
			{
				Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
				platformCollider = null;
				isJumpingDown = false;
			}
		}

		// If the input is moving the player right and the player is facing left...
		if (horizontal > 0 && !facingRight)
		{
			// ... flip the player.
			Flip();
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (horizontal < 0 && facingRight)
		{
			// ... flip the player.
			Flip();
		}
	}

	private void FixedUpdate()
	{
		rb2d.velocity = new Vector2(horizontal * speed * Time.fixedDeltaTime, rb2d.velocity.y);
		MovingOnSlopes();
	}

	private void MovingOnSlopes()
	{
		if (grounded && horizontal == 0 && vertical == 0 && !isJumping && !isJumpingDown)
		{
			RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10, groundLayerMask);

			// Check if we are on the slope
			if (hit && Mathf.Abs(hit.normal.x) > 0.1f)
			{
				// We freeze all the rigidbody constraints and put velocity to 0
				rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
				rb2d.velocity = Vector2.zero;
			}
		}
		else
		{
			// if we are on air or moving - jumping, unfreeze all and freeze only rotation.
			rb2d.constraints = RigidbodyConstraints2D.None;
			rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
		}
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void Bounce()
	{
		canBounce = true;

		bounceOnMonster = BounceOnMonster(bounceTimerWindow);
		StartCoroutine(bounceOnMonster);

		rb2d.AddForce(new Vector2(0f, bounceTakeOff));
	}

	private IEnumerator BounceOnMonster(float time)
	{
		yield return new WaitForSeconds(time);
		canBounce = false;
		StopCoroutine(bounceOnMonster);
	}
}
