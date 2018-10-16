//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldController : MonoBehaviour {

    // Creating an instance of 'WorldController' which is accessible from all classes
    public static WorldController Instance { get; protected set; }

    // Bind data to a GameObject
    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<InstalledObject, GameObject> installedObjectGameObjectMap;
    Dictionary<string, Sprite> installedObjectSpritesMap;

    [Header("Floor tile sprite")]
    public Sprite floorSprite;

    [Header("Empty tile sprite")]
    public Sprite emptySprite;

    // Sprites array
    Sprite[] sprites;

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

        // Register function
        World.RegisterInstalledObjectCreated(OnInstalledObjectCreated);

        // Instatiate dictionary that binds a GameObject with data 
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        installedObjectGameObjectMap = new Dictionary<InstalledObject, GameObject>();
        installedObjectSpritesMap = new Dictionary<string, Sprite>();

        LoadSprites();

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

                // Register action, which will run the funtion when 'tile' gets changed
                tile_Data.RegisterTileTypeChangedCallback(OnTileTypeChanged);
            }
        }

        // Center camera in the world
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);

        // Randomize all tiles in the world this was just created (thus calling the 'OnTileTypeChanged' function for each tile)
        //World.RandomizeTiles();
	}

    /// <summary>
    /// Load sprite from resources folder
    /// </summary>
    private void LoadSprites()
    {
        // Loading all sprites and adding them to the dictionary
        sprites = Resources.LoadAll<Sprite>("Sprites/Walls");
        foreach (Sprite sprite in sprites)
            installedObjectSpritesMap[sprite.name] = sprite;
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

            tile_Data.UnregisterTileTypeChangedCallback(OnTileTypeChanged);
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
    void OnTileTypeChanged(Tile tile_Data)
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
        else if (tile_Data.Type == TileType.Empty)
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

    /// <summary>
    /// Create the visual component linked to the given data
    /// </summary>
    /// <param name="installedObject">Functions as the data.</param>
    public void OnInstalledObjectCreated(InstalledObject installedObject)
    {
        // Creating new gameObject
        GameObject installedObject_GameObject = new GameObject();

        // Add data and installedObject_GameObject to the dictionary (data is the key)
        installedObjectGameObjectMap.Add(installedObject, installedObject_GameObject);

        // Adding a name and position to each installedObject_GameObject
        installedObject_GameObject.name = installedObject.ObjectType + "_" + installedObject.Tile.X + "_" + installedObject.Tile.Y;
        installedObject_GameObject.transform.position = new Vector2(installedObject.Tile.X, installedObject.Tile.Y);
        // Setting the new tile as a child, maintaining a clean hierarchy
        installedObject_GameObject.transform.SetParent(this.transform, true);
        
        SpriteRenderer spriteRenderer = installedObject_GameObject.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        spriteRenderer.sprite = GetSpriteForInstalledObject(installedObject);
        spriteRenderer.sortingLayerName = "TileUI";

        // Register action, which will run the funtion when 'tile' gets changed
        installedObject.RegisterOnChangedCallback(OnInstalledObjectChanged);
    }

    /// <summary>
    /// Return the correct sprite for a given installedObject
    /// </summary>
    /// <param name="installedObject">The installedObject that needs a sprite.</param>
    /// <returns>Sprite</returns>
    private Sprite GetSpriteForInstalledObject(InstalledObject installedObject)
    {
        // Return sprite with the same name as installedObject.ObjectType
        if (installedObject.IsLinkedToNeighbour == false)
        {
            return installedObjectSpritesMap[installedObject.ObjectType];
        }

        string spriteName = installedObject.ObjectType + "_";

        /* Check for neighbours: North, East, South & West (in that order)
         * Check if: there are neighbouring tiles, 
         * if those tiles have installedObject on them 
         * if those objects are of the same type. */
        Tile tile;
        int x = installedObject.Tile.X;
        int y = installedObject.Tile.Y;

        // Check North
        tile = World.GetTileAt(x, (y + 1));
        if (TileCheck(tile, installedObject))
            spriteName += "N";

        // Check East
        tile = World.GetTileAt((x + 1), y);
        if (TileCheck(tile, installedObject))
            spriteName += "E";

        // Check South
        tile = World.GetTileAt(x, (y - 1));
        if (TileCheck(tile, installedObject))
            spriteName += "S";

        // Check West
        tile = World.GetTileAt((x - 1), y);
        if (TileCheck(tile, installedObject))
            spriteName += "W";

        // If there isn't a sprite with this current spritename, throw error and return null
        if (installedObjectSpritesMap.ContainsKey(spriteName) == false)
        {
            Debug.LogError("installedObjectSpritesMap doesn't contain a sprite with the name: " + spriteName);
            return null;
        }

        return installedObjectSpritesMap[spriteName];
    }

    /// <summary>
    /// Sub function, to make code little cleaner.
    /// Check if: there are neighbouring tiles, if those tiles have installedObject on them & if those objects are of the same type.
    /// </summary>
    /// <param name="tile">Tile to check.</param>
    /// <param name="installedObject">InstalledObject to compare with.</param>
    /// <returns>True or false</returns>
    public bool TileCheck(Tile tile, InstalledObject installedObject)
    {
        return (tile != null && tile.InstalledObject != null && tile.InstalledObject.ObjectType == installedObject.ObjectType);
    }

    /// <summary>
    /// Function that needs to run after changing an InstalledObject.
    /// </summary>
    /// <param name="installedObject">InstalledObject</param>
    private void OnInstalledObjectChanged(InstalledObject installedObject)
    {
        if (installedObjectGameObjectMap.ContainsKey(installedObject) == false)
        {
            Debug.LogError("OnInstalledObjectChanged -- Trying to change visuals for InstalledObject not in dictionary!");
            return;
        }

        GameObject installedObject_GameObject = installedObjectGameObjectMap[installedObject];
        installedObject_GameObject.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(installedObject);
    }
}
