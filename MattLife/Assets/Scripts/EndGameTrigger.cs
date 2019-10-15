using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameTrigger : MonoBehaviour
{
	public Vector3 localWaypoint;
	private Vector3 globalWaypoint;

	public GameObject board;
	public GameObject[] messages;
	private Tutorial boardScript;
	private int currentMsg = 0;

	public string nextScene;
	public float transitionTime = 1.5f;
	public float levelTransitionTimer = 3f;
	public float hideMessageTimer = 5f;

	private bool animationStarted = false;
	private bool hasReachEndLevel = false;
	private IEnumerator hideMessageCoroutine, displayPowerCoroutine;

	private Animator screenRevealAnimator;
	private GameObject player;
	private Player playerScript;
	private Animator playerAnim;


	// Start is called before the first frame update
	void Start()
    {
		screenRevealAnimator = GameObject.FindGameObjectWithTag("ScreenReveal").GetComponent<Animator>();
		boardScript = board.GetComponent<Tutorial>();

		globalWaypoint = localWaypoint + transform.position;
	}

    // Update is called once per frame
    void Update()
    {
		if (!animationStarted || player == null)
		{
			return;
		}

		if (player.transform.position.x < globalWaypoint.x)
		{
			playerScript.SetDirectionalInput(Vector2.right);
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
		
		if (currentMsg < messages.Length)
		{
			boardScript.SetMessage(messages[currentMsg]);
			boardScript.DisplayMessage();
			currentMsg++;

			hideMessageCoroutine = HideMessage(hideMessageTimer);
			StartCoroutine(hideMessageCoroutine);
		}
		else
		{
			Destroy(board);
			StartCoroutine("LevelTransition");
		}
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
