using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class ThermalErosion
{
    static float talus_angle = 0.0078f; // T
    static float magnitude = 0.5f;
   

 public static float[,] Erode(float[,] heightmap, int gridSize)
 {

     for (int iteration = 0; iteration <= 30; ++iteration)
     {


         for (int x = 0; x < gridSize; ++x)
         {
             for (int y = 0; y < gridSize; ++y)
             {
                 float maxDiff = 0;
                 float diffTotal = 0;
                 float h = heightmap[x, y];
                 List<int> lowerNeighbours = new List<int>();
                 List<float> differences = new List<float>();

                 for (int i = 0; i <= 4; ++i)
                 {
                     float diff = 0f;
                     switch (i)
                     {
                         case 1:
                         {
                             diff = x > 0 ? h - heightmap[x - 1, y] : 0;
                         }
                             break;
                         case 2:
                         {
                             diff = y < gridSize - 1 ? h - heightmap[x, y + 1] : 0;
                         }
                             break;
                         case 3:
                         {
                             diff = x < gridSize - 1 ? h - heightmap[x + 1, y] : 0;
                         }
                             break;
                         case 4:
                         {
                             diff = y > 0 ? h - heightmap[x, y - 1] : 0;
                         }
                             break;
                     }

                     if (diff > talus_angle)
                     {
                         if (diff > maxDiff)
                         {
                             maxDiff = diff;
                         }
                         diffTotal += diff;
                         lowerNeighbours.Add(i);
                         differences.Add(diff);
                     }
                 }

                 for (int i = 0; i < lowerNeighbours.Count; ++i)
                 {
                     float diff = differences[i];
                     float amount = magnitude * (maxDiff - talus_angle) * diff / diffTotal;

                     //Check where to move
                     int lowerNeigbourIndex = lowerNeighbours[i];

                     switch (lowerNeigbourIndex)
                     {
                         case 1:
                         {
                             heightmap[x, y] -= amount;
                             heightmap[x - 1, y] += amount;
                         }
                             break;
                         case 2:
                         {
                             heightmap[x, y] -= amount;
                             heightmap[x, y + 1] += amount;
                         }
                             break;
                         case 3:
                         {
                             heightmap[x, y] -= amount;
                             heightmap[x + 1, y] += amount;
                         }
                             break;
                         case 4:
                         {
                             heightmap[x, y] -= amount;
                             heightmap[x, y - 1] += amount;
                         }
                             break;
                     }
                 }
             }
         }
     }
     return heightmap;
 }
}
    

 


 








