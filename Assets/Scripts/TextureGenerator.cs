using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator 
{
   public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
   {
      Texture2D texture = new Texture2D(width, height);
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
}
