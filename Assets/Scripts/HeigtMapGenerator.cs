using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeigtMapGenerator : MonoBehaviour
{
    [Header("Map Values")]
    [SerializeField] private int mapWidth = 0;
    [SerializeField] private int mapHeigth = 0;
    [SerializeField] private float noiseScale = 0f;
    [SerializeField] private int octaves = 0;
    
    [Range(0,1)]
    [SerializeField] private float persistance = 0.5f;
    [SerializeField] private float lacunarity = 2f;

    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    
    public bool autoUpdateMap = false;



    public void GenerateMap()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeigth, seed, noiseScale ,octaves, persistance, lacunarity, offset);
        
        MapPlaneDisplayer mapDisplay = FindObjectOfType<MapPlaneDisplayer>();
        
        mapDisplay.DrawNoiseMasp(noiseMap);
    }
    void OnValidate() {
        if (mapWidth < 1) {
            mapWidth = 1;
        }
        if (mapHeigth < 1) {
            mapHeigth = 1;
        }
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }
    }
}
