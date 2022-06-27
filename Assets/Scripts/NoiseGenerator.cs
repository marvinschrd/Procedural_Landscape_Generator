using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// This class handle the generation of noise in order to be used to create various HeigtMaps
public class NoiseGenerator
{
  static private float[,] noiseMap;
   public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, HeightMapGenerator.NoiseType noiseType) {
      //float[,] noiseMap = new float[mapWidth,mapHeight];
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

      // Create and configure FastNoise object
      scale /= 60f;
      FastNoiseLite noise = new FastNoiseLite();
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
      float halfWidth = mapWidth / 2f;
      float halfHeight = mapHeight / 2f;


      for (int y = 0; y < mapHeight; y++) {
         for (int x = 0; x < mapWidth; x++) {
		
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            for (int i = 0; i < octaves; i++) {
               float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
               float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;
               
               if (noiseType == HeightMapGenerator.NoiseType.SIMPLEXNOISE)
               {
                  // Gather noise data
                  float simplexValue = noise.GetNoise(sampleX, sampleY) * 2 - 1 ;
                  noiseHeight += simplexValue * amplitude;
               }
               else if (noiseType == HeightMapGenerator.NoiseType.PERLINNOISE)
               {
                  //float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                  float perlinValue = noise.GetNoise(sampleX, sampleY) * 2 - 1;
                  noiseHeight += perlinValue * amplitude;
               }
               else if (noiseType == HeightMapGenerator.NoiseType.CELLULARNOISE)
               {
                  //float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                  float perlinValue = noise.GetNoise(sampleX, sampleY) * 2 - 1;
                  noiseHeight += perlinValue * amplitude;
               }
               else if (noiseType == HeightMapGenerator.NoiseType.CUBICNOISE)
               {
                  //float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                  float perlinValue = noise.GetNoise(sampleX, sampleY) * 2 - 1;
                  noiseHeight += perlinValue * amplitude;
               }
               else if (noiseType == HeightMapGenerator.NoiseType.VALUENOISE)
               {
                  //float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                  float perlinValue = noise.GetNoise(sampleX, sampleY) * 2 - 1;
                  noiseHeight += perlinValue * amplitude;
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
         }
      }

      for (int y = 0; y < mapHeight; y++) {
         for (int x = 0; x < mapWidth; x++) {
            noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
         }
      }

      return noiseMap;
   }
   
   
}
