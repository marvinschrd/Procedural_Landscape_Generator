using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Noise
{


    public HeightMapGenerator.NoiseType noiseType = HeightMapGenerator.NoiseType.PERLINNOISE;
    
    [Header("Map Values")]
    [SerializeField] public float noiseScale = 0f;
    [SerializeField] public int octaves = 0;
    [SerializeField] public float meshHeightMultiplier = 0;

    [Range(0,1)]
    [SerializeField] public float persistance = 0.5f;
    [SerializeField] public float lacunarity = 2f;

    [SerializeField] public int seed;
    [SerializeField] public Vector2 offset;
    
    
}