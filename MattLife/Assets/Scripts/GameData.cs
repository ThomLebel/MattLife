using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
	//Player data
	public int lives;
	public int gold;
	public int score;

	//Level data
	public string currentLevel;

	//Souvenirs data
	public bool[] souvenirsRevealed;

	public GameData(Player player, SouvenirsHolder souvenirsHolder)
	{
		//Player data
		lives = player.playerLife;
		gold = player.playerGold;
		score = player.playerScore;

		//Level data
		currentLevel = player.currentLevel;

		//Souvenirs data
		souvenirsRevealed = new bool[souvenirsHolder.souvenirsList.Count];
		for(int i = 0; i < souvenirsHolder.souvenirsList.Count; i++)
		{
			Souvenir souvenir = souvenirsHolder.souvenirsList[i];
			souvenirsRevealed[i] = souvenir.revealed;
		}
	}
}
