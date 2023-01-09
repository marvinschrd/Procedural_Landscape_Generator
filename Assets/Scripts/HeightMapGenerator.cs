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

    // [SerializeField] private int nmbOfChunks = 1;
    // private int[] mapChunks = new int[1];

    [SerializeField] private Erosion erosionParameters;

    
    public bool autoUpdateMap = false;
    public bool useFallOffMap = false;
    [SerializeField] private AnimationCurve finalMapHeightCurve;
    public bool useHeightCurve = false;
    [SerializeField] private bool applyErosion = false;
    [SerializeField] private AnimationCurve falloffMapCurve;

    private float[] finalMap_;
    [SerializeField]
    private int seed_;

    [SerializeField] private bool applyScalingRatio = false;
    [SerializeField] private int meshMultiplier_;

    private float maxNoiseHeight_;
    private float minNoiseHeight_;

    [SerializeField] private ComputeShader erosionComputeShader;
    [SerializeField] private ComputeShader heightmapComputeShader;
    [SerializeField] private ComputeShader domainWarpingComputeShader;
    
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
             float[] layerMap;
            noiseLayers[i].noiseSettings.seed = seed_;
            //Check if the layer has to be warped
            if (noiseLayers[i].warpNoise)
            {
                layerMap = DomainWarpingGenerator.DomainWarping(chunkSize, chunkSize, noiseLayers[i].noiseSettings,
                    noiseLayers[i].warpDisplacementFactor);
                // noiseLayers[i].SetHeightmap(DomainWarpingGenerator.DomainWarpingGPU(chunkSize,chunkSize,noiseLayers[i].noiseSettings, noiseLayers[i].warpDisplacementFactor, domainWarpingComputeShader ));
            }
            else
            {
                layerMap = NoiseGenerator.GenerateNoiseMap2(chunkSize, chunkSize, noiseLayers[i].noiseSettings,
                    noiseLayers[i].layerCurve, noiseLayers[i].applyFallofMap, fallOffMap);
                // noiseLayers[i].SetHeightmap(NoiseGenerator.GenerateNoiseMapGPU(chunkSize, chunkSize, noiseLayers[i].noiseSettings, heightmapComputeShader));
            }
            if (noiseLayers[i].applyHeightCurve)
            {
                layerMap = applyHeightCurveToMap(noiseLayers[i].GetHeightmap(), chunkSize, noiseLayers[i].layerCurve);
            } 
            noiseLayers[i].SetHeightmap(layerMap);
        }
    }

    public void GenerateFinalMap()
    {
        finalMap_ = new float[chunkSize * chunkSize];

        //New test // NEED TO BE FULLY TESTED WITH MIN VALUE ETC... and test to not override value with each layer
        for (int i = 1; i < noiseLayers.Length; ++i)
        {
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
                    value = Mathf.Max(0, value - noiseLayers[i].minValue);
                   //Check to only add the positive difference between the new layer and the previous one so that we do not get a fully flat map of value clamped to 1
                   float gain = Mathf.Max(0, value - finalMap_[y * chunkSize + x]);
                   finalMap_[y * chunkSize + x] += gain * noiseLayers[i].strength;
                }
            }
        }

    }


    public void RandomiseValues()
    {
        seed_ = UnityEngine.Random.Range(0, 1000000);
    }
    float[] applyHeightCurveToMap(float[] noiseMap,int mapSize, AnimationCurve mapCurve)
    {
        for (int y = 0; y < mapSize; ++y)
        {
            for (int x = 0; x < mapSize; ++x)
            {
                noiseMap[y * mapSize + x] = mapCurve.Evaluate(noiseMap[y * mapSize + x]);
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
           for (int y = 0; y < chunkSize; ++y)
           {
               for (int x = 0; x < chunkSize; ++x)
               {
                   finalMap_[y * chunkSize + x] =
                       Mathf.InverseLerp(minNoiseHeight_, maxNoiseHeight_, finalMap_[y * chunkSize + x]);
                   if (useFallOffMap)
                   {
                       finalMap_[y * chunkSize + x] = Mathf.Clamp01(finalMap_[y * chunkSize + x] - fallOffMap[x, y]) ;
                   }
               }
           }
       }

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

        if (applyErosion)
        {
            // HydraulicErosion.Erode2(finalMap_, chunkSize, erosionParameters);    
           finalMap_ = HydraulicErosion.ErodeFromComputeShader(finalMap_, chunkSize, erosionParameters ,erosionComputeShader);
        }


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
            if(useHeightCurve)finalMap_ = applyHeightCurveToMap(finalMap_, chunkSize,finalMapHeightCurve);
            if (applyScalingRatio) meshMultiplier_ = (noiseLayers[1].noiseSettings.noiseScale * 2);
            int meshMultiplier = applyScalingRatio ? noiseLayers[1].noiseSettings.noiseScale * 2 : meshMultiplier_;
            mapDisplay.DrawMesh(
                MeshGenerator.GenerateMesh2(finalMap_, chunkSize, meshMultiplier, levelOfDetail), TextureGenerator.TextureFromColorMap2(colorMap, chunkSize, chunkSize));
            //Show noise map
            Texture2D texture = TextureGenerator.TextureFromHeightMap2(finalMap_, chunkSize);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == MapDrawMode.FALLOFMAP)
        {
            Texture2D texture = TextureGenerator.TextureFromHeightMap(FallOffGenerator.GenerateFallOffMap(chunkSize,falloffMapCurve));
            mapDisplay.DrawTexture(texture);
        }
    }
    void OnValidate() {
        fallOffMap = FallOffGenerator.GenerateFallOffMap(chunkSize,falloffMapCurve);
    }

    private void DetermineTerrainType2(float [] noiseMap, Color [] colorMap)
    {
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                // Using the falloff map
               
                
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
