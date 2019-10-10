/*
 
NOT USED ANYMORE !

 */



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Controller2D target;
	public float verticalOffset;
	public float lookAheadDistanceX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;
	public Vector2 focusAreaSize;

	private FocusArea focusArea;
	private float currentLookAheadX;
	private float targetLookAheadX;
	private float lookAheadDirectionX;
	private float smoothLookVelocityX;
	private float smoothLookVelocityY;

	private bool lookAheadStopped;

	public bool bounds;
	public Transform minCameraPos;
	public Transform maxCameraPos;
	private float camHorizontalExtend;
	private float camVerticalExtend;

	protected Camera cam;

	void Start()
	{
		cam = Camera.main;
		focusArea = new FocusArea(target.boxCollider.bounds, focusAreaSize);
		camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;
		camVerticalExtend = cam.orthographicSize;
	}

	private void LateUpdate()
	{
		focusArea.Update(target.boxCollider.bounds);

		Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;

		if (focusArea.velocity.x != 0)
		{
			lookAheadDirectionX = Mathf.Sign(focusArea.velocity.x);
			if (Mathf.Sign(target.playerInput.x) == Mathf.Sign(focusArea.velocity.x) && target.playerInput.x != 0)
			{
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirectionX * lookAheadDistanceX;
			}
			else
			{
				if (!lookAheadStopped)
				{
					lookAheadStopped = true;
					targetLookAheadX = currentLookAheadX + (lookAheadDirectionX * lookAheadDistanceX - currentLookAheadX) / 4f;
				}
			}
		}
		
		currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

		focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothLookVelocityY, verticalSmoothTime);
		focusPosition += Vector2.right * currentLookAheadX;
		transform.position = (Vector3)focusPosition + Vector3.forward * -10;

		if (bounds)
		{
			transform.position = new Vector3(Mathf.Clamp(transform.position.x, minCameraPos.position.x + camHorizontalExtend, maxCameraPos.position.x - camHorizontalExtend),
											 Mathf.Clamp(transform.position.y, minCameraPos.position.y + camVerticalExtend, maxCameraPos.position.y - camVerticalExtend),
											 Mathf.Clamp(transform.position.z, minCameraPos.position.z, maxCameraPos.position.z));
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(1,0,0,0.5f);
		Gizmos.DrawCube(focusArea.center, focusAreaSize);
	}

	struct FocusArea
	{
		public Vector2 velocity;
		public Vector2 center;
		float left, right;
		float top, bottom;

		public FocusArea(Bounds targetBounds, Vector2 size)
		{
			velocity = Vector2.zero;
			left = targetBounds.center.x - size.x / 2;
			right = targetBounds.center.x + size.x / 2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			center = new Vector2((left + right)/2, (top + bottom)/2);
		}

		public void Update(Bounds targetBounds)
		{
			float shiftX = 0;
			if (targetBounds.min.x < left)
			{
				shiftX = targetBounds.min.x - left;
			}else if (targetBounds.max.x > right)
			{
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if (targetBounds.min.y < bottom)
			{
				shiftY = targetBounds.min.y - bottom;
			}
			else if (targetBounds.max.y > top)
			{
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;

			center = new Vector2((left + right) / 2, (top + bottom) / 2);
			velocity = new Vector2(shiftX, shiftY);
		}
	}
}
