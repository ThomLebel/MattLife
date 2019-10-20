using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneToCredit : MonoBehaviour
{
	private void OnEnable()
	{
		SceneManager.LoadSceneAsync("CreditScene");
	}
}
