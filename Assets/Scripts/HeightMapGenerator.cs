using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Random = System.Random;

public class HeightMapGenerator : MonoBehaviour
{
    private const int chunkSize = 241; // 241 because of unity limitiation of 255 and because of possible use in LOD implementation
    [Range(0,6)]
    [SerializeField] private int levelOfDetail = 0;

    [SerializeField] public Noise[] noises;
    private List<float[,]> heightmaps = new List<float[,]>();

    
    public bool autoUpdateMap = false;
    public bool useFallOffMap = false;
    public bool applyRidges = false;
    public bool applyWarping = false;
    public bool combineNoises = false;
    [SerializeField] private AnimationCurve heightCurve;
    public bool useHeightCurve = false;
    [SerializeField] private AnimationCurve falloffMapCurve;

    public enum NoiseType
    {
        PERLINNOISE,
        SIMPLEXNOISE,
        CELLULARNOISE,
        CUBICNOISE,
        VALUENOISE
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
        noises[1].seed = UnityEngine.Random.Range(0, 1000000);
    }

    public void GenerateMap()
    {
        //heightCurve.MoveKey(1, new Keyframe(0.9f, 0.1f));
       // float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, chunkSize, seed, noiseScale ,octaves, persistance, lacunarity, offset, noiseType, applyRidges);
       float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, chunkSize, noises[1].seed, noises[1].noiseScale ,noises[1].octaves, noises[1].persistance, noises[1].lacunarity, noises[1].offset, noises[1].noiseType, applyRidges);
       float[,] secondNoiseMap = null;
       if (noises.Length > 2)
       {
           secondNoiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, chunkSize, noises[2].seed,
               noises[2].noiseScale, noises[2].octaves, noises[2].persistance, noises[2].lacunarity, noises[2].offset,
               noises[2].noiseType, false); 
       }
       // GenerateOthersHeigthmaps();
       if (applyWarping)
        {
            for (int y = 0; y < chunkSize; ++y)
            {
                for (int x = 0; x < chunkSize; ++x)
                {
                    noiseMap[x, y] = NoiseGenerator.Warping(new Vector2(x, y));
                }
                
            }
        }

        if (combineNoises)
        {
            for (int y = 0; y < chunkSize; ++y)
            {
                for (int x = 0; x < chunkSize; ++x)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] *(1.5f*(secondNoiseMap[x, y])) );
                }
            }
        }
        
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
            mapDisplay.DrawMesh(MeshGenerator.GenerateMesh(noiseMap, noises[1].meshHeightMultiplier,levelOfDetail, heightCurve, useHeightCurve),TextureGenerator.TextureFromColorMap(colorMap, chunkSize, chunkSize));
        }
        else if (drawMode == MapDrawMode.FALLOFMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromHeightMap(FallOffGenerator.GenerateFallOffMap(chunkSize,falloffMapCurve));
            mapDisplay.DrawTexture(texture);
        }
    }
    void OnValidate() {
        // if (lacunarity < 1) {
        //     lacunarity = 1;
        // }
        // if (octaves < 0) {
        //     octaves = 0;
        // }

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

                // maybe try to make the warping here, on the already generated noisemap/texture;

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

    public void GenerateOthersHeigthmaps()
    {
        // 2 because 0 is empty (editor problems) and 1 is already the base heightmap for the terrain
        if (noises.Length > 2)
        {
            for (int i = 2; i < noises.Length; ++i)
            {
                float[,] heightmap = NoiseGenerator.GenerateNoiseMap(chunkSize, chunkSize, noises[i].seed,
                    noises[i].noiseScale, noises[i].octaves, noises[i].persistance, noises[i].lacunarity,
                    noises[i].offset, noises[i].noiseType, applyRidges);
                heightmaps.Add(heightmap);
            }
        }
    }
    

    private void Awake()
    {
        fallOffMap = FallOffGenerator.GenerateFallOffMap(chunkSize,falloffMapCurve);
    }
}
