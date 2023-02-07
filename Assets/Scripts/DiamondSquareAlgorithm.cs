using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Random = System.Random;

public static class DiamondSquareAlgorithm 
{

	private static int sizeXY; //map[sizeXY, sizeXy]
	private static float[,] map_;
	private static int InValues = 10;

	static public float [,] DiamondSquareMap(int size)
	{
		sizeXY = size;
		map_ = new float[sizeXY, sizeXY];
		map_[0, 0] = InValues;
		map_[sizeXY - 1, sizeXY -1] = InValues;
		map_[0, sizeXY - 1] = InValues;
		map_[sizeXY - 1, 0] = InValues;

		GenerateMap();

		return map_;
	}

	static void GenerateMap()
	{
		float maxValue = 0.0f;
		diamondSquare(sizeXY);
		for (int y = 0; y < sizeXY; ++y)
		{
			for (int x = 0; x < sizeXY; ++x)
			{
				if (map_[x, y] > maxValue) maxValue = map_[x, y];
			}
		}
	}
	
	static void diamondSquare(int size)
	{

		int half = size /2;
		if (half < 1) return;

		for (int y = half; y < sizeXY; y += size)
		{
			for (int x = half; x <sizeXY; x += size)
			{
				DiamondStep(x, y, half);
			}
		}

		diamondSquare(half);
	}

	static void DiamondStep(int x, int y, int half)
	{
		float value = 0.0f;
		value += map_[x + half, y - half];
		value += map_[x - half, y + half];
		value += map_[x + half, y + half];
		value += map_[x - half, y - half];

		value += Random.Range(0, half * 2) - half;
		value /= 4;
		map_[x, y] = value;
		
		SquareStep(x- half, y, half);
		SquareStep(x+ half, y, half);
		SquareStep(x, y - half, half);
		SquareStep(x, y + half, half);
	}
	static void SquareStep(int x, int y, int half)
	{
		float value = 0.0f;
		int cont = 0;
		if (x - half >= 0)
		{
			value += map_[x - half, y];
			cont++;
		}
		if (x + half < sizeXY)
		{
			value += map_[x + half, y];
			cont++;
		}
		if (y - half >= 0)
		{
			value += map_[x, y - half];
			cont++;
		}
		if (y + half < sizeXY)
		{
			value += map_[x, y + half];
			cont++;
		}

		value += Random.Range(0, half * 2) - half;
		value /= cont;
		map_[x, y] = value;
	}
	
}

