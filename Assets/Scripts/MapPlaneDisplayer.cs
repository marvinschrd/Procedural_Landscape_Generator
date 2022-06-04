using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPlaneDisplayer : MonoBehaviour
{

    [SerializeField] private Renderer planeTextureRenderer;

    // Draw noise map on the display plane
    public void DrawNoiseMasp(float[,] noiseMap)
    {
        int witdh = noiseMap.GetLength(0);
        int heigth = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(witdh, heigth);

        Color[] colorMap = new Color[witdh * heigth];

        for (int y = 0; y < heigth; ++y)
        {
            for (int x = 0; x < witdh; ++x)
            {
                colorMap[witdh * y + x] = Color.Lerp(Color.black, Color.white, noiseMap[x,y]);
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        planeTextureRenderer.sharedMaterial.mainTexture = texture;
        planeTextureRenderer.transform.localScale = new Vector3(witdh, 1, heigth);
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
