﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitAfterCredit : MonoBehaviour
{
	private void OnEnable()
	{
		Application.Quit();
	}
}