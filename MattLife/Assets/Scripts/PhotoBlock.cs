using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoBlock : Block
{
	public Souvenir[] souvenirs;
	//public Sprite photo;
	public StreamSouvenir photoPlayer;
	public GalerieScript galerie;
	[SerializeField]
	private bool blockRevealed = false;

	protected override void Awake()
	{
		base.Awake();
		type = "photo";
	}

	private void Update()
	{
		if (open && !blockRevealed)
		{
			AudioManager.instance.PlaySound("SouvenirRevealed");
			Vector3 blockPosition = cam.WorldToScreenPoint(transform.position);
			blockRevealed = true;
			photoPlayer.StartPhotoStream(souvenirs, blockPosition);
			foreach (Souvenir souvenir in souvenirs)
			{
				galerie.RevealSouvenirs(souvenir);
			}
		}
	}
}
