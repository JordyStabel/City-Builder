using UnityEngine;

public class WorldController : MonoBehaviour {

    [Header("Sprite for floor tile")]
    public Sprite floorSprite;

    World world;
    
    // Create new world
	void Start () {

        // Create new world with empty tiles
        world = new World();

        // Create a GameObject for each tile
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                // Adding a Tile script, name and postion to each tile_gameObject
                GameObject tile_GameObject = new GameObject();
                Tile tile_Data = world.GetTileAt(x, y);
                tile_GameObject.name = "Tile_" + x + "_" + y;
                tile_GameObject.transform.position = new Vector2(tile_Data.X, tile_Data.Y);

                // Add SpriteRenderer to each tile_gameObject
                tile_GameObject.AddComponent<SpriteRenderer>();

                // Register action using Lambda, which will run the funtion when 'tile' gets called
                tile_Data.RegisterTileTypeChangedCallback( (tile) => { OnTileTypeChanged(tile, tile_GameObject); });
            }
        }

        // Randomize all tiles in the world this was just created (thus calling the 'OnTileTypeChanged' function for each tile)
        world.RandomizeTiles();
	}
    
	void Update () {
    }

    // Change the tile sprite upon changing its tileType
    void OnTileTypeChanged(Tile tile_Data, GameObject tile_GameObject)
    {
        if (tile_Data.Type == Tile.TileType.Floor)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = floorSprite;
        else if (tile_Data.Type == Tile.TileType.Empty)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = null;
        else
            Debug.LogError("OnTileTypeChanged - Unknown tile type.");
    }
}
