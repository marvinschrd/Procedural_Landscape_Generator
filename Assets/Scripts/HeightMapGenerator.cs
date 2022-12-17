using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class HeightMapGenerator : MonoBehaviour
{
   // private const int chunkSize = 241; // 241 because of unity limitiation of 255 and because of possible use in LOD implementation
     [SerializeField] int chunkSize = 241; // 241 because of unity limitiation of 255 and because of possible use in LOD implementation
     // [SerializeField] private TerrainData terrainData;
     

    [Range(0,6)]
    [SerializeField] private int levelOfDetail = 0;

    [SerializeField] private NoiseSettings[] noisesSettings;
    private NoiseLayer[] noiseLayers;
    private int octaves = 0;

    [SerializeField] private int nmbOfChunks = 1;
    private int[] mapChunks = new int[1];

    [SerializeField] private Erosion erosionParameters;

    
    public bool autoUpdateMap = false;
    public bool useFallOffMap = false;
    public bool applyRidges = false;
    [SerializeField] private AnimationCurve heightCurve;
    public bool useHeightCurve = false;
    [SerializeField] private bool applyErosion = false;
    [SerializeField] private AnimationCurve falloffMapCurve;


    //Shader tests not working
    // [SerializeField] private Material mapMaterial;
    // [SerializeField] private Color[] shaderColors;
    // [Range(0,1)]
    // [SerializeField] private float[] baseStartHeights;
    // public float minTerrainMinHeight
    // {
    //     get
    //     {
    //         return noises[1].meshHeightMultiplier * heightCurve.Evaluate(0);
    //     }
    // }
    // public float maxTerrainHeight
    // {
    //     get
    //     {
    //         return noises[1].meshHeightMultiplier * heightCurve.Evaluate(1);
    //     }
    // }
    
    

    public enum NoiseType
    {
        PERLINNOISE,
        SIMPLEXNOISE,
        CELLULARNOISE,
        CUBICNOISE,
        VALUENOISE
    }
     NoiseType noiseType = NoiseType.PERLINNOISE;
    
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

    // Generate one heightmap for each defined noise layer.
    // Each noise settings will be used to create one heightmap/noise layer corresponding to the settings values.
    public void GenerateNoiseLayers()
    {
        noiseLayers = new NoiseLayer[noisesSettings.Length - 1];
        for (int i = 0; i < noiseLayers.Length; ++i)
        {
            noiseLayers[i].heightmap = NoiseGenerator.GenerateNoiseMap2(chunkSize, chunkSize, noisesSettings[i+1].seed,
                noisesSettings[i+1].noiseScale, noisesSettings[i+1].octaves, noisesSettings[i+1].persistance,
                noisesSettings[i+1].lacunarity, noisesSettings[i].offset, noisesSettings[i+1].noiseType,
                noisesSettings[i+1].applyRidge, noisesSettings);
        }
    }


    public void RandomiseValues()
    {
        noisesSettings[1].seed = UnityEngine.Random.Range(0, 1000000);
    }
    float[] applyHeightCurveToMap(float[] noiseMap)
    {
        for (int y = 0; y < chunkSize; ++y)
        {
            for (int x = 0; x < chunkSize; ++x)
            {
                noiseMap[y * chunkSize + x] = heightCurve.Evaluate(noiseMap[y * chunkSize + x]);
            }
        }
        return noiseMap;
    }

    public void GenerateMap()
    {
        //Generate all the heightmaps based on the noiseSettings array
       // GenerateNoiseLayers();

        octaves = noisesSettings.Length > 1 ? noisesSettings.Length - 1 : noisesSettings.Length;

        // float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, chunkSize, seed, noiseScale ,octaves, persistance, lacunarity, offset, noiseType, applyRidges);
        // float[,] noiseMap1 = NoiseGenerator.GenerateNoiseMap(chunkSize, chunkSize, noises[1].seed, noises[1].noiseScale,
        //     noises[1].octaves, noises[1].persistance, noises[1].lacunarity, noises[1].offset, noises[1].noiseType,
        //     applyRidges);
        

        //float finalHeightmap [] = new float[chunksize * chunksize];
        //   
        //     for(int y = 0; y < chunksize; ++y)
        //      {
        //         for(int x = 0; x < chunksize; ++x)
        //           {
        //             float elevation = 0;
        //               for(int i = 0; i < heightmaps.length; ++i)
        //                  {
        //                      elevation+= heightmaps[i][y * chunksize + x];
        //                  }
        //              finalHeightmap[y * chunksize + x] = elevation;
        //           }
        //      }
        // 
       float[] noiseMap2 = NoiseGenerator.GenerateNoiseMap2(chunkSize, chunkSize, noisesSettings[1].seed, noisesSettings[1].noiseScale,
           octaves, noisesSettings[1].persistance, noisesSettings[1].lacunarity, noisesSettings[1].offset, noisesSettings[1].noiseType,
           applyRidges, noisesSettings);


       //Converting 1D flat noiseMap into a 2Dimensionnal array for testing on unity TerrainData
       // float[,] mapForData = new float[chunkSize,chunkSize];
       // for (int y = 0; y < chunkSize; ++y)
       // {
       //     for (int x = 0; x < chunkSize; ++x)
       //     {
       //         mapForData[x, y] = noiseMap2[y * chunkSize + x];
       //     }
       // }


       Color[] colorMap = new Color[chunkSize * chunkSize];
       //DetermineTerrainType(noiseMap,colorMap);
        DetermineTerrainType2(noiseMap2, colorMap);

        if (applyErosion){ HydraulicErosion.Erode2(noiseMap2, chunkSize, erosionParameters); Debug.Log("applied erosion");}


        MapPlaneDisplayer mapDisplay = FindObjectOfType<MapPlaneDisplayer>();

        if (drawMode == MapDrawMode.NOISEMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromHeightMap2(noiseMap2, chunkSize);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == MapDrawMode.COLORMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromColorMap2(colorMap, chunkSize, chunkSize);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == MapDrawMode.MESH)
        {
            if(useHeightCurve)noiseMap2 = applyHeightCurveToMap(noiseMap2);
           // terrainData.SetHeights(0,0,mapForData);
           //TextureGenerator.UpdateMeshMaterial(mapMaterial, minTerrainMinHeight, maxTerrainHeight, shaderColors, baseStartHeights);
            mapDisplay.DrawMesh(
                MeshGenerator.GenerateMesh2(noiseMap2, chunkSize, noisesSettings[1].meshHeightMultiplier, levelOfDetail, heightCurve,
                    useHeightCurve, erosionParameters, applyErosion), TextureGenerator.TextureFromColorMap2(colorMap, chunkSize, chunkSize));
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


    // private void DetermineTerrainType(float [,] noiseMap, Color [] colorMap)
    // {
    //     for (int y = 0; y < chunkSize; y++)
    //     {
    //         for (int x = 0; x < chunkSize; x++)
    //         {
    //             // Using the falloff map
    //             if (useFallOffMap)
    //             {
    //                 noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallOffMap[x, y]) ;
    //             }
    //
    //             // maybe try to make the warping here, on the already generated noisemap/texture;
    //
    //             float currentHeight = noiseMap[x, y];
    //             for (int i = 0; i < mapRegions.Length; i++)
    //             {
    //                 if (currentHeight <= mapRegions[i].height)
    //                 {
    //                     colorMap[y * chunkSize + x] = mapRegions[i].terrainColor;
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    // }
    
    private void DetermineTerrainType2(float [] noiseMap, Color [] colorMap)
    {
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                // Using the falloff map
                if (useFallOffMap)
                {
                    noiseMap[y * chunkSize + x] = Mathf.Clamp01(noiseMap[y * chunkSize + x] - fallOffMap[x, y]) ;
                }

                // maybe try to make the warping here, on the already generated noisemap/texture;

                float currentHeight = noiseMap[y * chunkSize + x];
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

    // would be used if more than 1 heightmap (noise) would be generated and used
    // public void GenerateOthersHeigthmaps()
    // {
    //     // 2 because 0 is empty (editor problems) and 1 is already the base heightmap for the terrain
    //     if (noises.Length > 2)
    //     {
    //         for (int i = 2; i < noises.Length; ++i)
    //         {
    //             float[,] heightmap = NoiseGenerator.GenerateNoiseMap(chunkSize, chunkSize, noises[i].seed,
    //                 noises[i].noiseScale, noises[i].octaves, noises[i].persistance, noises[i].lacunarity,
    //                 noises[i].offset, noises[i].noiseType, applyRidges);
    //             heightmaps.Add(heightmap);
    //         }
    //     }
    // }
    

    private void Awake()
    {
        fallOffMap = FallOffGenerator.GenerateFallOffMap(chunkSize,falloffMapCurve);
    }
}
