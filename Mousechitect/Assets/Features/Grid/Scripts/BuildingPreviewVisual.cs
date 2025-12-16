using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anthony Grummett - 2/12/25

/// <summary>
/// Handles temporary visual feedback for a building while it is being placed.
/// The logic (valid / invalid) lives in BuildingManager this class only worries about colours.
/// </summary>
public class BuildingPreviewVisual : MonoBehaviour
{
    private const float PREVIEW_ALPHA_DEFAULT = 0.6f;

    private Renderer[] renderers;
    private Color[][] original_colors;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        original_colors = new Color[renderers.Length][];

        // Cache original colours so we can restore them once the building is placed.
        for (int r = 0; r < renderers.Length; ++r)
        {
            Material[] materials = renderers[r].materials;
            original_colors[r] = new Color[materials.Length];

            for (int m = 0; m < materials.Length; ++m)
            {
                if (materials[m].HasProperty("_Color"))
                {
                    original_colors[r][m] = materials[m].color;
                }
            }
        }
    }

    // PUBLIC FUNCTIONS

    // Tints the building to indicate whether the current placement is valid.
    public void SetPreviewColor(bool is_valid)
    {
        // blue highlight while building is in a valid placement state.
        Color tint_color = is_valid ? Color.blue : Color.red;

        for (int r = 0; r < renderers.Length; ++r)
        {
            Material[] materials = renderers[r].materials;

            for (int m = 0; m < materials.Length; ++m)
            {
                if (!materials[m].HasProperty("_Color"))
                {
                    continue;
                }

                Color preview_color = tint_color;
                preview_color.a = PREVIEW_ALPHA_DEFAULT;
                materials[m].color = preview_color;
            }
        }
    }

    // Restores the original material colours once the building has been confirmed.
    public void RestoreOriginalColors()
    {
        for (int r = 0; r < renderers.Length; ++r)
        {
            Material[] materials = renderers[r].materials;

            for (int m = 0; m < materials.Length; ++m)
            {
                if (!materials[m].HasProperty("_Color"))
                {
                    continue;
                }

                materials[m].color = original_colors[r][m];
            }
        }
    }
}