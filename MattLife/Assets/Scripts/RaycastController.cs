using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
	public LayerMask collisionMask;

	[SerializeField]
	protected const float skinWidth = 0.025f;
	[SerializeField]
	protected const float distBetweenRays = 0.25f;

	[HideInInspector] public int horizontalRayCount = 4;
	[HideInInspector] public int verticalRayCount = 4;

	[HideInInspector] public float horizontalRaySpacing;
	[HideInInspector] public float verticalRaySpacing;
	[HideInInspector] public RaycastOrigins raycastOrigins;

	[HideInInspector]
	public BoxCollider2D boxCollider;

	protected virtual void Awake()
	{
		boxCollider = GetComponent<BoxCollider2D>();
	}

	protected virtual void Start()
	{
		CalculateRaySpacing();
	}

	public void UpdateRaycastOrigins()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand(skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}

	public void CalculateRaySpacing()
	{
		Bounds bounds = boxCollider.bounds;
		bounds.Expand(skinWidth * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		horizontalRayCount = Mathf.RoundToInt(boundsHeight / distBetweenRays);
		verticalRayCount = Mathf.RoundToInt(boundsWidth / distBetweenRays);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	public struct RaycastOrigins
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
