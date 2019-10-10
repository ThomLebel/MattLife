using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Enemy : MonoBehaviour
{
	public LayerMask enemiesMask;
	public Collisions collisions;
	public int score;
	public GameObject deathEffect;
	public string hitSound;

	public State state;

	public float walkSpeed = 2;
	public float maxJumpHeight = 5;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public float accelerationTimeAirborne = .2f;
	public float accelerationTimeGrounded = .1f;

	protected Vector3 velocity;
	protected float velocityXSmoothing;
	protected float gravity;
	protected float maxJumpVelocity;
	protected float minJumpVelocity;
	protected float speed;
	[SerializeField] protected bool facingRight = false;
	[SerializeField] protected float collisionsRayLength = 0.1f;

	[SerializeField]
	protected bool inAir = false;
	protected bool bumpOnObstacle = false;
	protected bool hasFlipped = false;

	protected Vector2 directionalInput;
	protected EnemySpawner spawner;
	protected Controller2D controller;
	protected Animator animator;
	protected SpriteRenderer spriteRenderer;

	[SerializeField]
	protected float deletingDistance = 10f;
	protected Camera cam;
	protected float camHorizontalExtend;
	protected float camVerticalExtend;

	private void Awake()
	{
		controller = GetComponent<Controller2D>();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// Start is called before the first frame update
	protected virtual void Start()
    {
		cam = Camera.main;
		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
		camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;
		directionalInput = Vector2.left;
		speed = walkSpeed;

		state = State.walking;
	}

    // Update is called once per frame
    protected virtual void Update()
    {
		//Delete monster if out of sight
		if (transform.position.x >= cam.transform.position.x + camHorizontalExtend + deletingDistance ||
			transform.position.x <= cam.transform.position.x - camHorizontalExtend - deletingDistance)
		{
			DeleteMonster();
		}

		CalculateVelocity();

		controller.Move(velocity * Time.deltaTime, directionalInput);

		if (controller.collisions.above || controller.collisions.below)
		{
			if (controller.collisions.slidingDownMaxSlope)
			{
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			}
			else
			{
				velocity.y = 0;
			}
		}

		if (!controller.collisions.below)
		{
			inAir = true;
		}
		if (controller.collisions.below && inAir)
		{
			inAir = false;
		}

		if ((controller.collisions.left || controller.collisions.right) && !hasFlipped)
		{
			hasFlipped = true;
			Flip();
		}
		if (hasFlipped && (!controller.collisions.left && !controller.collisions.right))
		{
			hasFlipped = false;
		}

		collisions.Reset();
		HorizontalCollisions();
		VerticalCollisions();
		CollideWithPlayer();
	}

	protected void HorizontalCollisions()
	{
		for (int i = 0; i < controller.horizontalRayCount; i++)
		{
			//Raycast left
			Vector2 rayOrigin = controller.raycastOrigins.bottomLeft;
			rayOrigin += Vector2.up * (controller.horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left, collisionsRayLength, enemiesMask);
			Debug.DrawRay(rayOrigin, Vector2.left * collisionsRayLength, Color.red);

			if (hit)
			{
				if (hit.collider == controller.boxCollider || collisions.leftCollider == hit.collider)
				{
					continue;
				}

				collisions.left = true;
				collisions.leftCollider = hit.collider;
			}

			//Raycast right
			rayOrigin = controller.raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (controller.horizontalRaySpacing * i);
			hit = Physics2D.Raycast(rayOrigin, Vector2.right, collisionsRayLength, enemiesMask);
			Debug.DrawRay(rayOrigin, Vector2.right * collisionsRayLength, Color.blue);

			if (hit)
			{
				if (hit.collider == controller.boxCollider || collisions.rightCollider == hit.collider)
				{
					continue;
				}

				collisions.right = true;
				collisions.rightCollider = hit.collider;
			}
		}
	}

	protected void VerticalCollisions()
	{
		for (int i = 0; i < controller.verticalRayCount; i++)
		{
			//Raycast above
			Vector2 rayOrigin = controller.raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (controller.verticalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, collisionsRayLength, enemiesMask);
			Debug.DrawRay(rayOrigin, Vector2.up * collisionsRayLength, Color.green);

			if (hit)
			{
				if (hit.collider == controller.boxCollider || collisions.aboveCollider == hit.collider)
				{
					continue;
				}

				collisions.above = true;
				collisions.aboveCollider = hit.collider;
			}

			//Raycast below
			rayOrigin = controller.raycastOrigins.bottomLeft;
			rayOrigin += Vector2.right * (controller.verticalRaySpacing * i);
			hit = Physics2D.Raycast(rayOrigin, Vector2.down, collisionsRayLength, enemiesMask);
			Debug.DrawRay(rayOrigin, Vector2.down * collisionsRayLength, Color.black);

			if (hit)
			{
				if (hit.collider == controller.boxCollider || collisions.belowCollider == hit.collider)
				{
					continue;
				}

				collisions.below = true;
				collisions.belowCollider = hit.collider;
			}
		}
	}

	protected virtual void CollideWithPlayer()
	{
		Collider2D targetCollider = null;
		Player playerScript = null;
		bool attack = false;
		bool beingAttacked = false;

		if (collisions.below)
		{
			targetCollider = collisions.belowCollider;
			attack = true;
		}else if (collisions.above)
		{
			targetCollider = collisions.aboveCollider;
			beingAttacked = true;
		}else if (collisions.left)
		{
			targetCollider = collisions.leftCollider;
			attack = true;
		}else if (collisions.right)
		{
			targetCollider = collisions.rightCollider;
			attack = true;
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
			Kill();
		}
		else if (attack)
		{
			attack = false;
			playerScript.Hit();
		}

		playerScript.Bounce();
	}

	public void SetDirectionalInput(Vector2 input)
	{
		directionalInput = input;
	}

	public void SetEnemySpawner(EnemySpawner spawn)
	{
		spawner = spawn;
	}

	public void OnJumpIntended()
	{
		Vector2 jumpVelocity = Vector2.zero;

		if (controller.collisions.below)
		{
			if (controller.collisions.slidingDownMaxSlope)
			{
				if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) // not jumping against max slope
				{
					jumpVelocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					jumpVelocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			}
			else
			{
				jumpVelocity.y = maxJumpVelocity;
			}
		}

		if (jumpVelocity != Vector2.zero)
		{
			Jump(jumpVelocity);
		}
	}

	public void Jump(Vector2 jumpVelocity)
	{
		velocity = jumpVelocity;
		JumpEffect();
	}

	public void JumpEffect()
	{
		//Instantiate(jumpParticle, jumpParticlePosition.position, Quaternion.identity);
		//AudioManager.instance.PlaySound("PlayerJump");
	}

	protected void CalculateVelocity()
	{
		float targetVelocityX = directionalInput.x * speed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}

	public void Kill()
	{
		Debug.Log("monster killed");
		Instantiate(deathEffect, transform.position, Quaternion.identity);
		AudioManager.instance.PlaySound(hitSound);
		GameMaster.Instance.UpdateScore(score);
		DeleteMonster();
	}

	public void DeleteMonster()
	{
		if (spawner != null)
		{
			spawner.monsterList.Remove(gameObject);
		}
		Destroy(gameObject);
	}

	protected void Flip()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		directionalInput.x *= -1;

		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	[System.Serializable]
	public struct Collisions
	{
		public bool above, below;
		public bool left, right;

		public Collider2D ignoredCollider;
		public Collider2D leftCollider, rightCollider;
		public Collider2D aboveCollider, belowCollider;

		public void Reset()
		{
			above = below = false;
			left = right = false;

			leftCollider = rightCollider = null;
			aboveCollider = belowCollider = null;
		}
	}

	public enum State
	{
		walking,
		stun,
		launched
	}
}
