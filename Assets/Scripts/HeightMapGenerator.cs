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

    //[SerializeField] private NoiseSettings[] noisesSettings;
    [SerializeField]
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

    private float[] finalMap_;
    [SerializeField]
    private int seed_;

    [SerializeField] private int meshMultiplier_;

    private float maxNoiseHeight_;
    private float minNoiseHeight_;
    
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
         //Generate all the heightmaps based their settings
         for (int i = 1; i < noiseLayers.Length; ++i)
        { 
            noiseLayers[i].noiseSettings.seed = seed_;
            noiseLayers[i].SetHeightmap(NoiseGenerator.GenerateNoiseMap2(chunkSize, chunkSize, noiseLayers[i].noiseSettings));
          
        }
    }

    public void GenerateFinalMap()
    {
        finalMap_ = new float[chunkSize * chunkSize];
        // for (int y = 0; y < chunkSize; ++y)
        // {
        //     for (int x = 0; x < chunkSize; ++x)
        //     {
        //         float elevation = 0f;
        //         for (int i = 1; i < noiseLayers.Length ; ++i)
        //         {
        //             float value = 0;
        //             if (noiseLayers[i].enable)
        //             { 
        //                 value = noiseLayers[i].GetHeightmap()[y * chunkSize + x];
        //                 elevation += value;
        //             }
        //            
        //            if (elevation > maxNoiseHeight_) {
        //                            maxNoiseHeight_ = elevation;
        //            } else if (elevation < minNoiseHeight_) {
        //                minNoiseHeight_ = elevation;
        //            }
        //         }
        //        // elevation = Mathf.Max(0, elevation - noiseLayers[2].minValue);
        //         finalMap_[y * chunkSize + x] = elevation;
        //         // finalMap_[y * chunkSize + x] = elevation;
        //     }
        // }
        
        //New test // NEED TO BE FULLY TESTED WITH MIN VALUE ETC... and test to not override value with each layer
        for (int i = 1; i < noiseLayers.Length; ++i)
        {
            float elevation = 0f;
            for (int y = 0; y < chunkSize; ++y)
            {
                for (int x = 0; x < chunkSize; ++x)
                {
                    float value = 0f;
                    if (noiseLayers[i].enable)
                    { 
                        value = noiseLayers[i].GetHeightmap()[y * chunkSize + x];
                    }
                    if (value > maxNoiseHeight_) 
                    {
                        maxNoiseHeight_ = value;
                    } else if (value < minNoiseHeight_) {
                        minNoiseHeight_ = value;
                    }
                   // Debug.Log(value);
                   value = Mathf.Max(0, value - noiseLayers[i].minValue);
                   finalMap_[y * chunkSize + x] += value * noiseLayers[i].strength;
                }
            }
        }

    }


    public void RandomiseValues()
    {
        seed_ = UnityEngine.Random.Range(0, 1000000);
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
        maxNoiseHeight_ = float.MinValue;
        minNoiseHeight_ = float.MaxValue;
        
       GenerateNoiseLayers();
       GenerateFinalMap();
      
       
       
       //This loop purpose is to get the values back to 0-1
       if (maxNoiseHeight_ != minNoiseHeight_)
       {
           for (int i = 0; i < finalMap_.Length; ++i)
           {
               finalMap_[i] = (finalMap_[i] - minNoiseHeight_) / (maxNoiseHeight_ - minNoiseHeight_);
           }
       }
       
       // octaves = noisesSettings.Length > 1 ? noisesSettings.Length - 1 : noisesSettings.Length;

       //---------------------------------------------------------------------------------------
        //Without multiple heightmaps
       // float[] noiseMap2 = NoiseGenerator.GenerateNoiseMap2(chunkSize, chunkSize, noisesSettings[1].seed, noisesSettings[1].noiseScale,
       //     noisesSettings[1].octaves, noisesSettings[1].persistance, noisesSettings[1].lacunarity, noisesSettings[1].offset, noisesSettings[1].noiseType,
       //     applyRidges, noisesSettings);
       //-----------------------------------------------------------------------------------------

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
        DetermineTerrainType2(finalMap_, colorMap);

        if (applyErosion){ HydraulicErosion.Erode2(finalMap_, chunkSize, erosionParameters);}


        MapPlaneDisplayer mapDisplay = FindObjectOfType<MapPlaneDisplayer>();

        if (drawMode == MapDrawMode.NOISEMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromHeightMap2(finalMap_, chunkSize);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == MapDrawMode.COLORMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromColorMap2(colorMap, chunkSize, chunkSize);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == MapDrawMode.MESH)
        {
            if(useHeightCurve)finalMap_ = applyHeightCurveToMap(finalMap_);
            // terrainData.SetHeights(0,0,mapForData);
           //TextureGenerator.UpdateMeshMaterial(mapMaterial, minTerrainMinHeight, maxTerrainHeight, shaderColors, baseStartHeights);
            mapDisplay.DrawMesh(
                MeshGenerator.GenerateMesh2(finalMap_, chunkSize, meshMultiplier_, levelOfDetail, heightCurve,
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
    
    private void Awake()
    {
        fallOffMap = FallOffGenerator.GenerateFallOffMap(chunkSize,falloffMapCurve);
    }
}
