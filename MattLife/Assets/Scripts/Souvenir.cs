using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Souvenir")]
public class Souvenir : ScriptableObject
{
	public int id;
	public Sprite photo;
	public VideoClip video;
	public bool revealed = false;
}
