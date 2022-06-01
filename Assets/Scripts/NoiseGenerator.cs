using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// This class handle the generation of noise in order to be used to create various HeigtMaps
public class NoiseGenerator  
{
   public static float[,] GenerateNoiseMap(int witdh, int heigth, float scale)
   {
      // 2D array
      float[,] noiseMap = new float[witdh, heigth];

      if (scale <= 0)
      {
         scale = 0.0001f;
      }

      for (int y = 0; y < heigth; ++y)
      {
         for (int x = 0; x < witdh; ++x)
         {
            // divide by scale to get non integer value that will not be the same everytime
            float sampleX = x / scale;
            float sampleY = y / scale;
            
            // Get perlin value for the map point
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

            noiseMap[x, y] = perlinValue;

         }
      }

      return noiseMap;
   }
}
