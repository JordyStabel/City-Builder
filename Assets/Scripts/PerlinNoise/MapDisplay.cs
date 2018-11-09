//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRenderer;

    public void DrawTexture(Texture2D texture)
    {
        // Set the texture of the mainTexture equal to the perlin texture
        // This is needed so that we can preview the changes without running the game
        textureRenderer.sharedMaterial.mainTexture = texture;

        // Make the plane fit the texture precisely
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
