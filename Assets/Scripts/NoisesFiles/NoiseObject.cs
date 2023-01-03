using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]

public class NoiseSettings
{
    
    public HeightMapGenerator.NoiseType noiseType = HeightMapGenerator.NoiseType.PERLINNOISE;
    
    [Header("Map Values")]
    [SerializeField] public int noiseScale = 0;
    [SerializeField] public int octaves = 1;
    //[SerializeField] public float meshHeightMultiplier = 45f;

    [SerializeField] public NoiseGenerator.NoiseEffect noiseEffect = NoiseGenerator.NoiseEffect.NOEFFECT;
    [Range(0,1)]
    [SerializeField] public float persistance = 0.5f;
    [SerializeField] public float lacunarity = 2f;

    //[SerializeField] public float strength = 1f;
    [Range(0,1)]
    //[SerializeField] public float minValue;

    [SerializeField] public int seed;
    [SerializeField] public Vector2 offset;

}