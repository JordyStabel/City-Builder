using UnityEngine;

public class World {

    // Protected fields
    Tile[,] tiles;
    int width;
    int height;

    // Return width of the world
    public int Width {
        get { return width; }
    }

    // Return height of the world
    public int Height
    {
        get { return height; }
    }

    // World constructor, default: width = 100, height = 100
    public World (int width = 100, int height = 100)
    {
        // Set width & height
        this.width = width;
        this.height = height;

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

    // Returns tile at certain coordinates
    public Tile GetTileAt(int x, int y)
    {
        // Check if coordinates are in range of the world
        if (x > width || x < 0 || y > height || y < 0)
        {
            Debug.LogError("Tile (" + x + "," + y + ") is out of range.");

            return null;
        }
        return tiles[x, y];
    }

    // Test function
    public void RandomizeTiles()
    {
        Debug.Log("Tiles randomized!");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (Random.Range(0, 2) == 0)
                    tiles[x, y].Type = Tile.TileType.Empty;
                else
                    tiles[x, y].Type = Tile.TileType.Floor;
            }
        }
    }
}
