﻿//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using System;

public class World {

    // A 2D array that holds the tile data.
    Tile[,] tiles;

    // List of all characters in the world
    List<Character> characters;

    // The pathfinding graph used to navigate the world
    Path_TileGraph path_TileGraph;

    // Bind ObjectType to a InstalledObject
    Dictionary<string, InstalledObject> installedBaseObjects;

    // Number of tiles as width in the world
    public int Width { get; protected set; }

    // Number of tiles as height in the world
    public int Height { get; protected set; }

    // Callback action for creating installedObject and changing tile
    Action<InstalledObject> cb_InstalledObjectCreated;
    Action<Tile> cb_TileChanged;
    Action<Character> cb_CharacterCreated;

    public JobQueue jobQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// Default: width = 100, height = 100
    /// </summary>
    /// <param name="width">Width in number of tiles</param>
    /// <param name="height">Height in number of tiles</param>
    public World (int width = 100, int height = 100)
    {
        jobQueue = new JobQueue();
        characters = new List<Character>();

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
        Debug.Log("World created with: " + width * height + " tiles -- width: " + width + ", height: " + height );

        // Create new dictionary of baseInstalledObjects
        installedBaseObjects = new Dictionary<string, InstalledObject>();
        CreateBaseInstalledObjects();
    }

    /// <summary>
    /// Update function for the world. Needs a delta time from somewhere else.
    /// </summary>
    /// <param name="deltaTime">The amount of time passed since last update. Higher = faster.</param>
    public void UpdateWorld(float deltaTime)
    {
        // Update each character in the list
        foreach (Character character in characters)
            character.UpdateCharacter(deltaTime);
    }

    /// <summary>
    /// Create a new character
    /// </summary>
    /// <param name="tile">Tile that the character will get spawned on.</param>
    public Character CreateCharacter(Tile tile)
    {
        // Spawn character in center of the world
        Character character = new Character(tile);
        characters.Add(character);

        if (cb_CharacterCreated != null)
            cb_CharacterCreated(character);

        return character;
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
    /// Quick debug function, generate pre-made map with installedObjects and floors
    /// </summary>
    public void SetupPathfindingExample()
    {
        Debug.Log("SetupPathfindingExample");

        // Make a set of floors/walls to test pathfinding with.

        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l - 5; x < l + 15; x++)
        {
            for (int y = b - 5; y < b + 15; y++)
            {
                tiles[x, y].Type = TileType.Floor;


                if (x == l || x == (l + 9) || y == b || y == (b + 9))
                {
                    if (x != (l + 9) && y != (b + 4))
                    {
                        PlaceInstalledObject("Wall", tiles[x, y]);
                    }
                }
            }
        }
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
        {
            cb_InstalledObjectCreated(installedObject);
            InvalidateTileGraph();
        }
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

        InvalidateTileGraph();
    }

    /// <summary>
    /// Needs to get called whenever a change to the world means the old pathfinding info is no longer valid.
    /// </summary>
    public void InvalidateTileGraph()
    {
        path_TileGraph = null;
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

    /// <summary>
    /// Ask for a baseInstalledObject from dictionary
    /// </summary>
    /// <param name="installedObjectType">Key for object</param>
    /// <returns>baseInstalledObject</returns>
    public InstalledObject GetInstalledBaseObject(string installedObjectType)
    {
        // Check if dictionary contains something with the given key
        if (installedBaseObjects.ContainsKey(installedObjectType) == false)
        {
            Debug.LogError("No InstalledObject with type: " + installedObjectType);
            return null;
        }
            
        return installedBaseObjects[installedObjectType];
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
    public void RegisterCharacterCreatedCallback(Action<Character> callbackFunction)
    {
        cb_CharacterCreated += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterCharacterCreatedCallback(Action<Character> callbackFunction)
    {
        cb_CharacterCreated -= callbackFunction;
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
