using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony - 24/2/2026
/// <summary>
/// Handles temporary visual feedback for a building while it is being placed.
/// This version mirrors DestroyTool behaviour:
/// Finds renderers dynamically (so it works even if visuals are spawned after Awake)
/// Stores original colours per renderer/material
/// Applies a solid tint (blue/red) while keeping the current alpha
/// </summary>
public class BuildingPreviewVisual : MonoBehaviour
{
    private Renderer[] renderers;
    private Color[][] original_colors;
    private bool has_cached = false;

    private void Awake()
    {
        has_cached = false;
    }

    private void CacheRenderersAndColors()
    {
        renderers = GetComponentsInChildren<Renderer>(true);

        original_colors = new Color[renderers.Length][];
        for (int r = 0; r < renderers.Length; ++r)
        {
            Material[] materials = renderers[r].materials;
            original_colors[r] = new Color[materials.Length];

            for (int m = 0; m < materials.Length; ++m)
            {
                if (materials[m] != null && materials[m].HasProperty("_Color"))
                {
                    original_colors[r][m] = materials[m].color;
                }
            }
        }

        has_cached = true;
    }

    // If visuals were spawned or changed since last cache, refresh.
    private void EnsureCacheIsValid()
    {
        if (!has_cached || renderers == null || renderers.Length == 0)
        {
            CacheRenderersAndColors();
            return;
        }

        // If the renderer count changed (eg visuals swapped), re-cache
        Renderer[] current = GetComponentsInChildren<Renderer>(true);
        if (current.Length != renderers.Length)
        {
            CacheRenderersAndColors();
        }
    }

    // Tints the building to indicate whether the current placement is valid.
    // Keeps the material's current alpha so BuildingManager opacity still works.
    public void SetPreviewColor(bool is_valid)
    {
        EnsureCacheIsValid();

        Color tint_color = is_valid ? Color.blue : Color.red;

        for (int r = 0; r < renderers.Length; ++r)
        {
            Material[] materials = renderers[r].materials;

            for (int m = 0; m < materials.Length; ++m)
            {
                Material mat = materials[m];
                if (mat == null || !mat.HasProperty("_Color"))
                    continue;

                // Keep current alpha (matches DestroyTool behaviour)
                Color current = mat.color;
                Color preview = tint_color;
                preview.a = current.a;

                mat.color = preview;
            }
        }
    }

    // Restores the original material colours once the building has been confirmed
    public void RestoreOriginalColors()
    {
        EnsureCacheIsValid();

        for (int r = 0; r < renderers.Length; ++r)
        {
            Material[] materials = renderers[r].materials;

            for (int m = 0; m < materials.Length; ++m)
            {
                Material mat = materials[m];
                if (mat == null || !mat.HasProperty("_Color"))
                    continue;

                mat.color = original_colors[r][m];
            }
        }
    }
}
