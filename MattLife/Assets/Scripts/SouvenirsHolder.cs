using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SouvenirsHolder : MonoBehaviour
{
	public List<Souvenir> souvenirsList;

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}
