using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class MeshGenerator 
{
   public static MeshData GenerateMesh(float [,] terrainHeightMap, float heightMultiplier, int levelOfDetail ,AnimationCurve heightCurve, bool useCurve)
   {
      int width = terrainHeightMap.GetLength(0);
      int height = terrainHeightMap.GetLength(1);
      
      float topLeftx = (width - 1)/(-2f);
      float topLeftz = (height - 1) / 2f;


      int meshLevelOfDetailIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
      int verticesPerLine = (width - 1) / meshLevelOfDetailIncrement + 1;
      
      Debug.Log(verticesPerLine);
      
      MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
      int vertexIndex = 0;

      for (int y = 0; y < height; y+= meshLevelOfDetailIncrement)
      {
         for (int x = 0; x < width; x += meshLevelOfDetailIncrement)
         {
            // give the terrain height map value for the y vertice to get height. X and Z values are centered using topleft
            // Y value is multiplied with height multiplier in order to get actual height variation
            if (useCurve)
            {
               meshData.vertices[vertexIndex] = new Vector3(topLeftx + x, heightCurve.Evaluate(terrainHeightMap[x, y])* heightMultiplier,topLeftz - y);
            }
            else
            {
               meshData.vertices[vertexIndex] = new Vector3(topLeftx + x, terrainHeightMap[x, y]* heightMultiplier,topLeftz - y);
            }

            meshData.UVS[vertexIndex] = new Vector2(x / (float)width, y /(float)height);

            // Check to ignore triangles when on the edge of the map
            if (x < width - 1 && y < height - 1)
            {
               // Creating both triangles of the square
               meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine +1, vertexIndex + verticesPerLine);
               meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
            }
            
            vertexIndex++;
         }
      }
       return meshData;
      //return meshData.CreateMesh();
   }
}

public class MeshData
{
   public Vector3[] vertices;
   public int[] triangles;
   public Vector2[] UVS;
   
   
   // MeshData constructor
   public MeshData(int meshWitdh, int meshHeight)
   {
      vertices = new Vector3[meshWitdh * meshHeight];
      // Need to clarifie why it is done like that...
      UVS = new Vector2[meshWitdh * meshHeight];
      triangles = new int[(meshWitdh-1)*(meshHeight-1)*6];
   }
   

   private int triangleIndex;

   // Add triangle using the three vertices
   public void AddTriangle(int a, int b, int c)
   {
      triangles[triangleIndex] = a;
      triangles[triangleIndex+1] = b;
      triangles[triangleIndex+2] = c;

      triangleIndex += 3;
   }

   // Create the actual mesh using mesh data
   public Mesh CreateMesh()
   {
      Mesh mesh = new Mesh();
      mesh.indexFormat = IndexFormat.UInt32;
      mesh.vertices = vertices;
      mesh.triangles = triangles;
      mesh.uv = UVS;
      
      mesh.RecalculateNormals();

      return mesh;
   }
}
