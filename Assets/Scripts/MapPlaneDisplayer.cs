using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPlaneDisplayer : MonoBehaviour
{

    [SerializeField] private Renderer planeTextureRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    // Draw noise map on the display plane
    public void DrawTexture(Texture2D texture)
    {
        planeTextureRenderer.sharedMaterial.mainTexture = texture;
        planeTextureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D meshtexture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
       meshRenderer.sharedMaterial.mainTexture = meshtexture;
    }
}
