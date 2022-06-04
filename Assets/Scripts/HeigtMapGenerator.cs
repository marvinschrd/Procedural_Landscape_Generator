using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeigtMapGenerator : MonoBehaviour
{
    [Header("Map Values")]
    [SerializeField]
    private int mapWidth = 0;
    [SerializeField]
    private int mapHeigth = 0;
    [SerializeField]
    private float noiseScale = 0f;

    public bool autoUpdateMap = false;



    public void GenerateMap()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeigth, noiseScale);
        
        MapPlaneDisplayer mapDisplay = FindObjectOfType<MapPlaneDisplayer>();
        
        mapDisplay.DrawNoiseMasp(noiseMap);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
