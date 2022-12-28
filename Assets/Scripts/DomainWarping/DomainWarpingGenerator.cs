using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DomainWarpingGenerator
{
   static FastNoiseLite noise = new FastNoiseLite();
   private static int octaves = 4;
   private static float persistance = 0.18f;
   private static float lacunarity = 3.0f;

   private static float amplitude = 1.0f;
   private static float frequency = 1.0f;
   private static float noiseHeight = 0.0f;
   private static float H = 0.5f;

   private static float scale = 3;
   // Use the seed to generate the same map regarding the given seed
   static System.Random prng = new System.Random (1000);
   
   static public float[] DomainWarping(int width, int heigth)
   {
      float halfWidth = width *0.5f;
      float halfHeight = heigth *0.5f;
      noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
      float[] map = new float[width * heigth];
      
      float maxNoiseHeight = float.MinValue;
      float minNoiseHeight = float.MaxValue;
      
      for (int y = 0; y < width; ++y)
      {
         for (int x = 0; x < heigth; ++x)
         {
            noiseHeight = 0f;

            Vector2 point = new Vector2((x - halfWidth )/ scale, (y - halfHeight) / scale);
            noiseHeight = Pattern(point);
            
            
            if (noiseHeight > maxNoiseHeight) {
               maxNoiseHeight = noiseHeight;
            } else if (noiseHeight < minNoiseHeight) {
               minNoiseHeight = noiseHeight;
            }
            
            map[y * width + x] = noiseHeight;

         }
      }

      //This loop purpose is to get the values back to 0-1
      // if (maxNoiseHeight != minNoiseHeight)
      // {
      //    for (int i = 0; i < map.Length; ++i)
      //    {
      //       map[i] = (map[i] - minNoiseHeight) / (maxNoiseHeight - minNoiseHeight);
      //    }
      // }
      
      return map;
   }

   static float Pattern(Vector2 point)
   {
      //First Warping
      Vector2 P1 = point + new Vector2(100, 200);
      Vector2 P2 = point + new Vector2(5.2f, 1.3f);
      Vector2 q = new Vector2(
         FBM(P1),
         FBM(P2)
      );
      Vector2 q2 = point + 4.0f * q;

      //Second Warping
      Vector2 P3 = point + 4.0f * q + new Vector2(1.7f, 9.2f);
      Vector2 P4 = point + 4.0f * q + new Vector2(8.3f, 2.8f);
      Vector2 r = new Vector2(FBM(P3), FBM(P4));
      
      Vector2 r2 = point + 4.0f * r;
      
      float noiseValue = FBM(r2);
      //Debug.Log(noiseValue);
      return noiseValue;
   }
   
static float FBM(Vector2 point )
{
   float G = Mathf.Pow(2, -H); // G = persitance;
   //float G = 0.6f;
   float f = 1.0f;
   float a = 1.0f;
   float t = 0f;
   for (int i = 0; i <= octaves; ++i)
   {
      float noiseValue = noise.GetNoise(point.x * f, point.y * f);
      t += noiseValue * a;
      //Debug.Log(t);
      a *= G;
      f *= 2.5f;
   }

   return t;
}
}



