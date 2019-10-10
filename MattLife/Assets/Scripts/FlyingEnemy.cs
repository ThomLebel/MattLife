using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    // Start is called before the first frame update
    protected override void Start()
    {
		cam = Camera.main;

		gravity = 0f;
		maxJumpVelocity = 0f;
		minJumpVelocity = 0f;

		camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;
		directionalInput = Vector2.left;
		speed = walkSpeed;

		state = State.walking;
	}

    // Update is called once per frame
    protected override void Update()
    {
		//Delete monster if out of sight
		if (transform.position.x >= cam.transform.position.x + camHorizontalExtend + deletingDistance ||
			transform.position.x <= cam.transform.position.x - camHorizontalExtend - deletingDistance)
		{
			DeleteMonster();
		}

		CalculateVelocity();

		controller.Move(velocity * Time.deltaTime, directionalInput);

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
}
