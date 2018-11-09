//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRenderer;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        // Get the width and height from the first & second dimentions of the 2D float array
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        // Create the actual texture
        Texture2D texture = new Texture2D(width, height);

        // Create a array of colors
        // Size of the array is width * height, so each point has it's own color
        Color[] colorMap = new Color[width * height];

        // Loop through all points in the map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create a color that's a shade between black and white, depending on noiseMap value of this point
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        // Set the colors of the texture and apply
        texture.SetPixels(colorMap);
        texture.Apply();

        // Set the texture of the mainTexture equal to the perlin texture
        // This is needed so that we can preview the changes without running the game
        textureRenderer.sharedMaterial.mainTexture = texture;

        // Make the plane fit the texture precisely
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }
}
