/*
 
NOT USED ANYMORE !

 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingEnemy : MonoBehaviour
{
	public Vector3[] localWaypoints;
	[SerializeField]
	private Vector3[] globalWaypoints;

	public float fallingTime;
	public float ascendTime;
	public GameObject dustPartices;

	[SerializeField]
	private State state;
	private bool isActive;
	[SerializeField]
	private float activatingDistance = 10f;
	private Camera cam;
	private float camHorizontalExtend;
	private Vector3 velocity;

	[SerializeField] private float ascendWaitTime = 2f;
	[SerializeField] private float fallWaitTime = 4f;
	[SerializeField] private float timeBeforeNextMove;
	
	private Animator animator;

	void Awake()
	{
		animator = GetComponent<Animator>();
	}

	// Start is called before the first frame update
	void Start()
    {
		cam = Camera.main;

		camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;

		globalWaypoints = new Vector3[localWaypoints.Length];
		for (int i = 0; i < localWaypoints.Length; i++)
		{
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}

		state = State.onGround;
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

		if (timeBeforeNextMove <= 0)
		{
			switch (state)
			{
				case State.onGround:
					Ascend();
					break;
				case State.inAir:
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
	}

	public void Fall()
	{
		state = State.falling;
		iTween.MoveTo(gameObject, iTween.Hash(
			"position", globalWaypoints[0],
			"time", fallingTime,
			"easetype", "easeInQuad",
			"oncomplete", "SlamEffect"
		));
	}

	private void SlamEffect()
	{
		state = State.onGround;
		StartCoroutine(CameraShake.Instance.Shake());
		Instantiate(dustPartices, transform.position, Quaternion.identity);
		timeBeforeNextMove = ascendWaitTime;
	}

	private void Ascend()
	{
		state = State.ascending;
		iTween.MoveTo(gameObject, iTween.Hash(
			"position", globalWaypoints[1],
			"time", ascendTime,
			"easetype", "easeInQuad",
			"oncomplete", "AirEffect"
		));
	}

	private void AirEffect()
	{
		state = State.inAir;
		timeBeforeNextMove = fallWaitTime;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (state != State.falling || collision.tag != "Player")
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
		falling,
		ascending,
		onGround,
		inAir,
		preparing
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
