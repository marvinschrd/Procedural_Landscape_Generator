using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Random = System.Random;

public class HeigtMapGenerator : MonoBehaviour
{
    [Header("Map Values")]
    [SerializeField] private int mapWidth = 0;
    [SerializeField] private int mapHeigth = 0;
    [SerializeField] private float noiseScale = 0f;
    [SerializeField] private int octaves = 0;
    
    [Range(0,1)]
    [SerializeField] private float persistance = 0.5f;
    [SerializeField] private float lacunarity = 2f;

    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    
    public bool autoUpdateMap = false;
    
    
    public enum MapDrawMode
    {
        NOISEMAP,
        COLORMAP,
        MESH
    }
    public MapDrawMode drawMode = MapDrawMode.NOISEMAP;
    public TerrainType[] mapRegions;

    
    [System.Serializable]
    public struct TerrainType
    {
        public string terrainName;
        public float height;
        public Color terrainColor;
        public float terrainHardness;
    }


    public void RandomiseValues()
    {
        seed = UnityEngine.Random.Range(0, 1000000);
    }

    public void GenerateMap()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeigth, seed, noiseScale ,octaves, persistance, lacunarity, offset);
        Color[] colorMap = new Color[mapHeigth * mapWidth];
        

         DetermineTerrainType(noiseMap,colorMap);
        
        MapPlaneDisplayer mapDisplay = FindObjectOfType<MapPlaneDisplayer>();

        if (drawMode == MapDrawMode.NOISEMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromHeightMap(noiseMap);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == MapDrawMode.COLORMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeigth);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == MapDrawMode.MESH)
        {
            mapDisplay.DrawMesh(MeshGenerator.GenerateMesh(noiseMap),TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeigth));
        }
    }
    void OnValidate() {
        if (mapWidth < 1) {
            mapWidth = 1;
        }
        if (mapHeigth < 1) {
            mapHeigth = 1;
        }
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }
    }


    private void DetermineTerrainType(float [,] noiseMap, Color [] colorMap)
    {
        for (int y = 0; y < mapHeigth; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < mapRegions.Length; i++)
                {
                    if (currentHeight <= mapRegions[i].height)
                    {
                        colorMap[y * mapWidth + x] = mapRegions[i].terrainColor;
                        break;
                    }
                }
            }
        }
    }
}
