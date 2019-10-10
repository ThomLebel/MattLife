using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public Sound[] sounds;
	public Sound[] musics;

	public bool soundsMuted;
	public bool musicsMuted;

	public static AudioManager instance;

    void Awake()
    {
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);

		//Create sounds audio sources
		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;

			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.loop = s.loop;
			s.source.playOnAwake = s.playOnAwake;
		}

		//Create musics audio sources
		foreach (Sound m in musics)
		{
			m.source = gameObject.AddComponent<AudioSource>();
			m.source.clip = m.clip;

			m.source.volume = m.volume;
			m.source.pitch = m.pitch;
			m.source.loop = m.loop;
			m.source.playOnAwake = m.playOnAwake;
		}
	}

	public void PlaySound(string name)
	{

		Sound s = Array.Find(sounds, sound => sound.name == name);
		if (s == null)
		{
			Debug.Log("Sound : "+name+" not found !");
			return;
		}

		s.source.volume = s.volume;
		s.source.pitch = s.pitch;
		Play(s.source);
	}

	public void StopSound(string name)
	{
		Sound s = Array.Find(sounds, sound => sound.name == name);
		if (s == null)
		{
			Debug.Log("Sound : " + name + " not found !");
			return;
		}

		Stop(s.source);
	}

	public void PlayMusic(string name)
	{

		Sound m = Array.Find(musics, music => music.name == name);
		if (m == null)
		{
			Debug.Log("Music : " + name + " not found !");
			return;
		}

		m.source.volume = m.volume;
		m.source.pitch = m.pitch;
		Play(m.source);
	}

	public void PauseMusic(string name)
	{
		Sound m = Array.Find(musics, music => music.name == name);
		if (m == null)
		{
			Debug.Log("Sound : " + name + " not found !");
			return;
		}

		Pause(m.source);
	}

	public void ResumeMusic(string name)
	{
		Sound m = Array.Find(musics, music => music.name == name);
		if (m == null)
		{
			Debug.Log("Sound : " + name + " not found !");
			return;
		}

		Resume(m.source);
	}

	public void StopMusic(string name)
	{
		Sound m = Array.Find(musics, music => music.name == name);
		if (m == null)
		{
			Debug.Log("Sound : " + name + " not found !");
			return;
		}

		Stop(m.source);
	}

	public void FadeFromMusic(string name, float time, float volume = 0f)
	{
		if (time == 0f)
		{
			time = 1f;
		}

		Sound m = Array.Find(musics, music => music.name == name);
		if (m == null)
		{
			Debug.Log("Sound : " + name + " not found !");
			return;
		}

		FadeFrom(m.source, time, volume);
	}

	public void FadeToMusic(string name, float time, float volume = 0f)
	{
		if (time == 0f)
		{
			time = 1f;
		}

		Sound m = Array.Find(musics, music => music.name == name);
		if (m == null)
		{
			Debug.Log("Sound : " + name + " not found !");
			return;
		}

		FadeTo(m.source, time, volume);
	}

	private void Play(AudioSource source)
	{
		source.Play();
	}

	private void Stop(AudioSource source)
	{
		if(source != null)
			source.Stop();
	}

	private void Pause(AudioSource source)
	{
		source.Pause();
	}

	private void Resume(AudioSource source)
	{
		source.UnPause();
	}

	private void FadeFrom(AudioSource source, float time, float volume)
	{
		iTween.AudioFrom(gameObject, iTween.Hash(
			"audiosource", source,
			"volume", volume,
			"time", time,
			"ignoretimescale", true
		));
	}

	private void FadeTo(AudioSource source, float time, float volume)
	{
		Hashtable paramHashtable = new Hashtable
		{
			{ "source", source },
			{ "volume", volume }
		};

		iTween.AudioTo(gameObject, iTween.Hash(
			"audiosource", source,
			"volume", volume,
			"time", time,
			"ignoretimescale", true,
			"oncomplete", "OnFadeEnd",
			"oncompleteparams", paramHashtable
		));
	}

	private void OnFadeEnd(Hashtable tweenParam)
	{
		if ((float)tweenParam["volume"] == 0f)
		{
			Pause((AudioSource)tweenParam["source"]);
		}
	}
}
