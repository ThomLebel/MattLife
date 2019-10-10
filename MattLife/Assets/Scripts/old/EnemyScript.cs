/*
 
NOT USED ANYMORE !

 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
	public int score;
	public float walkSpeed = 2;

	public bool bumpOnObstacle = false;
	public LayerMask obstacleLayerMask;

	public Transform headCheck;
	public Transform wallCheck;

	public EnemySpawner spawner;
	public GameObject effect;

	public string hitSound;

	protected float speed;
	protected int direction = -1;
	protected float spriteWidth;
	protected float spriteHeight;

	protected SpriteRenderer spriteRenderer;
	protected Rigidbody2D rb2d;
	protected Animator animator;
	protected Collider2D ownCollider;

	[SerializeField]
	private float deletingDistance = 10f;
	private Camera cam;
	private float camHorizontalExtend;
	private float camVerticalExtend;

	private void Awake()
	{
		cam = Camera.main;
		spriteRenderer = GetComponent<SpriteRenderer>();
		rb2d = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		ownCollider = GetComponent<Collider2D>();
	}
	// Start is called before the first frame update
	void Start()
    {
		speed = walkSpeed;
		spriteWidth = spriteRenderer.bounds.size.x;
		spriteHeight = spriteRenderer.bounds.size.y;
		camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;
	}

    // Update is called once per frame
    void Update()
    {
		bumpOnObstacle = Physics2D.Linecast(transform.position, wallCheck.position, obstacleLayerMask);
		if (bumpOnObstacle)
		{
			Flip();
		}

		rb2d.velocity = new Vector2(direction * speed * Time.deltaTime, rb2d.velocity.y);

		//Delete monster if out of sight
		if (transform.position.x >= cam.transform.position.x + camHorizontalExtend + deletingDistance ||
			transform.position.x <= cam.transform.position.x - camHorizontalExtend - deletingDistance)
		{
			DeleteMonster();
		}
	}

	public virtual void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.transform.tag == "Player")
		{
			Player playerScript = collision.transform.GetComponent<Player>();
			if (playerScript.isInvulnerable)
			{
				return;
			}

			Vector2 contact = collision.GetContact(0).point;
			Debug.Log(contact);
			if (contact.y >= headCheck.position.y)
			{
				Debug.Log("Kill this monster !");

				Instantiate(effect, transform.position, Quaternion.identity);
				playerScript.AllowBounceOffMonster();

				Kill();
			}else
			{
				Debug.Log("Kill the player !");
				playerScript.Hit();
			}

			playerScript.Bounce();
		}
	}

	protected void Flip()
	{
		direction *= -1;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void Kill()
	{
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
}
