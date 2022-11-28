using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator 
{
   public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
   {
      Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA64, false);
      texture.filterMode = FilterMode.Point;
      texture.wrapMode = TextureWrapMode.Clamp;
      texture.SetPixels(colorMap);
      texture.Apply();
      Debug.Log(texture.format);
      return texture;
   }
   
   public static Texture2D TextureFromColorMap2(Color[] colorMap, int width, int height)
   {
      Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA64, false);
      texture.filterMode = FilterMode.Point;
      texture.wrapMode = TextureWrapMode.Clamp;
      texture.SetPixels(colorMap);
      texture.Apply();
      return texture;
   }

   public static Texture2D TextureFromHeightMap(float [,] heightMap)
   {
      int witdh = heightMap.GetLength(0);
      int heigth = heightMap.GetLength(1);

      Color[] colorMap = new Color[witdh * heigth];

      for (int y = 0; y < heigth; ++y)
      {
         for (int x = 0; x < witdh; ++x)
         {
            colorMap[witdh * y + x] = Color.Lerp(Color.black, Color.white, heightMap[x,y]);
         }
      }

      return TextureFromColorMap(colorMap, witdh, heigth);
   }
   // For [] heightMap
   public static Texture2D TextureFromHeightMap2(float [] heightMap)
   {
      // int witdh = heightMap.GetLength(0);
      // int heigth = heightMap.GetLength(1);

      Color[] colorMap = new Color[241 * 241];

      for (int y = 0; y < 241; ++y)
      {
         for (int x = 0; x < 241; ++x)
         {
            colorMap[241 * y + x] = Color.Lerp(Color.black, Color.white, heightMap[241 * y + x]);
         }
      }

      return TextureFromColorMap(colorMap, 241, 241);
   }
}
