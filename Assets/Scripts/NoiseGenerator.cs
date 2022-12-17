using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
 private static int octaves_;
 private static  Vector2[] octaveOffsets_;
 
 static float maxNoiseHeight_ = float.MinValue;
 static float minNoiseHeight_ = float.MaxValue;
 public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset,HeightMapGenerator.NoiseType noiseType, bool applyRidges)
 {
    mapWidth_ = mapWidth;
    mapHeight_ = mapHeight;
    scale /= 60f;
    scale_ = scale;
    octaves_ = octaves;
      noiseMap = new float[mapWidth, mapHeight];
      // Use the seed to generate the same map regarding the given seed
      System.Random prng = new System.Random (seed);
      // Give an offset to each octave in order to sample them from random different locations
      Vector2[] octaveOffsets = new Vector2[octaves];
      for (int i = 0; i < octaves; i++) {
         float offsetX = prng.Next (-100000, 100000) + offset.x;
         float offsetY = prng.Next (-100000, 100000) + offset.y;
         octaveOffsets [i] = new Vector2 (offsetX, offsetY);
      }
      octaveOffsets_ = octaveOffsets;
 
      // Create and configure FastNoise object
      // Currently reducing the scale because of the apparent huge difference in size between
      // the unity perlin noise and Fastnoiselite opensimplexnoise
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
      
      if (scale <= 0) {
         scale = 0.0001f;
      }
      
      
 
      float maxNoiseHeight = float.MinValue;
      float minNoiseHeight = float.MaxValue;
      
      
      
      
 
      // Calculate the half witdh and half heigth in order to zoom in the center of the map instead of into the corner
      float halfWidth = mapWidth *0.5f;
      float halfHeight = mapHeight *0.5f;
      float noiseValue;
      
      //Loop for every cell of the terrain
      for (int y = 0; y < mapHeight; y++) {
         for (int x = 0; x < mapWidth; x++) {
 
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;
            
            // float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
            // float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;
 
            //Vector2 point = new Vector2(x, y);
           
           
            
               
               //FBM starts here
                for (int i = 0; i < octaves; i++) {
                   float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
                   float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;
                   // float sampleX = (x-halfWidth) * fr + octaveOffsets[i].x;
                   // float sampleY = (y-halfHeight) * 1.2f + octaveOffsets[i].y;
                   Vector2 point = new Vector2(sampleX, sampleY);
                   // float noiseValue = noise.GetNoise(sampleX, sampleY) * 2 - 1;
                   noiseValue = noise.GetNoise(point.x, point.y);
                
                   if (applyRidges)
                   {
                      float n = Mathf.Abs(noiseValue) * amplitude;
                      n = 1f - n;
                      noiseHeight += n * n;
                   }
                   else
                   {
                      noiseHeight += noiseValue * amplitude;
                   }
                
                   amplitude *= persistance;
                   frequency *= lacunarity;
                }
                
                if (noiseHeight > maxNoiseHeight) {
                   maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight) {
                   minNoiseHeight = noiseHeight;
                }
                noiseMap [x, y] = noiseHeight;
 
              // noiseMap[x, y] = SimpleFBM(new Vector2(x, y));
 
 
         }
      }
      //This loop purpose is to get the values back to 0-1
       for (int y = 0; y < mapHeight; y++) {
          for (int x = 0; x < mapWidth; x++) {
             noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
          }
       }
 
      return noiseMap;
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
 public static float[] GenerateNoiseMap2(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset,HeightMapGenerator.NoiseType noiseType, bool applyRidges, NoiseSettings [] noises)
 {
    mapWidth_ = mapWidth;
    mapHeight_ = mapHeight;
    scale /= 60f;
    scale_ = scale;
    octaves_ = octaves;
      noiseMap2 = new float[mapWidth * mapHeight];
      // Use the seed to generate the same map regarding the given seed
      System.Random prng = new System.Random (seed);
      // Give an offset to each octave in order to sample them from random different locations
      Vector2[] octaveOffsets = new Vector2[octaves];
      for (int i = 0; i < octaves; i++) {
         float offsetX = prng.Next (-100000, 100000) + offset.x;
         float offsetY = prng.Next (-100000, 100000) + offset.y;
         octaveOffsets [i] = new Vector2 (offsetX, offsetY);
      }
      octaveOffsets_ = octaveOffsets;

      // Create and configure FastNoise object
      // Currently reducing the scale because of the apparent huge difference in size between
      // the unity perlin noise and Fastnoiselite opensimplexnoise
      // if (noiseType == HeightMapGenerator.NoiseType.SIMPLEXNOISE)
      // {
      //    //scale /= 60f;
      //    noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
      // }
      // else if (noiseType == HeightMapGenerator.NoiseType.PERLINNOISE)
      // {
      //    noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
      // }
      // else if (noiseType == HeightMapGenerator.NoiseType.CELLULARNOISE)
      // {
      //    noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
      // }
      // else if (noiseType == HeightMapGenerator.NoiseType.CUBICNOISE)
      // {
      //    noise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
      // }
      // else if (noiseType == HeightMapGenerator.NoiseType.VALUENOISE)
      // {
      //    noise.SetNoiseType(FastNoiseLite.NoiseType.Value);
      // }
      
      if (scale <= 0) {
         scale = 0.0001f;
      }
      
      

      float maxNoiseHeight = float.MinValue;
      float minNoiseHeight = float.MaxValue;
      
      
      
      

      // Calculate the half witdh and half heigth in order to zoom in the center of the map instead of into the corner
      float halfWidth = mapWidth *0.5f;
      float halfHeight = mapHeight *0.5f;
      float noiseValue;
      
      //Loop for every cell of the terrain
      for (int y = 0; y < mapHeight; ++y) {
         for (int x = 0; x < mapWidth; ++x) {

            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;
            
            // float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
            // float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;

            //Vector2 point = new Vector2(x, y);
           
           
            
               
               //FBM starts here
                for (int i = 0; i < octaves; i++) {
                   // float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
                   // float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;

                   float sampleX = (x - halfWidth) / scale;
                   float sampleY = (y - halfHeight) / scale;
                   
                   // float sampleX = (x-halfWidth) * fr + octaveOffsets[i].x;
                   // float sampleY = (y-halfHeight) * 1.2f + octaveOffsets[i].y;
                   Vector2 point = new Vector2(sampleX, sampleY);
                   // float noiseValue = noise.GetNoise(sampleX, sampleY) * 2 - 1;

                   HeightMapGenerator.NoiseType thisNoiseType = noises[i+1].noiseType;
                   SetCorrectNoiseType(thisNoiseType);
                   
                   //test some things here
                   noiseValue = noise.GetNoise(point.x * frequency + octaveOffsets[i].x , point.y * frequency + octaveOffsets[i].y);
                   
                   //noiseValue = noise.GetNoise(point.x, point.y);
                
                   if (noises[i+1].applyRidge)
                   {
                      float n = Mathf.Abs(noiseValue) * amplitude;
                      n = 1f - n;
                      noiseHeight += n * n;
                   }
                   else
                   {
                      
                      noiseHeight += noiseValue * amplitude; //strength
                   }
                
                   amplitude *= persistance; //strength
                   frequency *= lacunarity; //roughness
                }
                
                if (noiseHeight > maxNoiseHeight) {
                   maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight) {
                   minNoiseHeight = noiseHeight;
                }

                //Used to make the heightmap able to lower into the original plane as min value grow
                noiseHeight = Mathf.Max(0, noiseHeight - noises[1].minValue);
                noiseMap2[y * mapHeight + x] = noiseHeight;

                // noiseMap[x, y] = SimpleFBM(new Vector2(x, y));


         }
      }
      //This loop purpose is to get the values back to 0-1
      if (maxNoiseHeight != minNoiseHeight)
      {
         for (int i = 0; i < noiseMap2.Length; ++i)
         {
            noiseMap2[i] = (noiseMap2[i] - minNoiseHeight) / (maxNoiseHeight - minNoiseHeight);
         }
      }
      return noiseMap2;
   }
 

   static float SimpleFBM(Vector2 point)
   {
      float noiseSum = 0;
      float amplitude = 1;
      float frequency = 1;
      
      
      for (int i = 0; i < octaves_; ++i)
      {
         // float sampleX = (point.x-mapWidth_ *0.5f) / scale_ * frequency + octaveOffsets_[i].x;
         // float sampleY = (point.y-mapHeight_ * 0.5f) / scale_ * frequency + octaveOffsets_[i].y;
        // noiseSum += noise.GetNoise(point.x * frequency, point.y * frequency) * amplitude;
         noiseSum += noise.GetNoise(point.x * frequency, point.y * frequency);
         Debug.Log(noiseSum);
         frequency *= 2;
         amplitude *= 0.5f;
      }
      
      if (noiseSum > maxNoiseHeight_) {
         maxNoiseHeight_ = noiseSum;
      } else if (noiseSum < minNoiseHeight_) {
         minNoiseHeight_ = noiseSum;
      }

      return noiseSum;
   }

// currently not working : do not use
   static float FBM(Vector2 point)
   {
      float amplitude = 1;
      float frequency = 1;
      float noiseHeight = 0;
      
      for (int i = 0; i < octaves_; i++) {
         // float noiseValue = noise.GetNoise(sampleX, sampleY) * 2 - 1;
         float noiseValue = noise.GetNoise(point.x, point.y);

         bool applyRidges = false;
         if (applyRidges)
         {
            float n = Mathf.Abs(noiseValue) * amplitude;
            n = 1f - n;
            noiseHeight += n * n;
         }
         else
         {
            noiseHeight += noiseValue * amplitude;
         }
               
         amplitude *= 0.5f;
         frequency *= 2;
      }

      // if (noiseHeight > maxNoiseHeight) {
      //    maxNoiseHeight = noiseHeight;
      // } else if (noiseHeight < minNoiseHeight) {
      //    minNoiseHeight = noiseHeight;
      // }

      return noiseHeight;
   }
   
   
   

  public static float Warping(Vector2 point)
   {
      Vector2 offset1 = new Vector2(4.2f, 0.5f);
      Vector2 offset2 = new Vector2(5.2f, 1.3f);

      Vector2 q = new Vector2(SimpleFBM(point + offset1),
                              SimpleFBM(point + offset2));

     // return SimpleFBM(point + 5.0f * q);
      return SimpleFBM(point +q * 5.0f);

   }

 // static  private float[,] m = new float[2, 2] { { 0.8f, -0.6f }, { .6f, 0.8f } };
 //  float DerivativeFBM(Vector2 point)
 //  {
 //     float a = 0;
 //     float b = 1.0f;
 //     Vector2 d = new Vector2(0, 0);
 //
 //     for (int i = 0; i <= 15; ++i)
 //     {
 //        Vector3 n = noise.GetNoise(point.x, point.y,0);
 //        Debug.Log(Mathf.Floor(10.0F));   // Prints  10
 //     }
 //     
 //  }
   
}