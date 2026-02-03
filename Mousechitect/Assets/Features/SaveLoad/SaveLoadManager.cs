using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UImGui;

// jess @ 12/12/2025

//<summary>
// singleton manager class that handles savi g and loading 
// orchestrates datta collection, serialization, encryption, file IO
//</summary>

public class SaveLoadManager : MonoBehaviour
{
    // singleton instance global access
    public static SaveLoadManager Instance { get; private set; }

    public string save_file_name = "save.mouse";

    private string save_file_path;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // persist across scenes, must be attached to gameobject at root for dontdestroyonload to work
            DontDestroyOnLoad(gameObject);

            // persistent data path unique to each users application and os
            save_file_path = System.IO.Path.Combine(Application.persistentDataPath, save_file_name);
        }
        else if (Instance != this)
        {
            // ensure only one instance exists
            Destroy(gameObject);
        }   
    }

    private void Start()
    {
        SaveLoadConsoleCommands();
    }

    //<summary>
    // executes the save pipeline - data collection, serialization, encryption, file IO
    // </summary>
    public void SaveGame()
    {
        // initialise container for game data
        GameData data = new GameData();

        // find all active mono behaviours that implement ISaveable
        ISaveable[] saveable_objects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();

        // delegates data collection to each saveable system
        foreach (ISaveable saveable in saveable_objects)
        {
            saveable.PopulateSaveData(data);
        }

        // serialize game data to json
        string json_text = JsonUtility.ToJson(data);

        // encrypt json
        string encrypted_text = SaveEncryption.EncryptString(json_text);

        // write encrypted string to file
        try
        {
            File.WriteAllText(save_file_path, encrypted_text);
            DebugWindow.LogToConsole("Items Saved:" + json_text);
        }
        catch (IOException e)
        {
            Debug.LogError("failed to save game" + e.Message);
        }
    }

    // <summary>
    // executes load pipeline, inverse of save pipeline
    // </summary>
    public void LoadGame()
    {
        if (!File.Exists(save_file_path))
        {
            Debug.LogWarning("no save file found at" + save_file_path);
            return;
        }

        // read encrypted string from file
        string encrypted_text;
        try
        {
            encrypted_text = File.ReadAllText(save_file_path);
        }
        catch (IOException e)
        {
            Debug.LogError("failed to read savee file" + e.Message);
            return;
        }

        // decrypt json
        string json_text = SaveEncryption.DecryptString(encrypted_text);

        // deserialize json to game data
        GameData data = JsonUtility.FromJson<GameData>(json_text);

        // find all active mono behaviours that implement ISaveable
        ISaveable[] saveable_objects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();

        // delegate data loading to each saveable system
        foreach (ISaveable saveable in saveable_objects)
        {
            saveable.LoadFromSaveData(data);
        }
        DebugWindow.LogToConsole("Items Loaded:" + json_text);
    }

    private void SaveLoadConsoleCommands()
    {
        DebugWindow.Instance.RegisterExternalCommand("save", " - Executes the save pipeline and displays all items saved.", args => SaveGame());
        DebugWindow.Instance.RegisterExternalCommand("load", " - Executes the load pipeline and displays all items loaded.", args => LoadGame());
        DebugWindow.Instance.RegisterExternalCommand("save.dump", " - Decrypts and prints the current save file to console.", (args) => {
            if (File.Exists(save_file_path))
            {
                string encrypted = File.ReadAllText(save_file_path);
                string decrypted = SaveEncryption.DecryptString(encrypted);
                Debug.Log("<color=yellow>DECRYPTED FILE CONTENT:</color>\n" + decrypted);
            }
            else
            {
                Debug.Log("No save file found.");
            }
        });
    }
}
