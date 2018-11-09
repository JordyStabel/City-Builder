//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public DrawMode drawMode;

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

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        // Create colormap => one color for each point in the noise map
        Color[] colorMap = new Color[mapWidth * mapHeight];

        // Loop through all values in the noise map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Store the current height value at this point/coordinate
                float currentHeight = noiseMap[x, y];

                // Loop through all different regions
                for (int i = 0; i < regions.Length; i++)
                {
                    // If the current height value belongs to a region => set the color equal to that region's color
                    if (currentHeight <= regions[i].height)
                    {
                        // Save the color
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        // Get reference of the MapDisplay
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();

        // Display the perlin noise map or color map => depending on the drawmode selected
        if (drawMode == DrawMode.NoiseMap)
            mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColorMap)
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
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

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color color;
    }
}
