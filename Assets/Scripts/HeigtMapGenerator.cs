using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Random = System.Random;

public class HeigtMapGenerator : MonoBehaviour
{
    private const int chunkSize = 241; // 241 because of unity limitiation of 255 and because of possible use in LOD implementation
    [Header("Map Values")]
    [SerializeField] private float noiseScale = 0f;
    [SerializeField] private int octaves = 0;
    [SerializeField] public float meshHeightMultiplier = 0;

    [Range(0,1)]
    [SerializeField] private float persistance = 0.5f;
    [SerializeField] private float lacunarity = 2f;

    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    [SerializeField] private AnimationCurve heightCurve;
    
    public bool autoUpdateMap = false;
    public bool useFallOffMap = false;
    [SerializeField] private AnimationCurve falloffMapCurve;

    public enum NoiseType
    {
        PERLINNOISE,
        SIMPLEXNOISE
    }
    public NoiseType noiseType = NoiseType.PERLINNOISE;
    
    public enum MapDrawMode
    {
        NOISEMAP,
        COLORMAP,
        MESH,
        FALLOFMAP
    }
    public MapDrawMode drawMode = MapDrawMode.NOISEMAP;
    public TerrainType[] mapRegions;
    private float[,] fallOffMap;
    
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
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, chunkSize, seed, noiseScale ,octaves, persistance, lacunarity, offset, noiseType);
        Color[] colorMap = new Color[chunkSize * chunkSize];
        

         DetermineTerrainType(noiseMap,colorMap);
        
        MapPlaneDisplayer mapDisplay = FindObjectOfType<MapPlaneDisplayer>();

        if (drawMode == MapDrawMode.NOISEMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromHeightMap(noiseMap);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == MapDrawMode.COLORMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromColorMap(colorMap, chunkSize, chunkSize);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == MapDrawMode.MESH)
        {
            mapDisplay.DrawMesh(MeshGenerator.GenerateMesh(noiseMap, meshHeightMultiplier, heightCurve),TextureGenerator.TextureFromColorMap(colorMap, chunkSize, chunkSize));
        }
        else if (drawMode == MapDrawMode.FALLOFMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromHeightMap(FallOffGenerator.GenerateFallOffMap(chunkSize,falloffMapCurve));
            mapDisplay.DrawTexture(texture);
        }
    }
    void OnValidate() {
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }

        fallOffMap = FallOffGenerator.GenerateFallOffMap(chunkSize,falloffMapCurve);
    }


    private void DetermineTerrainType(float [,] noiseMap, Color [] colorMap)
    {
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                // Using the falloff map
                if (useFallOffMap)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]) ;
                }
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < mapRegions.Length; i++)
                {
                    if (currentHeight <= mapRegions[i].height)
                    {
                        colorMap[y * chunkSize + x] = mapRegions[i].terrainColor;
                        break;
                    }
                }
            }
        }
    }

    private void Awake()
    {
        fallOffMap = FallOffGenerator.GenerateFallOffMap(chunkSize,falloffMapCurve);
    }
}
