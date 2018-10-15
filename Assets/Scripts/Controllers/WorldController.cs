//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class WorldController : MonoBehaviour {

    // Creating an instance of 'WorldController' which is accessible from all classes
    public static WorldController Instance { get; protected set; }

    [Header("Floor tile sprite")]
    public Sprite floorSprite;

    // The world, holds all tile data
    public World World { get; protected set; }

    /// <summary>
    /// Create new world
    /// </summary>
    void Start () {

        // Setting the instance equal to this current one (with check)
        if (Instance != null)
            Debug.LogError("There shouldn't be two world controllers.");
        else
            Instance = this;

        // Create new world with empty tiles
        World = new World();

        // Create a GameObject for each tile
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                // Get the tile data
                Tile tile_Data = World.GetTileAt(x, y);

                // Adding a Tile script, name and postion to each tile_gameObject
                GameObject tile_GameObject = new GameObject();
                tile_GameObject.name = "Tile_" + x + "_" + y;
                tile_GameObject.transform.position = new Vector2(tile_Data.X, tile_Data.Y);
                // Setting the new tile as a child, maintaining a clean hierarchy
                tile_GameObject.transform.SetParent(this.transform, true);

                // Add SpriteRenderer to each tile_gameObject
                tile_GameObject.AddComponent<SpriteRenderer>();

                // Register action using Lambda, which will run the funtion when 'tile' gets called
                tile_Data.RegisterTileTypeChangedCallback( (tile) => { OnTileTypeChanged(tile, tile_GameObject); });
            }
        }

        // Randomize all tiles in the world this was just created (thus calling the 'OnTileTypeChanged' function for each tile)
        World.RandomizeTiles();
	}

    /// <summary>
    /// Change the tile sprite upon changing its tileType.
    /// </summary>
    /// <param name="tile_Data">The Tile</param>
    /// <param name="tile_GameObject">The GameObject of the Tile</param>
    void OnTileTypeChanged(Tile tile_Data, GameObject tile_GameObject)
    {
        if (tile_Data.Type == Tile.TileType.Floor)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = floorSprite;
        else if (tile_Data.Type == Tile.TileType.Empty)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = null;
        else
            Debug.LogError("OnTileTypeChanged - Unknown tile type.");
    }

    /// <summary>
    /// Check which tile the mouse hovers over and then return that tile
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>Return a tile at the current mouse coordinates</returns>
    public Tile GetTileAtWorldCoordinate(Vector2 coordinates)
    {
        // Round float to int, since the tiles are all 1 by 1 unit
        int x = Mathf.FloorToInt(coordinates.x);
        int y = Mathf.FloorToInt(coordinates.y);

        return World.GetTileAt(x, y);
    }
}
