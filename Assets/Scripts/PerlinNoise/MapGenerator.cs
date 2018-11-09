//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;

    [Range(0,1)] // Adds a slider in the editor
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;

    public void GenerateMap()
    {
        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        // Get reference of the MapDisplay
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();

        // Display the perlin noise map
        mapDisplay.DrawNoiseMap(noiseMap);
    }

    /// <summary>
    /// This function automatically gets called when a value gets changed.
    /// Prevent values from being set lower than they should be
    /// </summary>
    private void OnValidate()
    {
        if (mapWidth < 1)
            mapWidth = 1;

        if (mapHeight < 1)
            mapHeight = 1;

        if (lacunarity < 1)
            lacunarity = 1;

        if (octaves < 0)
            octaves = 0;
    }
}
