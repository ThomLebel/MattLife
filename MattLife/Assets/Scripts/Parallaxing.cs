using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxing : MonoBehaviour
{
	public Transform[] backgrounds;         // Array (list) of all the back- and foregrounds to be parallaxed
	private float[] parallaxScales;         // The proportion of the camera's movement to move the backgrounds by
	public float smoothing = 1f;            // How smooth the parallax is going to be. Make this above 0

	private Transform cam;                  // Reference to the main camera
	private Vector3 previousCamPosition;    // Store the position of the camera in the previous frame


	// Start is called before the first frame update
	void Start()
    {
		cam = Camera.main.transform;

		previousCamPosition = cam.position;

		parallaxScales = new float[backgrounds.Length];

		for (int i=0; i<backgrounds.Length; i++)
		{
			parallaxScales[i] = backgrounds[i].position.z*-1;
		}
    }

    // Update is called once per frame
    void Update()
    {
		for (int i = 0; i < backgrounds.Length; i++)
		{
			float parallax = (previousCamPosition.x - cam.position.x) * parallaxScales[i];

			float backgroundTargetPositionX = backgrounds[i].position.x + parallax;

			Vector3 backgroundTargetPosition = new Vector3(backgroundTargetPositionX, backgrounds[i].position.y, backgrounds[i].position.z);

			backgrounds[i].position = Vector3.Lerp(backgrounds[i].position, backgroundTargetPosition, smoothing * Time.deltaTime);
		}

		previousCamPosition = cam.position;
	}
}
