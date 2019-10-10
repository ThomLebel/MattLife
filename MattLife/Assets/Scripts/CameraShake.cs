using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
	public float duration = 0.15f;
	public float magnitude = 0.2f;

	public static CameraShake Instance;

	//private Vector3 originalPos;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		//originalPos = transform.localPosition;
	}

	public IEnumerator Shake()
	{
		Vector3 originalPos = transform.localPosition;

		float elapsed = 0.0f;
		while (elapsed < duration)
		{
			float x = Random.Range(-1f,1) * magnitude;
			float y = Random.Range(-1f,1) * magnitude;

			transform.localPosition = new Vector3(x,y,originalPos.z);

			elapsed += Time.unscaledDeltaTime;

			yield return null;
		}

		transform.localPosition = originalPos;
	}
}
