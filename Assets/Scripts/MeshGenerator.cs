using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator 
{
   public static MeshData GenerateMesh(float [,] terrainHeighMap)
   {
      int width = terrainHeighMap.GetLength(0);
      int height = terrainHeighMap.GetLength(1);
      
      float topLeftx = (width - 1)/(-2f);
      float topLeftz = (height - 1) / 2f;

      MeshData meshData = new MeshData(width, height);
      int vertexIndex = 0;

      for (int y = 0; y < height; ++y)
      {
         for (int x = 0; x < width; ++x)
         {
            // give the terrain height map value for the y vertice value to get height. X and Z values are centered using topleft
            meshData.vertices[vertexIndex] = new Vector3(topLeftx + x,terrainHeighMap[x,y],topLeftz - y);
            meshData.UVS[vertexIndex] = new Vector2(x / (float)width, y /(float)height);

            // Check to ignore triangles when on the edge of the map
            if (x < width - 1 && y < height - 1)
            {
               // Creating both triangles of the square
               meshData.AddTriangle(vertexIndex, vertexIndex + width +1, vertexIndex + width);
               meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
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
      triangles = new int[(meshHeight-1)*(meshWitdh-1)*6];
      UVS = new Vector2[vertices.Length];
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
      mesh.vertices = vertices;
      mesh.triangles = triangles;
      mesh.uv = UVS;
      
      mesh.RecalculateNormals();

      return mesh;
   }
}
