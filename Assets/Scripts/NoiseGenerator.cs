using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;




// This class handle the generation of noise in order to be used to create various HeigtMaps
public static class NoiseGenerator
{
  static private float[,] noiseMap;
  static private float[] noiseMap2;
 static FastNoiseLite noise = new FastNoiseLite();
 private static bool warping = false;

 private static int mapWidth_;
 private static int mapHeight_;
 private static float scale_;
 // private static int octaves_;
 private static  Vector2[] octaveOffsets_;
 
 // static float maxNoiseHeight_ = float.MinValue;
 // static float minNoiseHeight_ = float.MaxValue;

 public enum NoiseEffect
 {
    NOEFFECT,
    BILLOW,
    RIDGED,
    TERRACE,
 }
 
 

 static void SetCorrectNoiseType(HeightMapGenerator.NoiseType noiseType)
 {
    if (noiseType == HeightMapGenerator.NoiseType.SIMPLEXNOISE)
    {
       //scale /= 60f;
       noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }
    else if (noiseType == HeightMapGenerator.NoiseType.PERLINNOISE)
    {
       noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
    }
    else if (noiseType == HeightMapGenerator.NoiseType.CELLULARNOISE)
    {
       noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
    }
    else if (noiseType == HeightMapGenerator.NoiseType.CUBICNOISE)
    {
       noise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
    }
    else if (noiseType == HeightMapGenerator.NoiseType.VALUENOISE)
    {
       noise.SetNoiseType(FastNoiseLite.NoiseType.Value);
    }
 }
 
 //NoiseMap is equivalent to noise filter, and noiseMap = heightmap
 public static float[] GenerateNoiseMap2(int mapWidth, int mapHeight, NoiseSettings settings, AnimationCurve layerCurve, bool applyFallofMap, float [,] fallofMap)
 {
   
    mapWidth_ = mapWidth;
    mapHeight_ = mapHeight;
    //settings.noiseScale /= 60f;
    scale_ =  settings.noiseScale;
    scale_ /= 60f;
    noiseMap2 = new float[mapWidth * mapHeight];
      // Use the seed to generate the same map regarding the given seed
      System.Random prng = new System.Random (settings.seed);
      // Give an offset to each octave in order to sample them from random different locations
      Vector2[] octaveOffsets = new Vector2[settings.octaves];
      for (int i = 0; i < settings.octaves; i++) {
         float offsetX = prng.Next (-100000, 100000) + settings.offset.x;
         float offsetY = prng.Next (-100000, 100000) + settings.offset.y;
         octaveOffsets [i] = new Vector2 (offsetX, offsetY);
      }
      octaveOffsets_ = octaveOffsets;

      if (scale_ <= 0) {
         scale_ = 0.0001f;
      }

      float maxNoiseHeight = float.MinValue;
      float minNoiseHeight = float.MaxValue;
      
      
      HeightMapGenerator.NoiseType noiseType = settings.noiseType;
      SetCorrectNoiseType(noiseType);

      // Calculate the half witdh and half heigth in order to zoom in the center of the map instead of into the corner
      float halfWidth = mapWidth *0.5f;
      float halfHeight = mapHeight *0.5f;
      float noiseValue = 0;
      
      //Loop for every cell of the terrain
      for (int y = 0; y < mapHeight; ++y) {
         for (int x = 0; x < mapWidth; ++x) {

            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            float sampleX = (x - halfWidth) / scale_;
            float sampleY = (y - halfHeight) / scale_;
            
                  Vector2 point = new Vector2(sampleX, sampleY);
               
               // //FBM starts here
               for (int i = 0; i < settings.octaves; i++) {
                  
                  noiseValue = noise.GetNoise(point.x * frequency + octaveOffsets[i].x , point.y * frequency + octaveOffsets[i].y);
                  
                  //Apply a specific effect to this noise map
                  switch (settings.noiseEffect)
                  {
                     // Billow noise will create round hills and shapes on the noise
                     case NoiseEffect.BILLOW :
                     {
                        float n = Mathf.Abs(noiseValue) * amplitude;
                        noiseHeight += n;
                     }
                        break;
                     // Ridged noise will create sharp ridge, mountains peaks
                     case NoiseEffect.RIDGED:
                     {
                        // Take the opposite of the billow noise to create the ridges instead of hills
                        float n = Mathf.Abs(noiseValue) * amplitude;
                        n = 1f - n;
                        noiseHeight += n;
                     }
                        break;
                     case NoiseEffect.TERRACE:
                     {
                        // Take the opposite of the billow noise to create the ridges instead of hills
                        float n = Mathf.Abs(noiseValue) * amplitude;
                        n = 1f - n;
                        n = Mathf.Round(n * 12);
                        noiseHeight += n;
                     }
                        break;
                     case NoiseEffect.NOEFFECT:
                     {
                        noiseHeight += noiseValue * amplitude; //strength
                     }
                        break;
                  }
                  amplitude *= settings.persistance; //strength/gain
                  frequency *= settings.lacunarity; //roughness
               }
               
               if (noiseHeight > maxNoiseHeight) {
                   maxNoiseHeight = noiseHeight;
               } else if (noiseHeight < minNoiseHeight) {
                   minNoiseHeight = noiseHeight;
            }
               
            noiseMap2[y * mapHeight + x] = noiseHeight;
         }
      }
      // Apply clamping of values back to 0-1 and fallofMap in the same loop to avoid having to iterate over the map another time later.
      for (int y = 0; y < mapHeight; ++y)
      {
         for (int x = 0; x < mapHeight; ++x)
         {
            noiseMap2[y * mapHeight + x] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap2[y * mapHeight + x]);
            //noiseMap2[y * mapHeight + x] = (noiseMap2[y * mapHeight + x] - minNoiseHeight) / (maxNoiseHeight - minNoiseHeight);
            if(applyFallofMap) noiseMap2[y * mapHeight + x] = Mathf.Clamp01(noiseMap2[y * mapHeight + x] - fallofMap[x, y]);
         }
      }
      return noiseMap2;
   }

 

 static public float[] GenerateNoiseMapGPU(int mapWidth, int mapHeight, NoiseSettings settings,
    ComputeShader heightmapComputeShader)
 {

    var prng = new System.Random (settings.seed);

    Vector2[] offsets = new Vector2[settings.octaves];
    for (int i = 0; i < settings.octaves; i++) {
       offsets[i] = new Vector2 (prng.Next (-10000, 10000), prng.Next (-10000, 10000));
    }
    ComputeBuffer offsetsBuffer = new ComputeBuffer (offsets.Length, sizeof (float) * 2);
    offsetsBuffer.SetData (offsets);
    heightmapComputeShader.SetBuffer (0, "offsets", offsetsBuffer);

    int floatToIntMultiplier = 1000;
    float[] heightMap = new float[mapWidth * mapHeight];

    ComputeBuffer heightMapBuffer = new ComputeBuffer (heightMap.Length, sizeof (float));
    heightMapBuffer.SetData (heightMap);
    heightmapComputeShader.SetBuffer (0, "heightMap", heightMapBuffer);

    int[] minMaxHeight = { floatToIntMultiplier * settings.octaves, 0 };
    ComputeBuffer minMaxBuffer = new ComputeBuffer (minMaxHeight.Length, sizeof (int));
    minMaxBuffer.SetData (minMaxHeight);
    heightmapComputeShader.SetBuffer (0, "minMax", minMaxBuffer);

    heightmapComputeShader.SetInt ("mapSize", mapHeight);
    heightmapComputeShader.SetInt ("octaves", settings.octaves);
    heightmapComputeShader.SetFloat ("lacunarity", settings.lacunarity);
    heightmapComputeShader.SetFloat ("persistence", settings.persistance);
    heightmapComputeShader.SetFloat ("scaleFactor", settings.noiseScale);
    switch (settings.noiseEffect)
    {
       case NoiseEffect.NOEFFECT:
       {
          heightmapComputeShader.SetInt("noiseEffect", 0);
       }
          break;
       case NoiseEffect.BILLOW:
       {
          heightmapComputeShader.SetInt("noiseEffect", 1);
       }
          break;
       case NoiseEffect.RIDGED:
       {
          heightmapComputeShader.SetInt("noiseEffect", 2);
       }
          break;
    }
    heightmapComputeShader.SetInt ("floatToIntMultiplier", floatToIntMultiplier);

    int numThreadGroup = heightMap.Length / 1024;
    numThreadGroup = numThreadGroup >= 1 ? numThreadGroup : 1;
    heightmapComputeShader.Dispatch (0, numThreadGroup, 1, 1);

    heightMapBuffer.GetData (heightMap);
    minMaxBuffer.GetData (minMaxHeight);
    heightMapBuffer.Release ();
    minMaxBuffer.Release ();
    offsetsBuffer.Release ();

    float minValue = (float) minMaxHeight[0] / (float) floatToIntMultiplier;
    float maxValue = (float) minMaxHeight[1] / (float) floatToIntMultiplier;

    for (int i = 0; i < heightMap.Length; i++) {
       heightMap[i] = Mathf.InverseLerp (minValue, maxValue, heightMap[i]);
    }

    return heightMap;
 }
}