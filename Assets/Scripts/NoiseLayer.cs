using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable] public class NoiseLayer
{
    public NoiseSettings noiseSettings;
    private float[] heightmap_;
    [Range(0f,1f)]
    [SerializeField] public float minValue;

    public void SetHeightmap(float[] heightmap)
    {
        heightmap_ = heightmap;
    }

    public float[] GetHeightmap()
    {
        return heightmap_;
    }
}
