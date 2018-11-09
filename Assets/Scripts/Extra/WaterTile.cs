//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterTile : UnityEngine.Tilemaps.Tile {

    [SerializeField]
    private Sprite[] waterSprites;

    [SerializeField]
    private Sprite preview;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        // Loop through all neighbours (also diagonals)
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Store a temp position for the neighbour
                Vector3Int neighbourPosition = new Vector3Int(position.x + x, position.y + y, position.z);

                // If a neighbour contains water, refresh/update the sprite
                if (HasWater(tilemap, neighbourPosition) == true)
                    tilemap.RefreshTile(neighbourPosition);
            }
        }
    }

    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        // The tile-sprite name
        string tileCode = string.Empty;

        // Loop through all neighbouring tiles
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Prevent checking the starting tile => don't change tileCode for the starting tile, this would result in a 9 character string instead of 8 or less
                if (x != 0 || y != 0)
                {
                    // If a neighbour has water, add a "W" (for water) else add a "E" (for empty/earth)
                    if (HasWater(tilemap, new Vector3Int(location.x + x, location.y + y, location.z)))
                        tileCode += 'W';
                    else
                        tileCode += 'E';
                }
            }
        }

        // Add randomness to the water tiles
        // Works because this function only get's called for neighbours and not all tiles.
        // Also, full water tiles never get set else where
        int randomVal = Random.Range(0, 100);

        if (randomVal < 15)
        {
            // 15% chance for lilly-pad
            tileData.sprite = waterSprites[46];
        }
        else if (randomVal >= 15 && randomVal < 35)
        {
            // 20% chance for wave
            tileData.sprite = waterSprites[48];
        }
        else
        {
            // Rest is normal water
            tileData.sprite = waterSprites[47];
        }

        // Check the tileCode and set the tile sprite according to the tileCode
        #region Finding correct sprite
        if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[0];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'W' && tileCode[6] == 'W')
        {
            tileData.sprite = waterSprites[1];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'E' && tileCode[6] == 'W')
        {
            tileData.sprite = waterSprites[2];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'W' && tileCode[6] == 'W')
        {
            tileData.sprite = waterSprites[3];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[4];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[5];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'W' && tileCode[6] == 'W')
        {
            tileData.sprite = waterSprites[6];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[5] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'W')
        {
            tileData.sprite = waterSprites[7];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[5] == 'E' && tileCode[7] == 'W')
        {
            tileData.sprite = waterSprites[8];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[5] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tileData.sprite = waterSprites[9];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[5] == 'E' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tileData.sprite = waterSprites[10];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[11];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[12];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[13];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'E' && tileCode[6] == 'W')
        {
            tileData.sprite = waterSprites[14];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[15];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'E' && tileCode[6] == 'W')
        {
            tileData.sprite = waterSprites[16];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'W')
        {
            tileData.sprite = waterSprites[17];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tileData.sprite = waterSprites[18];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[4] == 'W' && tileCode[3] == 'E' && tileCode[6] == 'W' && tileCode[7] == 'W')
        {
            tileData.sprite = waterSprites[19];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tileData.sprite = waterSprites[20];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'W')
        {
            tileData.sprite = waterSprites[21];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tileData.sprite = waterSprites[22];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[23];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[24];
        }
        else if (tileCode[1] == 'W' && tileCode[3] == 'E' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[25];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'E' && tileCode[6] == 'W')
        {
            tileData.sprite = waterSprites[26];
        }
        else if (tileCode[1] == 'W' && tileCode[3] == 'E' && tileCode[4] == 'E' && tileCode[6] == 'W')
        {
            tileData.sprite = waterSprites[27];
        }
        else if (tileCode[1] == 'E' && tileCode[4] == 'W' && tileCode[3] == 'W' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[28];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[6] == 'E' && tileCode[4] == 'W')
        {
            tileData.sprite = waterSprites[29];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tileData.sprite = waterSprites[30];
        }
        else if (tileCode == "EWWWWEWW")
        {
            tileData.sprite = waterSprites[31];
        }
        else if (tileCode == "EWEWWWWE")
        {
            tileData.sprite = waterSprites[32];
        }
        else if (tileCode == "EWEWWWWW")
        {
            tileData.sprite = waterSprites[33];
        }
        else if (tileCode == "WWWWWEWW")
        {
            tileData.sprite = waterSprites[34];
        }
        else if (tileCode == "WWEWWWWE")
        {
            tileData.sprite = waterSprites[35];
        }
        else if (tileCode == "WWWWWWWE")
        {
            tileData.sprite = waterSprites[36];
        }
        else if (tileCode == "EWWWWWWW")
        {
            tileData.sprite = waterSprites[37];
        }
        else if (tileCode == "WWEWWWWW")
        {
            tileData.sprite = waterSprites[38];
        }
        else if (tileCode == "EWWWWWWE")
        {
            tileData.sprite = waterSprites[39];
        }
        else if (tileCode == "EWWWWEWE")
        {
            tileData.sprite = waterSprites[40];
        }
        else if (tileCode == "WWWWWEWE")
        {
            tileData.sprite = waterSprites[41];
        }
        else if (tileCode == "WWEWWEWW")
        {
            tileData.sprite = waterSprites[42];
        }
        else if (tileCode == "EWEWWEWW")
        {
            tileData.sprite = waterSprites[43];
        }
        else if (tileCode == "WWEWWEWE")
        {
            tileData.sprite = waterSprites[44];
        }
        else if (tileCode == "EWEWWEWE")
        {
            tileData.sprite = waterSprites[45];
        }
        #endregion
    }

    /// <summary>
    /// Check if a tile is a 'watertile'.
    /// </summary>
    /// <param name="tilemap">The tilemap to check tiles from.</param>
    /// <param name="position">Position of the tile to check.</param>
    /// <returns>Has water or not</returns>
    private bool HasWater(ITilemap tilemap, Vector3Int position)
    {
        // Is the tile that belongs to the given position of type this, which is WaterTile
        return tilemap.GetTile(position) == this;
    }

    // This code only runs if we're in the Unity Editor
#if UNITY_EDITOR

    [MenuItem("Assets/Create/Tiles/WaterTile")]

    public static void CreateWaterTile()
    {
        // Create a path to which watertiles can be saved
        string path = EditorUtility.SaveFilePanelInProject("Save watertile", "New watertile", "asset", "Save watertile", "Assets");

        // Check if there is a path and return if there is no path
        if (path == "")
            return;

        // Create a new watertile at the specified path
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WaterTile>(), path);
    }

#endif
}
