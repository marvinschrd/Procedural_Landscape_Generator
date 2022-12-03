using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDatas : MonoBehaviour
{
   
    public TerrainType[] mapRegions;

    
    [System.Serializable]
    public struct TerrainType
    {
        public string terrainName;
        public float height;
        public Color terrainColor;
        public float terrainHardness;
    }
    
    public TerrainType[] GetTerrainTypes()
    {
        return mapRegions;
    }
}
