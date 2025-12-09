using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Jess @ 09/12/25
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

