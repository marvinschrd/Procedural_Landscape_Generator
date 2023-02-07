using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DomainWarpingGenerator
{
   static FastNoiseLite noise = new FastNoiseLite();
   private static int octaves_ = 5;
   private static float persistance_ = 0.18f;
   private static float lacunarity_ = 3.0f;
   
   private static float noiseHeight_ = 0.0f;
   private static float H = 0.5f;
   private static Vector2 [] octaveOffsets_;

   private static float displacementFactor_ = 80.0f;
   private static NoiseGenerator.NoiseEffect noiseEffect_;

   private static float scale_ = 3;
   // Use the seed to generate the same map regarding the given seed
   static private int seed_ = 1000;

   static public float[] DomainWarping(int width, int heigth, NoiseSettings noiseSettings, float displacementFactor)
   {
      float halfWidth = width *0.5f;
      float halfHeight = heigth *0.5f;
      SetCorrectNoiseType(noiseSettings.noiseType);
      float[] map = new float[width * heigth];
      
      float maxNoiseHeight = float.MinValue;
      float minNoiseHeight = float.MaxValue;
      
      //Set values
      scale_ = noiseSettings.noiseScale <= 0 ? 0.1f : noiseSettings.noiseScale;
      octaves_ = noiseSettings.octaves;
      seed_ = noiseSettings.seed;
      persistance_ = noiseSettings.persistance;
      lacunarity_ = noiseSettings.lacunarity;
      displacementFactor_ = displacementFactor;
      noiseEffect_ = noiseSettings.noiseEffect;
      System.Random prng = new System.Random (seed_);
      
      // Give an offset to each octave in order to sample them from random different locations
      Vector2[] octaveOffsets = new Vector2[octaves_];
      for (int i = 0; i < octaves_; ++i) {
         float offsetX = prng.Next (-100000, 100000) + noiseSettings.offset.x;
         float offsetY = prng.Next (-100000, 100000) + noiseSettings.offset.y;
         octaveOffsets [i] = new Vector2 (offsetX, offsetY);
      }
      octaveOffsets_ = octaveOffsets;


      for (int y = 0; y < width; ++y)
      {
         for (int x = 0; x < heigth; ++x)
         {
            noiseHeight_ = 0f;

            Vector2 point = new Vector2((x - halfWidth )/ scale_, (y - halfHeight) / scale_);
            noiseHeight_ = Pattern(point);
            
            
            if (noiseHeight_ > maxNoiseHeight) {
               maxNoiseHeight = noiseHeight_;
            } else if (noiseHeight_ < minNoiseHeight) {
               minNoiseHeight = noiseHeight_;
            }
            
            map[y * width + x] = noiseHeight_;

         }
      }

      //This loop purpose is to get the values back to 0-1
      if (maxNoiseHeight != minNoiseHeight)
      {
         for (int i = 0; i < map.Length; ++i)
         {
            map[i] = (map[i] - minNoiseHeight) / (maxNoiseHeight - minNoiseHeight);
         }
      }
      
      return map;
   }

   static public float[] DomainWarpingGPU(int width, int height, NoiseSettings noiseSettings, float displacementFactor, ComputeShader domainWarpingComputeShader)
   {
      var prng = new System.Random(noiseSettings.seed);
      Vector2[] offsets = new Vector2[noiseSettings.octaves];
      for (int i = 0; i < noiseSettings.octaves; i++) {
         offsets[i] = new Vector2 (prng.Next (-10000, 10000), prng.Next (-10000, 10000));
      }

      ComputeBuffer offsetBuffer = new ComputeBuffer(offsets.Length, sizeof(float) * 2);
      offsetBuffer.SetData(offsets);
      domainWarpingComputeShader.SetBuffer(0,"offsetBuffer", offsetBuffer );
      int floatToIntMultiplier = 1000;

      float[] heightMap = new float[width * width];
      ComputeBuffer heightMapComputeBuffer = new ComputeBuffer(heightMap.Length, sizeof(float));
      heightMapComputeBuffer.SetData(heightMap);
      domainWarpingComputeShader.SetBuffer(0, "heightmapComputeBuffer", heightMapComputeBuffer);
      
      int[] minMaxHeight = { floatToIntMultiplier * noiseSettings.octaves, 0 };
      ComputeBuffer minMaxBuffer = new ComputeBuffer (minMaxHeight.Length, sizeof (int));
      minMaxBuffer.SetData (minMaxHeight);
      domainWarpingComputeShader.SetBuffer (0, "minMax", minMaxBuffer);
      
      domainWarpingComputeShader.SetInt("mapSize", width);
      domainWarpingComputeShader.SetInt("octaves", noiseSettings.octaves);
      domainWarpingComputeShader.SetFloat ("lacunarity", noiseSettings.lacunarity);
      domainWarpingComputeShader.SetFloat ("persistence", noiseSettings.persistance);
      domainWarpingComputeShader.SetFloat ("scaleFactor", noiseSettings.noiseScale);
      domainWarpingComputeShader.SetFloat("displacementFactor", displacementFactor);
      domainWarpingComputeShader.SetInt("floatToIntMultiplier", floatToIntMultiplier);
      
      int numThreadGroup = heightMap.Length / 1024;
      numThreadGroup = numThreadGroup >= 1 ? numThreadGroup : 1;
      domainWarpingComputeShader.Dispatch (0, numThreadGroup, 1, 1);
      
      heightMapComputeBuffer.GetData (heightMap);
      minMaxBuffer.GetData (minMaxHeight);
      heightMapComputeBuffer.Release ();
      minMaxBuffer.Release ();
      offsetBuffer.Release ();

      float minValue = (float) minMaxHeight[0] / (float) floatToIntMultiplier;
      Debug.Log(minValue);
      float maxValue = (float) minMaxHeight[1] / (float) floatToIntMultiplier;
      Debug.Log(maxValue);
      
      for (int i = 0; i < heightMap.Length; i++) {
         heightMap[i] = Mathf.InverseLerp (minValue, maxValue, heightMap[i]);
      }
      Debug.Log("test2");

      return heightMap;
   }
   static float Pattern(Vector2 point)
   {
      //First Warping
      Vector2 P1 = point + new Vector2(10, 0f);
      Vector2 P2 = point + new Vector2(5.2f, 1.3f);
      Vector2 q = new Vector2(
         FBM(P1),
         FBM(P2)
      );

      //Second Warping
      Vector2 P3 = point + displacementFactor_ * q + new Vector2(1.7f, 9.2f);
      Vector2 P4 = point + displacementFactor_ * q + new Vector2(8.3f, 2.8f);
      Vector2 r = new Vector2(FBM(P3), FBM(P4));
      
      Vector2 r2 = point + displacementFactor_ * r;
      
      float noiseValue = FBM(r2);
      //Debug.Log(noiseValue);
      return noiseValue;
   }
   
static float FBM(Vector2 point )
{
  // float G = Mathf.Pow(2, -H); // G = persitance;
  float G = persistance_;
   float frequency = 1.0f;
   float amplitude = 1.0f;
   float height = 0f;
   for (int i = 0; i < octaves_; ++i)
   {
      float noiseValue = noise.GetNoise(point.x * frequency + octaveOffsets_[i].x, point.y * frequency + octaveOffsets_[i].y);

      switch (noiseEffect_)
      {
         case NoiseGenerator.NoiseEffect.NOEFFECT:
         {
            height += noiseValue * amplitude;
         } break;
         case NoiseGenerator.NoiseEffect.BILLOW:
         {
            //test billow noise
             float billow = Mathf.Abs(noiseValue) * amplitude;
             height += billow;
         } break;
         case NoiseGenerator.NoiseEffect.RIDGED:
         {
            //test ridged noise
            float ridged = Mathf.Abs(noiseValue) * amplitude;
            ridged = 1f - ridged;
            height += ridged;
         } break;
         default:
         {
            height += noiseValue * amplitude;
         } break; 
      }
      amplitude *= G;
      // frequency *= 2.5f;
      frequency *= lacunarity_;
   }

   return height;
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


}



