using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// jess @ 11/12/2025

//<summary>
// This script creates an interface called ISaveable that defines methods for saving and loading game data. it works based on inheritance.
// it can be used in a script by adding it next to "class MyClass : MonoBehaviour, ISaveable"
// both methods must be implemented in the class that uses this interface.
//</summary>
public interface ISaveable
{
    void PopulateSaveData(GameData data);
    void LoadFromSaveData(GameData data);
}