using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anthiony - 8/2/2026

/// <summary>
/// Component placed on decoration prefabs.
/// Stores tag data and registers/unregisters the decoration with DecorRegistry.
/// </summary>
public class Decoration : MonoBehaviour
{
    [Header("Decor Settings")]
    public DecorTag tags = DecorTag.None;

    [Tooltip("Optional: some decor is worth slightly more than others")]
    public float base_value = 1f;

    [HideInInspector] public Vector2Int grid_cell;

    private void OnEnable()
    {
        DecorRegistry.Instance?.Register(this);
    }

    private void OnDisable()
    {
        DecorRegistry.Instance?.Unregister(this);
    }
}

