using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Sirenix.Serialization;

public class GameDataFileHandler
{
    private const string PlayerDataFileName = "/PlayerData.save";
    private const string WorldDataFileName = "/WorldData.save";
    private const string SettingsDataFileName = "/SettingsData.json";
    
    public static void WritePlayerSaveFile(PlayerSaveWrapper data, ScriptableObjectReferenceCache cache)
    {
        var context = new SerializationContext
        {
            StringReferenceResolver = cache
        };

        byte[] bytes = SerializationUtility.SerializeValue(data, DataFormat.Binary, context);
        File.WriteAllBytes(Application.persistentDataPath + PlayerDataFileName, bytes);
        Debug.Log("Saved playerData wrapper.");
    }

    public static void LoadPlayerSaveFile(out PlayerSaveWrapper data, ScriptableObjectReferenceCache cache)
    {
        if(File.Exists(Application.persistentDataPath + PlayerDataFileName))
        {
            var context = new DeserializationContext
            {
                StringReferenceResolver = cache
            };
            byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + PlayerDataFileName);
            data = SerializationUtility.DeserializeValue<PlayerSaveWrapper>(bytes, DataFormat.Binary, context);
            Debug.Log("Loaded playerData wrapper.");
        }
        else
        {
            data = null;
            Debug.Log("No save file found.");
        }
    }

    public static bool LoadWorldData(out WorldData data, ScriptableObjectReferenceCache cache)
    {
        if(File.Exists(Application.persistentDataPath + WorldDataFileName))
        {
            var context = new DeserializationContext
            {
                StringReferenceResolver = cache
            };

            byte[] bytes = File.ReadAllBytes(Application.persistentDataPath + WorldDataFileName);
            data = SerializationUtility.DeserializeValue<WorldData>(bytes, DataFormat.Binary, context);
            Debug.Log("Loaded worldData file.");
            return true;
        }
        else
        {
            Debug.Log("No worldData file found.");
            data = null;
            return false;
        }
    }
    
    public static void WriteWorldData(WorldData data, ScriptableObjectReferenceCache cache)
    {
        var context = new SerializationContext
        {
            StringReferenceResolver = cache
        };
        byte[] bytes = SerializationUtility.SerializeValue<WorldData>(data, DataFormat.Binary, context);
        File.WriteAllBytes(Application.persistentDataPath + WorldDataFileName, bytes);
        Debug.Log("Saved worldData.");
    }

    /// <summary>
    /// Save SettingsData to JSON file.
    /// </summary>
    /// <param name="data"></param>
    public static void WriteSettingsData(SettingsData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + SettingsDataFileName, json);
        Debug.Log("Saved settingsData.");
    }

    /// <summary>
    /// Checks if a settingsData file exists and loads it if it does.
    /// </summary>
    /// <param name="data">Outputs SettingsData if a file was found.</param>
    /// <returns>Returns if a file was found.</returns>
    public static bool LoadSettingsData(out SettingsData data)
    {
        if(File.Exists(Application.persistentDataPath + SettingsDataFileName))
        {
            string json = File.ReadAllText(Application.persistentDataPath + SettingsDataFileName);
            data = JsonUtility.FromJson<SettingsData>(json);
            Debug.Log("Loaded settingsData file.");
            return true;
        }

        data = null;
        Debug.Log("No save file found.");
        return false;
    }
    
    /// <summary>
    /// Checks if a save file exists.
    /// </summary>
    /// <returns></returns>
    public static bool HasPlayerSaveFile()
    {
        return File.Exists(Application.persistentDataPath + PlayerDataFileName);
    }
}
