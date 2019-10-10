using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent (typeof(Controller2D))]
public class Player : MonoBehaviour
{
	public string currentLevel = "";
	public int playerLife = 5;
	public int playerScore = 0;
	public int playerGold = 0;

	public float moveSpeed = 6;
	public float maxJumpHeight = 5;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public float accelerationTimeAirborne = .2f;
	public float accelerationTimeGrounded = .1f;
	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	public float bounceTakeOff = 7f;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

	public bool doubleJumpPower = false;
	public bool shootPower = false;

	public bool isInvulnerable = false;
	public bool isFalling = false;

	public Transform jumpParticlePosition;
	public Transform projectilePosition;
	public GameObject lifeLostParticle;
	public GameObject jumpParticle;
	public GameObject projectile;

	public RuntimeAnimatorController[] bodiesControllers;
	public GameObject[] bodies;

	[SerializeField]
	private float fireRate = 1f;
	private float timeBeforeNextShot;

	private float gravity;
	[SerializeField]
	private float fallingGravity = 0.5f;
	public float maxJumpVelocity;
	private float minJumpVelocity;
	private float timeToWallUnstick;
	private Vector3 velocity;
	private float velocityXSmoothing;
	private bool wallSliding;
	private int wallDirX;
	private bool facingRight = true;
	private int previousIndex = 0;

	[SerializeField]
	private bool shooting = false;
	[SerializeField]
	private bool inAir = false;
	[SerializeField]
	private bool canBounce = false;
	[SerializeField]
	private bool bouncing = false;
	[SerializeField]
	private bool canDoubleJump = false;
	[SerializeField]
	private bool hasDoubleJumped = false;
	[SerializeField]
	private float bounceTimerWindow = 0.2f;
	private IEnumerator bounceOnMonster;
	[SerializeField]
	private float invulnerabilityTime = 1.5f;
	private IEnumerator playerIsInvulnerable;

	private Vector2 directionalInput;
	private Controller2D controller;
	private Animator animator;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		controller = GetComponent<Controller2D>();
		animator = GetComponent<Animator>();
	}

	void Start()
	{
		gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
		SceneManager.activeSceneChanged += ChangedActiveScene;
	}

	private void ChangedActiveScene(Scene current, Scene next)
	{
		currentLevel = next.name;
	}

	void Update()
	{
		CalculateVelocity();
		HandleWallSliding();

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
			canDoubleJump = false;
		}
		if (velocity.y <= 0 && bouncing)
		{
			bouncing = false;
		}
		if (isFalling)
		{
			if (controller.collisions.below)
			{
				isFalling = false;
			}
			else
			{
				velocity.y = -fallingGravity;
			}			
		}

		if (!controller.collisions.below)
		{
			inAir = true;
			animator.SetBool("isJumping", true);
			if (!canDoubleJump && !hasDoubleJumped)
			{
				canDoubleJump = true;
			}
		}
		if (controller.collisions.below && inAir)
		{
			inAir = false;
			hasDoubleJumped = false;
			animator.SetBool("isJumping", false);
			Instantiate(jumpParticle, jumpParticlePosition.position, Quaternion.identity);
			AudioManager.instance.PlaySound("PlayerLand");
		}

		if (timeBeforeNextShot <= 0)
		{
			if (shooting)
			{
				Shoot();

				timeBeforeNextShot = fireRate;
			}
		}
		else
		{
			timeBeforeNextShot -= Time.deltaTime;
		}

		if (directionalInput == Vector2.zero)
		{
			animator.SetBool("isMoving", false);
		}
		else
		{
			animator.SetBool("isMoving", true);
		}

		if (!facingRight && directionalInput.x > 0)
		{
			Flip();
		}
		else if (facingRight && directionalInput.x < 0)
		{
			Flip();
		}
	}

	public void SetDirectionalInput(Vector2 input)
	{
		directionalInput = input;
	}

	public void OnJumpInputDown()
	{
		Vector2 jumpVelocity = velocity;

		if (wallSliding)
		{
			if (wallDirX == directionalInput.x)
			{
				jumpVelocity.x = -wallDirX * wallJumpClimb.x;
				jumpVelocity.y = wallJumpClimb.y;
			}
			else if (directionalInput.x == 0)
			{
				jumpVelocity.x = -wallDirX * wallJumpOff.x;
				jumpVelocity.y = wallJumpOff.y;
			}
			else
			{
				jumpVelocity.x = -wallDirX * wallLeap.x;
				jumpVelocity.y = wallLeap.y;
			}
		}
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
		if (doubleJumpPower && canDoubleJump && !hasDoubleJumped)
		{
			hasDoubleJumped = true;
			jumpVelocity.y = maxJumpVelocity;
		}
		if (canBounce)
		{
			bouncing = true;
			canBounce = false;
			jumpVelocity.y = maxJumpVelocity;
		}

		if (jumpVelocity != (Vector2)velocity)
		{
			Jump(jumpVelocity);
		}
	}

	public void Jump(Vector2 jumpVelocity)
	{
		velocity = jumpVelocity;
		JumpEffect();
	}

	private void JumpEffect()
	{
		animator.SetTrigger("takeOff");
		Instantiate(jumpParticle, jumpParticlePosition.position, Quaternion.identity);
		AudioManager.instance.PlaySound("PlayerJump");
	}

	public void OnJumpInputUp()
	{
		if (velocity.y > minJumpVelocity && !bouncing)
		{
			velocity.y = minJumpVelocity;
		}
		if (!canDoubleJump)
		{
			canDoubleJump = true;
		}
		else
		{
			canDoubleJump = false;
		}
	}

	public void OnFireInputDown()
	{
		if (!shootPower)
		{
			return;
		}

		shooting = true;
	}

	public void OnFireInputUp()
	{
		shooting = false;
	}

	public void Shoot()
	{
		animator.SetTrigger("shoot");
		AudioManager.instance.PlaySound("PlayerShoot");

		GameObject shot = Instantiate(projectile, projectilePosition.position, Quaternion.identity);

		Projectile pScript = shot.GetComponent<Projectile>();

		if (facingRight)
			pScript.direction = Vector2.right;
		else
			pScript.direction = Vector2.left;
	}

	public void StopMovement()
	{
		velocity = Vector3.zero;
	}

	void CalculateVelocity()
	{
		float targetVelocityX = directionalInput.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}

	void HandleWallSliding()
	{
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
		{
			wallSliding = true;
			Instantiate(jumpParticle, jumpParticlePosition.position, Quaternion.identity);
			//AudioManager.instance.PlaySound("PlayerJump");

			if (velocity.y < -wallSlideSpeedMax)
			{
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0)
			{
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (directionalInput.x != wallDirX && directionalInput.x != 0)
				{
					timeToWallUnstick -= Time.deltaTime;
				}
				else
				{
					timeToWallUnstick = wallStickTime;
				}
			}
			else
			{
				timeToWallUnstick = wallStickTime;
			}
		}
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void Bounce()
	{
		velocity.y = bounceTakeOff;
		Jump(velocity);
	}

	public void AllowBounceOffMonster()
	{
		canBounce = true;

		bounceOnMonster = BounceOnMonster(bounceTimerWindow);
		StartCoroutine(bounceOnMonster);
	}

	public void Hit()
	{
		if (isInvulnerable)
		{
			return;
		}
		int rand = Random.Range(0,2);
		if (rand == 0)
		{
			AudioManager.instance.PlaySound("PlayerHit1");
		}
		else
		{
			AudioManager.instance.PlaySound("PlayerHit2");
		}

		StartCoroutine(CameraShake.Instance.Shake());
		Instantiate(lifeLostParticle, transform.position, Quaternion.identity);
		GameMaster.Instance.UpdateLife(-1);
		ActivateInvulnerability();
	}

	public void ActivateInvulnerability()
	{
		isInvulnerable = true;
		//Prevent player to collide with enemies
		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemies"), true);
		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("MovingShells"), true);
		controller.collisionMask &= ~(1 << LayerMask.NameToLayer("Enemies"));
		controller.collisionMask &= ~(1 << LayerMask.NameToLayer("MovingShells"));
		animator.SetBool("PlayerInvulnerable", isInvulnerable);
		playerIsInvulnerable = PlayerInvulnerable(invulnerabilityTime);
		StartCoroutine(playerIsInvulnerable);
	}

	public void Fall()
	{
		isFalling = true;
		animator.SetTrigger("takeOff");
		animator.SetBool("isJumping", true);
		transform.position = new Vector3(transform.position.x, 10f, transform.position.z);
	}

	public void SwapBody(int index)
	{
		if (index == previousIndex)
		{
			return;
		}

		bodies[previousIndex].SetActive(false);
		animator.runtimeAnimatorController = bodiesControllers[index] as RuntimeAnimatorController;
		bodies[index].SetActive(true);

		previousIndex = index;
	}

	public void PlayWalkSound()
	{
		AudioManager.instance.PlaySound("PlayerWalk");
	}

	private IEnumerator BounceOnMonster(float time)
	{
		yield return new WaitForSeconds(time);
		canBounce = false;
		StopCoroutine(bounceOnMonster);
	}

	private IEnumerator PlayerInvulnerable(float time)
	{
		yield return new WaitForSeconds(time);
		isInvulnerable = false;
		isFalling = false;
		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemies"), false);
		Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("MovingShells"), false);
		controller.collisionMask |= (1 << LayerMask.NameToLayer("Enemies"));
		controller.collisionMask |= (1 << LayerMask.NameToLayer("MovingShells"));
		animator.SetBool("PlayerInvulnerable", isInvulnerable);
		StopCoroutine(playerIsInvulnerable);
	}
}
