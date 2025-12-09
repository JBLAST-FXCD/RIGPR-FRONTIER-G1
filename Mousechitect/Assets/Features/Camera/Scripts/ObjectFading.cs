using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Jess @ 09/12/25
//<summary>
// This script essentially finds the renderer component on the object its assigned to, then for each renderer it finds the material which has the dissolve shader, and adjusts the alpha value by parsing in the logic from the camera script
//</summary>
public class ObjectFading : MonoBehaviour
{
    private Renderer[] object_renderers;
    private MaterialPropertyBlock material_block;

    private void Awake()
    {
        object_renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetObjectFade(float alpha)
    {
        // this sends the alpha value passed in from the camera controller to the shader to adjust the alpha of the material
        foreach (var renderer in object_renderers)
        {
            material_block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(material_block);
            material_block.SetFloat("_Fade", alpha);
            renderer.SetPropertyBlock(material_block);
        }
    }
}

