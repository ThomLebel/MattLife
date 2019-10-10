using System;
using UnityEngine;

namespace UnityStandardAssets._2D
{
    public class Camera2DFollow : MonoBehaviour
    {
        public Transform target;
        public float damping = 1;
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0.5f;
        public float lookAheadMoveThreshold = 0.1f;

        private float m_OffsetZ;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_CurrentVelocity;
        private Vector3 m_LookAheadPos;

		public bool bounds;
		public Transform minCameraPos;
		public Transform maxCameraPos;
		private float camHorizontalExtend;
		private float camVerticalExtend;

		// Use this for initialization
		private void Start()
        {
			target = GameObject.FindGameObjectWithTag("Player").transform;
            m_LastTargetPosition = target.position;
            m_OffsetZ = (transform.position - target.position).z;
            transform.parent = null;

			camHorizontalExtend = Camera.main.orthographicSize * Screen.width / Screen.height;
			camVerticalExtend = Camera.main.orthographicSize;
		}


        // Update is called once per frame
        private void Update()
        {
			if (target == null)
			{
				return;
			}
            // only update lookahead pos if accelerating or changed direction
            float xMoveDelta = (target.position - m_LastTargetPosition).x;

            bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

            if (updateLookAheadTarget)
            {
                m_LookAheadPos = lookAheadFactor*Vector3.right*Mathf.Sign(xMoveDelta);
            }
            else
            {
                m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime*lookAheadReturnSpeed);
            }

            Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward*m_OffsetZ;
            Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

            transform.position = newPos;

			if (bounds)
			{
				transform.position = new Vector3(Mathf.Clamp(transform.position.x, minCameraPos.position.x + camHorizontalExtend, maxCameraPos.position.x - camHorizontalExtend),
												 Mathf.Clamp(transform.position.y, minCameraPos.position.y + camVerticalExtend, maxCameraPos.position.y - camVerticalExtend),
												 Mathf.Clamp(transform.position.z, minCameraPos.position.z, maxCameraPos.position.z));
			}

			m_LastTargetPosition = target.position;
        }
    }
}
