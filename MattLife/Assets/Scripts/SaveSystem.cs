using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
   public static void SaveGame(Player player, SouvenirsHolder souvenirsHolder)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		string path = Application.persistentDataPath + "/game.wedding";
		FileStream stream = new FileStream(path, FileMode.Create);

		GameData data = new GameData(player, souvenirsHolder);

		formatter.Serialize(stream, data);
		stream.Close();

		Debug.Log("Game saved");
	}

	public static GameData LoadGame()
	{
		string path = Application.persistentDataPath + "/game.wedding";
		if (File.Exists(path))
		{
			BinaryFormatter formatter = new BinaryFormatter();

			using (FileStream stream = new FileStream(path, FileMode.Open))
			{
				GameData data = formatter.Deserialize(stream) as GameData;

				return data;
			}
		}
		else
		{
			Debug.LogError("Save file not found in "+path);
			return null;
		}
	}
}
