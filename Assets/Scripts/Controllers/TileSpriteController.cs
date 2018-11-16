//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileSpriteController : MonoBehaviour {

    // Bind data to a GameObject
    Dictionary<Tile, GameObject> tileGameObjectMap;

    [Header("Floor tile sprite")]
    public Sprite floorSprite;

    [Header("Empty tile sprite")]
    public Sprite emptySprite;

    [Header("Water tile sprite")]
    public Sprite waterSprite;

    [Header("Sand tile sprite")]
    public Sprite sandSprite;

    [Header("Oild tile sprite")]
    public Sprite oilSprite;

    // Sprites for watertiles
    Sprite[] waterTileSprites;

    // Bind sprites with a naming convention of the water tiles
    Dictionary<string, Sprite> waterTileSpriteMap;

    // Sprites for tiles
    Sprite[] tileSprites;

    // Bind sprites with a naming convention of the tiles
    Dictionary<string, Sprite> tileSpriteMap;

    // Get reference to the World
    World World { get { return WorldController.Instance.World; } }

    void Start () {

        // Instatiate dictionary that binds a name with a sprite 
        waterTileSpriteMap = new Dictionary<string, Sprite>();
        tileSpriteMap = new Dictionary<string, Sprite>();

        // Load all water tile sprites
        LoadWaterTileSprites();

        // Load all tile sprites
        LoadTileSprites();

        // Instatiate dictionary that binds a GameObject with data 
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Create a GameObject for each tile
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                // Get the tile data
                Tile tile_Data = World.GetTileAt(x, y);

                // Creating new gameObject
                GameObject tile_GameObject = new GameObject();

                // Add tile_Data and tile_GameObject to the dictionary (tile_Data is the key)
                tileGameObjectMap.Add(tile_Data, tile_GameObject);

                // Adding a name and position to each tile_gameObject
                tile_GameObject.name = "Tile_" + x + "_" + y;
                tile_GameObject.transform.position = new Vector2(tile_Data.X, tile_Data.Y);
                // Setting the new tile as a child, maintaining a clean hierarchy
                tile_GameObject.transform.SetParent(this.transform, true);

                // Add SpriteRenderer and default empty sprite to each tile_gameObject
                tile_GameObject.AddComponent<SpriteRenderer>().sprite = emptySprite;

                // Trigger callback action
                OnTileChanged(tile_Data);
            }
        }

        // Register action, which will run the funtion when tile_data gets changed
        World.RegisterTileChanged(OnTileChanged);
	}

    /// <summary>
    /// Load water tile sprite from resources folder
    /// </summary>
    private void LoadWaterTileSprites()
    {
        // Loading all sprites and adding them to the dictionary
        waterTileSprites = Resources.LoadAll<Sprite>("Sprites/WaterTiles");
        foreach (Sprite sprite in waterTileSprites)
            waterTileSpriteMap[sprite.name] = sprite;
    }

    /// <summary>
    /// Load all tile sprites from resource folder
    /// </summary>
    private void LoadTileSprites()
    {
        // Loading all sprites and adding them to the dictionary
        tileSprites = Resources.LoadAll<Sprite>("Sprites");
        foreach (Sprite sprite in tileSprites)
            tileSpriteMap[sprite.name] = sprite;
    }

    #region CURRENTLY NOT IN USE - Unbind pairs in dictionary
    /// <summary>
    /// Unpair all tile_Data and tile GameObjects from the dictionary.
    /// Destroy tile GameObject, the visual part in-game.
    /// Can be used when creating new level and the old levels need to be removed.
    /// </summary>
    void DestroyAllTileGameObjects()
    {
        // Run while there are pair in the dictionary
        while (tileGameObjectMap.Count > 0)
        {
            // Grab first pair from dictionary
            Tile tile_Data = tileGameObjectMap.Keys.First();
            GameObject tile_GameObject = tileGameObjectMap[tile_Data];

            // Unpair tile_Data and tile_GameObject from dictionary, thus shrinking the dictionary
            tileGameObjectMap.Remove(tile_Data);

            tile_Data.UnregisterTileTypeChangedCallback(OnTileChanged);
            Destroy(tile_GameObject);
        }

        // TODO: Create new level...or something else
    }
    #endregion

    /// <summary>
    /// Change the tile sprite upon changing its tileType.
    /// </summary>
    /// <param name="tile_Data">The Tile</param>
    /// <param name="tile_GameObject">The GameObject of the Tile</param>
    void OnTileChanged(Tile tile_Data)
    {
        // Check if tileGameObjectMap actually contains the tile_Data key
        if (!tileGameObjectMap.ContainsKey(tile_Data))
        {
            Debug.LogError("tileGameObjectMap doesn't contain the tile_Data -- did you forget to add the tile to the dictionary? Or forgot to unregister a callback?");
            return;
        }

        // Grabbing tile_GameObject from the dictionary. Using the tile_Data as the key
        GameObject tile_GameObject = tileGameObjectMap[tile_Data];

        // Check if the returned GameObject from the dictionary isn't null
        if (tile_GameObject == null)
        {
            Debug.LogError("tileGameObjectMap's returned GameObject is null -- did you forget to add the tile to the dictionary? Or forgot to unregister a callback?");
            return;
        }

        // Change to correct tile Sprite
        if (tile_Data.Type == TileType.Floor)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = floorSprite;
        else if (tile_Data.Type == TileType.Sand)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = GetCorrectSprite(tile_Data); // Sand correct tile depending on neighbouring tiles
        else if (tile_Data.Type == TileType.Oil)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = oilSprite;
        else if (tile_Data.Type == TileType.Empty)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = emptySprite;
        else if (tile_Data.Type == TileType.Water)
            SetWaterTileSprite(tile_Data, tile_GameObject);
        else
            Debug.LogError("OnTileTypeChanged - Unknown tile type.");
    }


    private void SetWaterTileSprite(Tile tile, GameObject tile_GameObject)
    {
        // The tile-sprite name
        string tileCode = string.Empty;

        // Neighbouring tiles to check
        Tile checkTile;

        // Loop through all neighbouring tiles
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Prevent checking the starting tile => don't change tileCode for the starting tile, this would result in a 9 character string instead of 8 or less
                if (x != 0 || y != 0)
                {
                    // If a neighbour has water, add a "W" (for water) else add a "E" (for empty/earth)
                    //if (HasWater(tilemap, new Vector3Int(location.x + x, location.y + y, location.z)))

                    checkTile = tile.World.GetTileAt(tile.X + x, tile.Y + y);

                    // If the tile is null, that means there is no tile (edge of map)
                    if (checkTile == null)
                        tileCode += 'E';

                    // If we get here it means checkTile is NOT null
                    else if (checkTile.Type == TileType.Water || checkTile.Type == TileType.Oil)
                        tileCode += 'W';

                    // It's not the desired tile but still a tile
                    else
                    {
                        tileCode += 'E';
                    }
                }
            }
        }

        tile_GameObject.name += " => " + tileCode;

        // Add randomness to the water tiles
        // Works because this function only get's called for neighbours and not all tiles.
        // Also, full water tiles never get set else where
        int randomVal = Random.Range(0, 100);

        if (randomVal < 15)
        {
            // 15% chance for lilly-pad
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["46"];
        }
        else if (randomVal >= 15 && randomVal < 35)
        {
            // 20% chance for wave
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["48"];
        }
        else
        {
            // Rest is normal water
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["47"];
        }

        // FIXME: Change sprite names so this monster of a IF-statement can be removed!

        // Check the tileCode and set the tile sprite according to the tileCode
        #region Finding correct sprite
        if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["0"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'W' && tileCode[6] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["1"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'E' && tileCode[6] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["2"];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'W' && tileCode[6] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["3"];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["4"];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["5"];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'W' && tileCode[6] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["6"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[5] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["7"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[5] == 'E' && tileCode[7] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["8"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[5] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["9"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[5] == 'E' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["10"];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["11"];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["12"];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["13"];
        }
        else if (tileCode[0] == 'W' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'E' && tileCode[6] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["14"];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["15"];
        }
        else if (tileCode[0] == 'E' && tileCode[1] == 'W' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[5] == 'E' && tileCode[6] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["16"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["17"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["18"];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[4] == 'W' && tileCode[3] == 'E' && tileCode[6] == 'W' && tileCode[7] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["19"];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["20"];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["21"];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'W' && tileCode[7] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["22"];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'W' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["23"];
        }
        else if (tileCode[1] == 'W' && tileCode[2] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'W' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["24"];
        }
        else if (tileCode[1] == 'W' && tileCode[3] == 'E' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["25"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[4] == 'E' && tileCode[6] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["26"];
        }
        else if (tileCode[1] == 'W' && tileCode[3] == 'E' && tileCode[4] == 'E' && tileCode[6] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["27"];
        }
        else if (tileCode[1] == 'E' && tileCode[4] == 'W' && tileCode[3] == 'W' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["28"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'E' && tileCode[6] == 'E' && tileCode[4] == 'W')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["29"];
        }
        else if (tileCode[1] == 'E' && tileCode[3] == 'W' && tileCode[4] == 'E' && tileCode[6] == 'E')
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["30"];
        }
        else if (tileCode == "EWWWWEWW")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["31"];
        }
        else if (tileCode == "EWEWWWWE")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["32"];
        }
        else if (tileCode == "EWEWWWWW")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["33"];
        }
        else if (tileCode == "WWWWWEWW")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["34"];
        }
        else if (tileCode == "WWEWWWWE")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["35"];
        }
        else if (tileCode == "WWWWWWWE")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["36"];
        }
        else if (tileCode == "EWWWWWWW")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["37"];
        }
        else if (tileCode == "WWEWWWWW")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["38"];
        }
        else if (tileCode == "EWWWWWWE")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["39"];
        }
        else if (tileCode == "EWWWWEWE")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["40"];
        }
        else if (tileCode == "WWWWWEWE")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["41"];
        }
        else if (tileCode == "WWEWWEWW")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["42"];
        }
        else if (tileCode == "EWEWWEWW")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["43"];
        }
        else if (tileCode == "WWEWWEWE")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["44"];
        }
        else if (tileCode == "EWEWWEWE")
        {
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = waterTileSpriteMap["45"];
        }
        #endregion
    }



    private Sprite GetCorrectSprite(Tile tile)
    {
        string spriteName = tile.Type.ToString() + "_";

        /* Check for neighbours: North, East, South & West (in that order)
        * Check if: there are neighbouring tiles, 
        * if those tiles have installedObject on them 
        * if those objects are of the same type. */

        //Tile tile;
        int x = tile.X;
        int y = tile.Y;

        Tile neighbour;

        // Check North
        neighbour = World.GetTileAt(x, (y + 1));
        if (TileCheck(neighbour, tile))
            spriteName += "N";

        // Check East
        neighbour = World.GetTileAt((x + 1), y);
        if (TileCheck(neighbour, tile))
            spriteName += "E";

        // Check South
        neighbour = World.GetTileAt(x, (y - 1));
        if (TileCheck(neighbour, tile))
            spriteName += "S";

        // Check West
        neighbour = World.GetTileAt((x - 1), y);
        if (TileCheck(neighbour, tile))
            spriteName += "W";

        // Return correct sprite
        return tileSpriteMap[spriteName];
    }

    /// <summary>
    /// Checks if check- and reference tile are not null and if the tile-types are of the same type.
    /// </summary>
    /// <param name="checkTile">Neighbouring tile</param>
    /// <param name="referenceTile">Center tile</param>
    /// <returns>True if neither tile is null and both tileTypes are the same.</returns>
    public bool TileCheck(Tile checkTile, Tile referenceTile)
    {
        return (checkTile != null && referenceTile != null && checkTile.Type == referenceTile.Type);
    }
}
