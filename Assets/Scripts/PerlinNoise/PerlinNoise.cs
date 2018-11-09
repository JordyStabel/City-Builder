//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public static class PerlinNoise {

    /// <summary>
    /// Create a perlin noise map, with a given width and height
    /// Scale will change the 'texture' of the noise map.
    /// </summary>
    /// <param name="width">Number of points in the X-axis</param>
    /// <param name="height">Number of points in the Y-axis</param>
    /// <param name="scale">Noise value multiplier</param>
    /// <returns></returns>
	public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int numberOctaves, float persistance, float lacunarity, Vector2 offset)
    {
        // Create new 2d float array with size width and height
        float[,] perlinNoiseMap = new float[width, height];

        // Create a random based of the seed
        System.Random random = new System.Random(seed);

        // Generate a array of random value for the octave offset
        Vector2[] octaveOffsets = new Vector2[numberOctaves];
        for (int i = 0; i < numberOctaves; i++)
        {
            float offsetX = random.Next(-100000, 100000) + offset.x;
            float offsetY = random.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Scale shouldn't be 0 or negative (can't devide by 0 and a negative number wouldn't work either)
        // Set scale very close to 0
        if (scale <= 0)
            scale = 0.0001f;

        // Add limits to the min and max values of the noiseHeight
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Center point of the noise map
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        // Loop through all points in the world
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                // Loop through all the octaves
                for (int i = 0; i < numberOctaves; i++)
                {
                    // Devide by 'scale' so we end up with non interger values.
                    // Multiply the scale by frequency to add more randomness and more different values
                    // Intergers give the same value in Unity's perlin noise generation
                    // halfWidth- and height => scale from the center point of the map
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    // Create and set perlin noise value for each point in the world
                    // Mathf.PerlinNoise will give a number between 0-1, so multiply by 2 and substract 1 to posible get negative values aswell
                    // Negative values will make the noise map more interesting
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // Check the min and max noiseHeight values
                // This is needed for the next loop and will make sure the noiseValue will always be between 0 and 1
                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;

                // Set the value of noise of this point
                perlinNoiseMap[x, y] = noiseHeight;
            }
        }

        // Loop through all points in the map again
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Mathf.InverseLerp return a value between 0-1
                perlinNoiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, perlinNoiseMap[x, y]);
            }
        }

        return perlinNoiseMap;
    }
}
