using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoBlock : Block
{
	public Souvenir souvenir;
	//public VideoClip videoClip;
	//public Sprite videoMiniature;
	public StreamSouvenir videoPlayer;
	public GalerieScript galerie;
	[SerializeField]
	private bool blockRevealed = false;

	protected override void Awake()
	{
		base.Awake();
		type = "video";
	}

	private void Update()
	{
		if (open && !blockRevealed)
		{
			AudioManager.instance.PlaySound("SouvenirRevealed");
			Vector3 blockPosition = cam.WorldToScreenPoint(transform.position);
			blockRevealed = true;
			videoPlayer.StartVideoStream(souvenir, blockPosition);
			galerie.RevealSouvenirs(souvenir);
		}
	}
}
