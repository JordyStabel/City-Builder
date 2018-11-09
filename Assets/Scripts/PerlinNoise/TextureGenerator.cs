//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public static class TextureGenerator {

    /// <summary>
    /// Create and return a Texture2D, generated from a given colormap array
    /// </summary>
    /// <param name="colorMap">Input, color array</param>
    /// <param name="width">Width of the texture</param>
    /// <param name="height">Height of the texture</param>
    /// <returns>Texture2D from colormap</returns>
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        // Get the width and height from the first & second dimentions of the 2D float array
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        // Create a array of colors
        // Size of the array is width * height, so each point has it's own color
        Color[] colorMap = new Color[width * height];

        // Loop through all points in the map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create a color that's a shade between black and white, depending on heightMap value of this point
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        // Call the TextureFromColorMap function and return that Texture2D object
        return TextureFromColorMap(colorMap, width, height);
    }
}
