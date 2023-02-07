using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using Random = System.Random;

public class HeightMapGenerator : MonoBehaviour
{
    [SerializeField] int chunkSize = 241; // 241 because of unity limitiation of 255 and because of possible use in LOD implementation

    [Range(0,6)]
    [SerializeField] private int levelOfDetail = 0;
    
    [SerializeField]
    private NoiseLayer[] noiseLayers;
    private int octaves = 0;

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
             float[,] diamondMap = new float[chunkSize, chunkSize];
            noiseLayers[i].noiseSettings.seed = seed_;
            //Check if the layer has to be warped
            if (noiseLayers[i].warpNoise)
            {
                layerMap = DomainWarpingGenerator.DomainWarping(chunkSize, chunkSize, noiseLayers[i].noiseSettings,
                     noiseLayers[i].warpDisplacementFactor);
                //layerMap =DomainWarpingGenerator.DomainWarpingGPU(chunkSize,chunkSize,noiseLayers[i].noiseSettings, noiseLayers[i].warpDisplacementFactor, domainWarpingComputeShader );
            }
            else
            {
                //DIamond square test
                //diamondMap = DiamondSquareAlgorithm.DiamondSquareMap(chunkSize);


                // FBM test
                layerMap = NoiseGenerator.GenerateNoiseMap2(chunkSize, chunkSize, noiseLayers[i].noiseSettings,
                    noiseLayers[i].layerCurve, noiseLayers[i].applyFallofMap, fallOffMap);
                //FOR GPU 
                 //layerMap = NoiseGenerator.GenerateNoiseMapGPU(chunkSize, chunkSize, noiseLayers[i].noiseSettings, heightmapComputeShader);
                 
                
            }
            if (noiseLayers[i].applyHeightCurve)
            {
                layerMap = applyHeightCurveToMap(noiseLayers[i].GetHeightmap(), chunkSize, noiseLayers[i].layerCurve);
            } 
             noiseLayers[i].SetHeightmap(layerMap);
            //noiseLayers[i].SetHeightmap(TwoDimensionMapToFlatMap(diamondMap, chunkSize));

         }
    }

   public float[] TwoDimensionMapToFlatMap(float[,] twoDimensionalMap, int size)
    {
        float[] flatMap = new float[size * size];

        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                flatMap[y * size + x] = twoDimensionalMap[x, y];
            }
        }

        return flatMap;
    }

    public float[,] FlatMapToTwoDimensionMap(float[] flatMap, int size)
    {
        float[,] twoDimensionMap = new float[size, size];
        for (int y = 0; y < size; ++y)
        {
            for (int x = 0; x < size; ++x)
            {
                twoDimensionMap[x, y] = flatMap[y * size + x];
            }
        }
        return twoDimensionMap;
    }

    public void GenerateFinalMap()
    {
        finalMap_ = new float[chunkSize * chunkSize];
        
        for (int i = 1; i < noiseLayers.Length; ++i)
        {
            float[] layerHeightMap = noiseLayers[i].GetHeightmap();
            for (int y = 0; y < chunkSize; ++y)
            {
                for (int x = 0; x < chunkSize; ++x)
                {
                    float value = 0f;
                    if (noiseLayers[i].enable)
                    { 
                        value = layerHeightMap[y * chunkSize + x];
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
                   //float gain = value;
                   finalMap_[y * chunkSize + x] += gain * noiseLayers[i].strength;
                }
            }
        }

    }

    public void generateFinalMap2()
    {
        // finalMap_ = new float[chunkSize * chunkSize];
        // for (int y = 0; y < chunkSize; ++y)
        // {
        //     for (int x = 0; x < chunkSize; ++x)
        //     {
        //         float gain = 0f;
        //         // Loop trough already generated layers
        //         for (int i = 1; i < noiseLayers.Length; ++i)
        //         {
        //             if(noiseLayers[i].enable) gain+= noiseLayers[i].GetHeightmap()[y * chunkSize + x] * noiseLayers[i].strength;
        //             if (gain > maxNoiseHeight_) 
        //             {
        //                 maxNoiseHeight_ = gain;
        //             } else if (gain < minNoiseHeight_) {
        //                 minNoiseHeight_ = gain;
        //             }
        //             gain = Mathf.Max(0, gain - noiseLayers[i].minValue);
        //         }
        //
        //         finalMap_[y * chunkSize + x] = gain;
        //     }
        // }
        
        finalMap_ = new float[chunkSize * chunkSize];
        
        for (int i = 1; i < noiseLayers.Length; ++i)
        {
            float[] layerHeightMap = noiseLayers[i].GetHeightmap();
            for (int y = 0; y < chunkSize; ++y)
            {
                for (int x = 0; x < chunkSize; ++x)
                {
                    float value = 0f;
                    if (noiseLayers[i].enable)
                    { 
                        value = layerHeightMap[y * chunkSize + x];
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
                    //float gain = value;
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
      // GenerateFinalMap();
       generateFinalMap2();
      
       
       
       //This loop purpose is to get the values back to 0-1
       if (maxNoiseHeight_ != minNoiseHeight_)
       {
           for (int y = 0; y < chunkSize; ++y)
           {
               for (int x = 0; x < chunkSize; ++x)
               {
                    finalMap_[y * chunkSize + x] =
                        Mathf.InverseLerp(minNoiseHeight_, maxNoiseHeight_, finalMap_[y * chunkSize + x]);
                    if (useFallOffMap) {
                       finalMap_[y * chunkSize + x] = Mathf.Clamp01(finalMap_[y * chunkSize + x] - fallOffMap[x, y]) ; }
               }
           }
       }


       Color[] colorMap = new Color[chunkSize * chunkSize];
       //DetermineTerrainType(noiseMap,colorMap);
        DetermineTerrainType2(finalMap_, colorMap);

        if (applyErosion)
        {
            float[,] erodedMap = FlatMapToTwoDimensionMap(finalMap_, chunkSize);
            //finalMap_ = TwoDimensionMapToFlatMap(ThermalErosion.Erode(erodedMap, chunkSize), chunkSize);
            
             //HydraulicErosion.Erode2(finalMap_, chunkSize, erosionParameters);  
              finalMap_ = HydraulicErosion.ErodeFromComputeShader(finalMap_, chunkSize, erosionParameters ,erosionComputeShader);
            //finalMap_ = TwoDimensionMapToFlatMap(AthermalErosion.Simulate(finalMap_, chunkSize), chunkSize);
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
