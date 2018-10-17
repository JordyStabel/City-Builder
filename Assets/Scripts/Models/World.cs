//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using System;

public class World {

    // A 2D array that holds the tile data.
    Tile[,] tiles;

    // Bind ObjectType to a InstalledObject
    Dictionary<string, InstalledObject> installedBaseObjects;

    // Number of tiles as width in the world
    public int Width { get; protected set; }

    // Number of tiles as height in the world
    public int Height { get; protected set; }

    // Callback action for creating installedObject and changing tile
    Action<InstalledObject> cb_InstalledObjectCreated;
    Action<Tile> cb_TileChanged;

    // Holds all queued jobs
    public Queue<Job> jobQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// Default: width = 100, height = 100
    /// </summary>
    /// <param name="width">Width in number of tiles</param>
    /// <param name="height">Height in number of tiles</param>
    public World (int width = 100, int height = 100)
    {
        jobQueue = new Queue<Job>();

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
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
            }
        }
        Debug.Log("World created with: " + width * height + " tiles (width: " + width + ", height: " + height );

        // Create new dictionary of baseInstalledObjects
        installedBaseObjects = new Dictionary<string, InstalledObject>();
        CreateBaseInstalledObjects();
    }

    /// <summary>
    /// Create baseInstalledObject and add it to the dictionary
    /// </summary>
    void CreateBaseInstalledObjects()
    {
        // Create and add baseInstalledObject to the dictionary
        installedBaseObjects.Add("Wall", InstalledObject.CreateBaseObject(
            "Wall",     // InstalledObject ID (type)
            0,          // Movementcost: 0 = imappable, default = 1
            1,          // Width, default = 1
            1,          // Height, default = 1
            true        // Links to neighbours and 'forms' one large object, default = false
            ));
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
                if (UnityEngine.Random.Range(0, 2) == 0)
                    tiles[x, y].Type = TileType.Empty;
                else
                    tiles[x, y].Type = TileType.Floor;
            }
        }
    }

    /// <summary>
    /// Search for correct InstalledObject in dictionary and place it on a tile
    /// </summary>
    /// <param name="objectType">InstalledObject key.</param>
    /// <param name="tile">Tile to place InstalledObject on.</param>
    public void PlaceInstalledObject(string objectType, Tile tile)
    {
        // TODO: Implement multiple tiles support
        // TODO: Implement rotation

        // Check if the dictionary contains an object with the given key (objectType)
        if (installedBaseObjects.ContainsKey(objectType) == false)
        {
            Debug.LogError("installedBaseObjects doesn't contain a baseObject for key: " + objectType);
            return;
        }

        // Actually place the object on a tile
        InstalledObject installedObject = InstalledObject.PlaceObject(installedBaseObjects[objectType], tile);

        if (installedObject == null)
        {
            // Failed to place installedObject -- most likely there was already something there.
            return;
        }

        // If the callback action has registered function, call/execute them
        if (cb_InstalledObjectCreated != null)
            cb_InstalledObjectCreated(installedObject);
    }

    /// <summary>
    /// Gets called when tile changes/updates, triggers callback actions.
    /// </summary>
    /// <param name="tile">Tile to change/update.</param>
    void OnTileChanged(Tile tile)
    {
        // Return if there are no registered functions
        if (cb_TileChanged == null)
            return;

        cb_TileChanged(tile);
    }

    /// <summary>
    /// Validate if its allowed to place a installedObject on a certain tile
    /// </summary>
    /// <param name="installedObjectType">The objectType to validate.</param>
    /// <param name="tile">The tile to validate.</param>
    /// <returns>isValid</returns>
    public bool IsInstalledObjectPlacementValid(string installedObjectType, Tile tile)
    {
        return installedBaseObjects[installedObjectType].IsValidPosition(tile);
    }

    #region (Un)Register callback(s)
    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void RegisterInstalledObjectCreated(Action<InstalledObject> callbackFunction)
    {
        cb_InstalledObjectCreated += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterInstalledObjectCreated(Action<InstalledObject> callbackFunction)
    {
        cb_InstalledObjectCreated -= callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void RegisterTileChanged(Action<Tile> callbackFunction)
    {
        cb_TileChanged += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterTileChanged(Action<Tile> callbackFunction)
    {
        cb_TileChanged -= callbackFunction;
    }
    #endregion
}
