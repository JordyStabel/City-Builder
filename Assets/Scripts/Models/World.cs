//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class World {

    // A 2D array that holds the tile data.
    Tile[,] tiles;

    // Number of tiles as width in the world
    public int Width { get; protected set; }

    // Number of tiles as height in the world
    public int Height { get; protected set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// Default: width = 100, height = 100
    /// </summary>
    /// <param name="width">Width in number of tiles</param>
    /// <param name="height">Height in number of tiles</param>
    public World (int width = 100, int height = 100)
    {
        // Set width & height
        Width = width;
        Height = height;

        // Create new tile array. Default size: 100 * 100 = 10,000 tiles
        tiles = new Tile[width, height];

        // Instantiate tiles and adding them to the tiles array
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
            }
        }
        Debug.Log("World created with: " + width * height + " tiles (width: " + width + ", height: " + height );
    }

    /// <summary>
    /// Returns tile at certain coordinates
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The tile: <see cref="Tile"/>.</returns>
    public Tile GetTileAt(int x, int y)
    {
        // Check if coordinates are in range of the world
        if (x > Width || x < 0 || y > Height || y < 0)
            return null;

        return tiles[x, y];
    }

    /// <summary>
    /// Test function, randomize tile type
    /// </summary>
    public void RandomizeTiles()
    {
        Debug.Log("Tiles randomized!");

        // For each tile in the world, give it a random tileType
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (Random.Range(0, 2) == 0)
                    tiles[x, y].Type = TileTypes.Empty;
                else
                    tiles[x, y].Type = TileTypes.Floor;
            }
        }
    }
}
