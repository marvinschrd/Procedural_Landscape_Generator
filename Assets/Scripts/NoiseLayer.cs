using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable] public class NoiseLayer
{
    public NoiseSettings noiseSettings;
    private float[] heightmap_;
    [Range(0f,1f)]
    [SerializeField] public float minValue;

    [Range(0f, 3f)]
    [SerializeField] public float strength;

    [SerializeField] public bool warpNoise = false;
    [SerializeField] public float warpDisplacementFactor = 4.0f;
    [SerializeField] public bool enable = true;
    [SerializeField] public bool applyHeightCurve = false;
    [SerializeField] public AnimationCurve layerCurve;
    [SerializeField] public bool applyFallofMap = false;
    public void SetHeightmap(float[] heightmap)
    {
        heightmap_ = heightmap;
    }

    public float[] GetHeightmap()
    {
        return heightmap_;
    }
}
