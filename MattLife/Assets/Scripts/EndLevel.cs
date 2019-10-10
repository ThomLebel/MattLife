using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevel : MonoBehaviour
{
	public Vector3 localWaypoint;
	private Vector3 globalWaypoint;

	public GameObject board;
	private Tutorial boardScript;
	public float hideMessageTimer = 5f;
	public float displayPowerTimer = 1f;
	public float levelTransitionTimer = 3f;

	public string nextScene;
	public float transitionTime = 1.5f;
	
	private Animator screenRevealAnimator;
	private GameObject player;
	private Player playerScript;
	private Animator playerAnim;

	private bool animationStarted = false;
	private bool hasReachEndLevel = false;
	private bool hasJump = false;
	private IEnumerator hideMessageCoroutine, displayPowerCoroutine;

	private void Start()
	{
		screenRevealAnimator = GameObject.FindGameObjectWithTag("ScreenReveal").GetComponent<Animator>();
		boardScript = board.GetComponent<Tutorial>();

		globalWaypoint = localWaypoint + transform.position;
	}

	private void Update()
	{
		if (!animationStarted || player == null)
		{
			return;
		}

		if (player.transform.position.x < globalWaypoint.x)
		{
			playerScript.SetDirectionalInput(Vector2.right);
		}
		else if (player.transform.position.x > board.transform.position.x)
		{
			playerScript.SetDirectionalInput(Vector2.left);
		}
		else
		{
			animationStarted = false;
			playerScript.SetDirectionalInput(Vector2.zero);

			hideMessageCoroutine = HideMessage(hideMessageTimer);
			StartCoroutine(hideMessageCoroutine);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (hasReachEndLevel)
		{
			return;
		}

		if (collision.CompareTag("Player"))
		{
			hasReachEndLevel = true;

			player = collision.transform.gameObject;
			playerScript = player.GetComponent<Player>();
			playerAnim = player.GetComponent<Animator>();
			player.GetComponent<PlayerInput>().enabled = false;

			animationStarted = true;
		}
	}

	IEnumerator HideMessage(float time)
	{
		yield return new WaitForSeconds(time);
		StopCoroutine(hideMessageCoroutine);

		boardScript.HideMessage();
		Destroy(board);
		playerAnim.SetTrigger("transform");
		AudioManager.instance.PlaySound("PlayerTransform");

		displayPowerCoroutine = DisplayPower(displayPowerTimer);
		StartCoroutine(displayPowerCoroutine);
	}

	IEnumerator DisplayPower(float time)
	{
		yield return new WaitForSeconds(time);
		StopCoroutine(displayPowerCoroutine);
		
		int currentLevel = GameMaster.Instance.levelIndex;
		int nextLevel = currentLevel + 1;

		playerScript.SwapBody(nextLevel);

		switch (currentLevel)
		{
			case 0:
				playerScript.doubleJumpPower = true;
				DoubleJump();
				break;
			case 1:
				playerScript.shootPower = true;
				Shoot();
				break;
			default:
				break;
		}

		StartCoroutine("LevelTransition");
	}

	public void DoubleJump()
	{
		playerScript.Jump(new Vector2(0f, playerScript.maxJumpVelocity));

		if(!hasJump)
			Invoke("DoubleJump", 0.5f);

		hasJump = true;
	}

	public void Shoot()
	{
		playerScript.Shoot();
	}

	IEnumerator LevelTransition()
	{
		yield return new WaitForSeconds(levelTransitionTimer);

		AudioManager.instance.FadeToMusic(GameMaster.Instance.musicName, transitionTime);
		StartCoroutine("OnCompleteScreenReavealStartAnimation");
		screenRevealAnimator.SetTrigger("End");
	}

	IEnumerator OnCompleteScreenReavealStartAnimation()
	{
		yield return new WaitForSeconds(transitionTime + 0.2f);

		// TODO: Do something when animation did complete
		LoadNextLevel();
	}

	public void LoadNextLevel()
	{
		SceneManager.LoadScene(nextScene);
	}

	void OnDrawGizmos()
	{
		if (localWaypoint != null)
		{
			Gizmos.color = Color.red;
			float size = .3f;

			Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoint : localWaypoint + transform.position;
			Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
			Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
		}
	}
}
