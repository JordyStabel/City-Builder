//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class WorldController : MonoBehaviour {

    // Creating an instance of 'WorldController' which is accessible from all classes
    public static WorldController Instance { get; protected set; }

    // Bind data to a GameObject
    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<InstalledObject, GameObject> installedObjectGameObjectMap;

    [Header("Floor tile sprite")]
    public Sprite floorSprite;

    [Header("Wall sprite")]
    public Sprite wallSprite;

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

                // Add SpriteRenderer to each tile_gameObject
                tile_GameObject.AddComponent<SpriteRenderer>();

                // Register action, which will run the funtion when 'tile' gets changed
                tile_Data.RegisterTileTypeChangedCallback(OnTileTypeChanged);
            }
        }

        // Randomize all tiles in the world this was just created (thus calling the 'OnTileTypeChanged' function for each tile)
        World.RandomizeTiles();
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
        spriteRenderer.sprite = wallSprite;
        spriteRenderer.sortingLayerName = "TileUI";

        // Register action, which will run the funtion when 'tile' gets changed
        installedObject.RegisterOnChangedCallback(OnInstalledObjectCreated);
    }

    /// <summary>
    /// Function that needs to run after changing an InstalledObject.
    /// </summary>
    /// <param name="installedObject">InstalledObject</param>
    void OnInstalledObjectChanged(InstalledObject installedObject)
    {
        Debug.LogError("OnInstalledObjectChanged -- Not implemented!");
    }
}
