using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class HydraulicErosion 
{
    static System.Random prng;
    static private int currentSeed;
    static int currentErosionRadius;
    static int currentMapSize;
    
    static int[][] erosionBrushIndices;
    static float[][] erosionBrushWeights;
    
    
    
     // Initialization creates a System.Random object and precomputes indices and weights of erosion brush
   static void Initialize(int mapSize, Erosion erosionParameters, bool resetSeed = false) {
        if (resetSeed || prng == null || currentSeed != erosionParameters.seed) {
            prng = new System.Random (erosionParameters.seed);
            currentSeed = erosionParameters.seed;
        }

        if (erosionBrushIndices == null || currentErosionRadius != erosionParameters.erosionRadius || currentMapSize != mapSize) {
            InitializeBrushIndices (mapSize, erosionParameters.erosionRadius);
            currentErosionRadius = erosionParameters.erosionRadius;
            currentMapSize = mapSize;
        }
    }

    public static void Erode2 (float[] map, int mapSize, Erosion erosionParameters, bool resetSeed = false) {
        Initialize (mapSize, erosionParameters, resetSeed);

        for (int iteration = 0; iteration < erosionParameters.dropletNumber; iteration++) {
            // Create water droplet at random point on map
            float posX = prng.Next (0, mapSize - 1);
            float posY = prng.Next (0, mapSize - 1);
            float dirX = 0;
            float dirY = 0;
            float speed = erosionParameters.initialSpeed;
            float water = erosionParameters.initialWaterVolume;
            float sediment = 0;

            for (int lifetime = 0; lifetime < erosionParameters.dropletLifetime; lifetime++) {
                int nodeX = (int) posX;
                int nodeY = (int) posY;
                int dropletIndex = nodeY * mapSize + nodeX;
                // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
                float cellOffsetX = posX - nodeX;
                float cellOffsetY = posY - nodeY;

                // Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
                HeightAndGradient heightAndGradient = CalculateHeightAndGradient (map, mapSize, posX, posY);

                // Update the droplet's direction and position (move position 1 unit regardless of speed)
                dirX = (dirX * erosionParameters.inertia - heightAndGradient.surfaceNormalX * (1 - erosionParameters.inertia));
                dirY = (dirY * erosionParameters.inertia - heightAndGradient.surfaceNormalY * (1 - erosionParameters.inertia));
                // Normalize direction
                float len = Mathf.Sqrt (dirX * dirX + dirY * dirY);
                if (len != 0) {
                    dirX /= len;
                    dirY /= len;
                }
                posX += dirX;
                posY += dirY;

                // Stop simulating droplet if it's not moving or has flowed over edge of map
                if ((dirX == 0 && dirY == 0) || posX < 0 || posX >= mapSize - 1 || posY < 0 || posY >= mapSize - 1) {
                    break;
                }

                // Find the droplet's new height and calculate the deltaHeight
                float newHeight = CalculateHeightAndGradient (map, mapSize, posX, posY).height;
                float deltaHeight = newHeight - heightAndGradient.height;

                // Calculate the droplet's sediment capacity (higher when moving fast down a slope and contains lots of water)
                float sedimentCapacity = Mathf.Max (-deltaHeight * speed * water * erosionParameters.sedimentCapacityFactor, erosionParameters.minSedimentCapacity);

                // If carrying more sediment than capacity, or if flowing uphill:
                if (sediment > sedimentCapacity || deltaHeight > 0) {
                    // If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
                    float amountToDeposit = (deltaHeight > 0) ? Mathf.Min (deltaHeight, sediment) : (sediment - sedimentCapacity) * erosionParameters.depositSpeed;
                    sediment -= amountToDeposit;

                    // Add the sediment to the four nodes of the current cell using bilinear interpolation
                    // Deposition is not distributed over a radius (like erosion) so that it can fill small pits
                    map[dropletIndex] += amountToDeposit * (1 - cellOffsetX) * (1 - cellOffsetY);
                    map[dropletIndex + 1] += amountToDeposit * cellOffsetX * (1 - cellOffsetY);
                    map[dropletIndex + mapSize] += amountToDeposit * (1 - cellOffsetX) * cellOffsetY;
                    map[dropletIndex + mapSize + 1] += amountToDeposit * cellOffsetX * cellOffsetY;

                } else {
                    // Erode a fraction of the droplet's current carry capacity.
                    // Clamp the erosion to the change in height so that it doesn't dig a hole in the terrain behind the droplet
                    float amountToErode = Mathf.Min ((sedimentCapacity - sediment) * erosionParameters.erosionRate, -deltaHeight);

                    // Use erosion brush to erode from all nodes inside the droplet's erosion radius
                    for (int brushPointIndex = 0; brushPointIndex < erosionBrushIndices[dropletIndex].Length; brushPointIndex++) {
                        int nodeIndex = erosionBrushIndices[dropletIndex][brushPointIndex];
                        float weighedErodeAmount = amountToErode * erosionBrushWeights[dropletIndex][brushPointIndex];
                        float deltaSediment = (map[nodeIndex] < weighedErodeAmount) ? map[nodeIndex] : weighedErodeAmount;
                        map[nodeIndex] -= deltaSediment;
                        sediment += deltaSediment;
                    }
                }

                // Update droplet's speed and water content
                speed = Mathf.Sqrt (speed * speed + deltaHeight * erosionParameters.gravity);
                water *= (1 - erosionParameters.evaporateSpeed);
            }
        }
    }
    static void InitializeBrushIndices (int mapSize, int radius) {
        erosionBrushIndices = new int[mapSize * mapSize][];
        erosionBrushWeights = new float[mapSize * mapSize][];

        int[] xOffsets = new int[radius * radius * 4];
        int[] yOffsets = new int[radius * radius * 4];
        float[] weights = new float[radius * radius * 4];
        float weightSum = 0;
        int addIndex = 0;

        for (int i = 0; i < erosionBrushIndices.GetLength (0); i++) {
            int centreX = i % mapSize;
            int centreY = i / mapSize;

            if (centreY <= radius || centreY >= mapSize - radius || centreX <= radius + 1 || centreX >= mapSize - radius) {
                weightSum = 0;
                addIndex = 0;
                for (int y = -radius; y <= radius; y++) {
                    for (int x = -radius; x <= radius; x++) {
                        float sqrDst = x * x + y * y;
                        if (sqrDst < radius * radius) {
                            int coordX = centreX + x;
                            int coordY = centreY + y;

                            if (coordX >= 0 && coordX < mapSize && coordY >= 0 && coordY < mapSize) {
                                float weight = 1 - Mathf.Sqrt (sqrDst) / radius;
                                weightSum += weight;
                                weights[addIndex] = weight;
                                xOffsets[addIndex] = x;
                                yOffsets[addIndex] = y;
                                addIndex++;
                            }
                        }
                    }
                }
            }

            int numEntries = addIndex;
            erosionBrushIndices[i] = new int[numEntries];
            erosionBrushWeights[i] = new float[numEntries];

            for (int j = 0; j < numEntries; j++) {
                erosionBrushIndices[i][j] = (yOffsets[j] + centreY) * mapSize + xOffsets[j] + centreX;
                erosionBrushWeights[i][j] = weights[j] / weightSum;
            }
        }
    }
    
    
    
    public static void Erode(float[] heightmap, int mapSize, Erosion erosionParameters)
    {
        prng = new System.Random (erosionParameters.seed);
        //One iteration per droplet
        for (int i = 0; i < erosionParameters.dropletNumber; ++i)
            
        {
            float positionX = prng.Next(0, mapSize - 1); // random x position
            float positionY = prng.Next(0, mapSize - 1); // random y position
            float velocityX = 0f;
            float velocityY = 0f;
            float speed = erosionParameters.initialVelocity;
            float sediment = 0f; // amount of carried sediment

            float previousX = 0;
            float previousY = 0;

            // One iteration per droplet step(life step) until lifetime is over
            for (int lifetime = 0; lifetime <erosionParameters.dropletLifetime; ++lifetime)
            {
                int nodeX = (int) positionX; // nodes based on the droplet position
                int nodeY = (int) positionY;
                int dropletIndex = nodeY * mapSize + nodeX; // index of the droplet on the flat array representing the heightmap grid
                // Calculate droplet's offset inside the cell to find in which node the droplet is
                float cellOffsetX = positionX - nodeX;
                float cellOffsetY = positionY - nodeY;
                
                // Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
                HeightAndGradient heightAndGradient = CalculateHeightAndGradient (heightmap, mapSize, positionX, positionY);
                
                //Calculate droplet velocities
                velocityX = (velocityX - heightAndGradient.surfaceNormalX * speed);
                velocityY = (velocityY - heightAndGradient.height * speed);
                // Normalize direction
                float len = Mathf.Sqrt (velocityX * velocityX + velocityY * velocityY);
                if (len != 0) {
                    velocityX /= len;
                    velocityY /= len;
                }

                previousX = positionX;
                previousY = positionY;

                positionX += velocityX;
                positionY += velocityY;
                
                // Stop simulating droplet if it's not moving or has flowed over edge of map
                if ((velocityX == 0 && velocityY == 0) || positionX < 0 || positionX >= mapSize - 1 || positionY < 0 || positionY >= mapSize - 1) {
                    break;
                }
                
                // Find the droplet's new height and calculate the deltaHeight
                float newDropletHeight = CalculateHeightAndGradient(heightmap, mapSize, positionX, positionY).height;
                float deltaHeight = newDropletHeight - heightAndGradient.height;
                
                //Testing from here
                float deposit = sediment * erosionParameters.depositSpeed * heightAndGradient.surfaceNormalY;
                float erosion = erosionParameters.erosionRate * (1 - heightAndGradient.surfaceNormalY) * Mathf.Min(1, erosionParameters.iterationScale);

                float amountToDeposit = deposit - erosion;
                
                heightmap[dropletIndex] += amountToDeposit * (1 - cellOffsetX) * (1 - cellOffsetY);
                heightmap[dropletIndex + 1] += amountToDeposit * cellOffsetX * (1 - cellOffsetY);
                heightmap[dropletIndex + mapSize] += amountToDeposit * (1 - cellOffsetX) * cellOffsetY;
                heightmap[dropletIndex + mapSize + 1] += amountToDeposit * cellOffsetX * cellOffsetY;

                sediment += erosion - deposit;
            }
        }
    }

    struct HeightAndGradient {
        public float height;
        public float surfaceNormalX;
        public float surfaceNormalY;
    }
    // Billinear interpolation of surrounding heights. Is used to calculate height by using four nodes arround the evaluate grid cell
    static HeightAndGradient CalculateHeightAndGradient (float[] nodes, int mapSize, float posX, float posY) {
        int coordX = (int) posX;
        int coordY = (int) posY;

        // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
        float x = posX - coordX;
        float y = posY - coordY;

        // Calculate heights of the four nodes of the droplet's cell
        int nodeIndexNW = coordY * mapSize + coordX;
        float heightNW = nodes[nodeIndexNW];
        float heightNE = nodes[nodeIndexNW + 1];
        float heightSW = nodes[nodeIndexNW + mapSize];
        float heightSE = nodes[nodeIndexNW + mapSize + 1];

        // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
        float gradientX = (heightNE - heightNW) * (1 - y) + (heightSE - heightSW) * y;
        float gradientY = (heightSW - heightNW) * (1 - x) + (heightSE - heightNE) * x;

        // Calculate height with bilinear interpolation of the heights of the nodes of the cell
        float height = heightNW * (1 - x) * (1 - y) + heightNE * x * (1 - y) + heightSW * (1 - x) * y + heightSE * x * y;

        return new HeightAndGradient () { height = height, surfaceNormalX = gradientX, surfaceNormalY = gradientY };
    }

    //Erosion process but done on a compute shader
    //All the erosion parameters are given to the compute shader and the needed buffer are created
    public static float [] ErodeFromComputeShader(float [] finalMap, int mapSize, Erosion erosionParameters, ComputeShader erosionComputeShader )
    {
        ComputeBuffer mapBuffer = new ComputeBuffer(finalMap.Length, sizeof(float));
        mapBuffer.SetData(finalMap);
        erosionComputeShader.SetBuffer(0,"finalMap", mapBuffer);
        
        int numThreadGroups = erosionParameters.dropletNumber / 1024;
        
        //Set alt the values and buffers
        // ------------------------------------------------------------------------------------------------------
        // Create brush
        List<int> brushIndexOffsets = new List<int> ();
        List<float> brushWeights = new List<float> ();

        float weightSum = 0;
        for (int brushY = -erosionParameters.erosionRadius; brushY <= erosionParameters.erosionRadius; brushY++) {
            for (int brushX = -erosionParameters.erosionRadius; brushX <= erosionParameters.erosionRadius; brushX++) {
                float sqrDst = brushX * brushX + brushY * brushY;
                if (sqrDst < erosionParameters.erosionRadius * erosionParameters.erosionRadius) {
                    brushIndexOffsets.Add (brushY * mapSize + brushX);
                    float brushWeight = 1 - Mathf.Sqrt (sqrDst) / erosionParameters.erosionRadius;
                    weightSum += brushWeight;
                    brushWeights.Add (brushWeight);
                }
            }
        }
        for (int i = 0; i < brushWeights.Count; i++) {
            brushWeights[i] /= weightSum;
        }

        // Send brush data to compute shader
        ComputeBuffer brushIndexBuffer = new ComputeBuffer (brushIndexOffsets.Count, sizeof (int));
        ComputeBuffer brushWeightBuffer = new ComputeBuffer (brushWeights.Count, sizeof (int));
        brushIndexBuffer.SetData (brushIndexOffsets);
        brushWeightBuffer.SetData (brushWeights);
        erosionComputeShader.SetBuffer (0, "brushIndices", brushIndexBuffer);
        erosionComputeShader.SetBuffer (0, "brushWeights", brushWeightBuffer);
        
        // Generate random indices for droplet placement
        int[] randomIndices = new int[erosionParameters.dropletNumber];
        for (int i = 0; i < erosionParameters.dropletNumber; i++) {
            int randomX = Random.Range (erosionParameters.erosionRadius, mapSize + erosionParameters.erosionRadius);
            int randomY = Random.Range (erosionParameters.erosionRadius, mapSize + erosionParameters.erosionRadius);
            randomIndices[i] = randomY * mapSize + randomX;
        }

        // Send random indices to compute shader
        ComputeBuffer randomIndexBuffer = new ComputeBuffer (randomIndices.Length, sizeof (int));
        randomIndexBuffer.SetData (randomIndices);
        erosionComputeShader.SetBuffer (0, "randomIndices", randomIndexBuffer);
        
        erosionComputeShader.SetInt("maxDropletLifeTime",erosionParameters.dropletLifetime);
        erosionComputeShader.SetInt("mapSize",mapSize);
        erosionComputeShader.SetInt ("brushLength", brushIndexOffsets.Count);
        erosionComputeShader.SetInt ("borderSize", erosionParameters.erosionRadius);
        erosionComputeShader.SetFloat("gravity",erosionParameters.gravity);
        erosionComputeShader.SetFloat("inertia",erosionParameters.inertia);
        erosionComputeShader.SetFloat("depositSpeed",erosionParameters.depositSpeed);
        erosionComputeShader.SetFloat("erosionSpeed",erosionParameters.erosionRate);
        erosionComputeShader.SetFloat("evaporationSpeed",erosionParameters.evaporateSpeed);
        erosionComputeShader.SetFloat("initialSpeed",erosionParameters.initialSpeed);
        erosionComputeShader.SetFloat("initialWaterVolume",erosionParameters.initialWaterVolume);
        erosionComputeShader.SetFloat("minSedimentCapacity",erosionParameters.minSedimentCapacity);
        erosionComputeShader.SetFloat("sedimentCapacityFactor",erosionParameters.sedimentCapacityFactor);



        //Dispatch and run the kernel for computation
        erosionComputeShader.Dispatch(0, numThreadGroups, 1, 1);
        mapBuffer.GetData(finalMap);
        
        randomIndexBuffer.Release();
        brushIndexBuffer.Release();
        brushWeightBuffer.Release();
        mapBuffer.Release();

        return finalMap;
    }
    
}
