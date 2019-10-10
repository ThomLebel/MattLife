using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivingEnemy : RaycastController
{
	public Vector3[] localWaypoints;
	private Vector3[] globalWaypoints;
	
	public Collisions collisions;

	public int score;
	public float jumpTime;
	public float diveTime;
	public GameObject deathEffect;
	public string hitSound;
	public Sprite jumpSprite;
	public Sprite diveSprite;

	[SerializeField]
	private State state;
	private bool isActive;

	[SerializeField] private float jumpWaitTime = 2f;
	[SerializeField] private float diveWaitTime = 0.5f;
	[SerializeField] private float timeBeforeNextMove;
	[SerializeField] private float collisionsRayLength = 0.1f;

	[SerializeField]
	private float activatingDistance = 10f;
	private Camera cam;
	private float camHorizontalExtend;
	private float camVerticalExtend;

	private SpriteRenderer spriteRenderer;

	protected override void Awake()
	{
		base.Awake();
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// Start is called before the first frame update
	protected override void Start()
    {
		base.Start();
		cam = Camera.main;

		camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;

		globalWaypoints = new Vector3[localWaypoints.Length];
		for (int i = 0; i < localWaypoints.Length; i++)
		{
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}

		state = State.resting;
	}

    // Update is called once per frame
    void Update()
	{
		//Deactivate monster if out of sight
		if (transform.position.x >= cam.transform.position.x + camHorizontalExtend + activatingDistance ||
			transform.position.x <= cam.transform.position.x - camHorizontalExtend - activatingDistance)
		{
			isActive = false;
		}
		else
		{
			isActive = true;
		}
		if (!isActive)
		{
			return;
		}

		UpdateRaycastOrigins();
		collisions.Reset();
		HorizontalCollisions();
		VerticalCollisions();
		CollideWithPlayer();

		if (timeBeforeNextMove <= 0)
		{
			switch (state)
			{
				case State.resting:
					Jump();
					break;
				case State.inAir:
					Dive();
					break;
				default:
					break;
			}
		}
		else
		{
			timeBeforeNextMove -= Time.deltaTime;
		}

	}

	private void Jump()
	{
		state = State.jumping;
		spriteRenderer.sprite = jumpSprite;
		iTween.MoveTo(gameObject, iTween.Hash(
			"position", globalWaypoints[1],
			"time", jumpTime,
			"easeType", "easeOutQuint",
			"oncomplete", "ReachJumpApex"
		));
	}

	private void Dive()
	{
		state = State.diving;
		spriteRenderer.sprite = diveSprite;
		iTween.MoveTo(gameObject, iTween.Hash(
			"position", globalWaypoints[0],
			"time", diveTime,
			"easeType", "easeInCubic",
			"oncomplete", "Resting"
		));
	}

	private void ReachJumpApex()
	{
		state = State.inAir;
		timeBeforeNextMove = diveWaitTime;
	}

	private void Resting()
	{
		state = State.resting;
		timeBeforeNextMove = jumpWaitTime;
	}

	private void CollideWithPlayer()
	{
		Collider2D targetCollider = null;
		Player playerScript = null;
		bool attack = false;
		bool beingAttacked = false;

		if (collisions.below)
		{
			targetCollider = collisions.belowCollider;
			attack = true;
		}
		else if (collisions.above)
		{
			targetCollider = collisions.aboveCollider;
			beingAttacked = true;
		}
		else if (collisions.left)
		{
			targetCollider = collisions.leftCollider;
			attack = true;
		}
		else if (collisions.right)
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
		Destroy(gameObject);
	}

	private void HorizontalCollisions()
	{
		for (int i = 0; i < horizontalRayCount; i++)
		{
			//Raycast left
			Vector2 rayOrigin = raycastOrigins.bottomLeft;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left, collisionsRayLength, collisionMask);
			Debug.DrawRay(rayOrigin, Vector2.left * collisionsRayLength, Color.red);

			if (hit)
			{
				if (hit.collider == boxCollider || collisions.leftCollider == hit.collider)
				{
					continue;
				}

				collisions.left = true;
				collisions.leftCollider = hit.collider;
			}

			//Raycast right
			rayOrigin = raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			hit = Physics2D.Raycast(rayOrigin, Vector2.right, collisionsRayLength, collisionMask);
			Debug.DrawRay(rayOrigin, Vector2.right * collisionsRayLength, Color.blue);

			if (hit)
			{
				if (hit.collider == boxCollider || collisions.rightCollider == hit.collider)
				{
					continue;
				}

				collisions.right = true;
				collisions.rightCollider = hit.collider;
			}
		}
	}

	private void VerticalCollisions()
	{
		for (int i = 0; i < verticalRayCount; i++)
		{
			//Raycast above
			Vector2 rayOrigin = raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, collisionsRayLength, collisionMask);
			Debug.DrawRay(rayOrigin, Vector2.up * collisionsRayLength, Color.green);

			if (hit)
			{
				if (hit.collider == boxCollider || collisions.aboveCollider == hit.collider)
				{
					continue;
				}

				collisions.above = true;
				collisions.aboveCollider = hit.collider;
			}

			//Raycast below
			rayOrigin = raycastOrigins.bottomLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			hit = Physics2D.Raycast(rayOrigin, Vector2.down, collisionsRayLength, collisionMask);
			Debug.DrawRay(rayOrigin, Vector2.down * collisionsRayLength, Color.black);

			if (hit)
			{
				if (hit.collider == boxCollider || collisions.belowCollider == hit.collider)
				{
					continue;
				}

				collisions.below = true;
				collisions.belowCollider = hit.collider;
			}
		}
	}

	enum State
	{
		jumping,
		diving,
		resting,
		inAir
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

	void OnDrawGizmos()
	{
		if (localWaypoints != null)
		{
			Gizmos.color = Color.red;
			float size = .3f;

			for (int i = 0; i < localWaypoints.Length; i++)
			{
				Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
				Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
				Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
			}
		}
	}
}
