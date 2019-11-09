using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public static class SaveSystem {

    private static readonly string path = Path.Combine(Application.persistentDataPath, "gameData.fun");

    public static void SaveGame(GameManagerScript gm)
    {
        //Debug.Log("saving to " + path);
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData data = new PlayerData(gm);

        formatter.Serialize(stream, data);
        stream.Close();
    }


    public static PlayerData LoadGame()
    {
        if (File.Exists(path))
        {

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData data = formatter.Deserialize(stream) as PlayerData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }
}
