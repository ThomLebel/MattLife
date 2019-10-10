using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlamingEnemy : PlatformController
{
	public float slamingSpeed;
	public float replacingSpeed;
	public GameObject dustPartices;
	public AudioSource slamSound;

	[SerializeField]
	private State state;
	private bool isActive;
	[SerializeField]
	private float activatingDistance = 10f;
	private Camera cam;
	private float camHorizontalExtend;

	[SerializeField]
	private float replaceWaitTime = 2f;
	[SerializeField]
	private float slamWaitTime = 4f;
	[SerializeField]
	private float timeBeforeNextMove;

	private Rigidbody2D rb2d;
	private Animator animator;

	protected override void Awake()
	{
		base.Awake();
		rb2d = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	}

	// Start is called before the first frame update
	protected override void Start()
    {
		base.Start();

		cam = Camera.main;
		camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;
		state = State.hasSlamed;
	}

	// Update is called once per frame
	protected override void Update()
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

		if (timeBeforeNextMove <= 0)
		{
			switch (state)
			{
				case State.hasSlamed:
					speed = replacingSpeed;
					state = State.replacing;
					break;
				case State.isReady:
					state = State.preparing;
					animator.SetTrigger("PrepareFalling");
					break;
				default:
					break;
			}
		}
		else
		{
			timeBeforeNextMove -= Time.deltaTime;
		}

		if (state == State.replacing || state == State.slaming)
		{
			Move();
		}
	}

	public void IsReadyToSlam()
	{
		speed = slamingSpeed;
		state = State.slaming;
	}

	private void Move()
	{
		Vector3 velocity = CalculatePlatformMovement();

		CalculatePassengerMovement(velocity);

		MovePassengers(true);
		transform.Translate(velocity);
		MovePassengers(false);
	}

	protected override Vector3 CalculatePlatformMovement()
	{
		fromWaypointIndex %= globalWaypoints.Length;
		int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Easing.Quadratic.In(percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

		if (percentBetweenWaypoints >= 1)
		{
			percentBetweenWaypoints = 0;
			fromWaypointIndex++;
			switch (state)
			{
				case State.slaming:
					SlamEffect();
					break;
				case State.replacing:
					ReplaceEffect();
					break;
			}

			if (!cyclic)
			{
				if (fromWaypointIndex >= globalWaypoints.Length - 1)
				{
					fromWaypointIndex = 0;
					System.Array.Reverse(globalWaypoints);
				}
			}
		}

		return newPos - transform.position;
	}

	private void SlamEffect()
	{
		state = State.hasSlamed;
		StartCoroutine(CameraShake.Instance.Shake());
		Instantiate(dustPartices, transform.position, Quaternion.identity);
		slamSound.Play();
		timeBeforeNextMove = replaceWaitTime;
	}

	private void ReplaceEffect()
	{
		state = State.isReady;
		timeBeforeNextMove = slamWaitTime;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (state != State.slaming || !collision.CompareTag("Player"))
		{
			return;
		}

		Player playerScript = collision.transform.GetComponent<Player>();
		if (playerScript.isInvulnerable)
		{
			return;
		}

		playerScript.Hit();
		playerScript.Bounce();
	}

	enum State
	{
		slaming,
		replacing,
		hasSlamed,
		isReady,
		preparing
	}
}
