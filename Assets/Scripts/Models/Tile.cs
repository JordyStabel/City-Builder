//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System;
using UnityEngine;

public class Tile {
    // Tiletype default = 'Empty'
    // Type getter & setter
    private TileType type = TileType.Empty;
    public TileType Type
    {
        get { return type; }
        set
        {
            // Add previousType to prevent calling 'cbTileTypeChanged' if type didn't change
            TileType previousType = type;
            type = value;
            if (cb_TileTypeChanged != null && previousType != type)
                cb_TileTypeChanged(this);
        }
    }

    // LooseObject: static buildings, pile of resources, equipment, etc.
    LooseObject looseObject;

    // InstalledObject: wall, door, etc.
    InstalledObject installedObject;

    // Data a Tile needs have access to
    World world;
    public int X { get; protected set; }
    public int Y { get; protected set; }

    // Callback action for changing tile type
    Action<Tile> cb_TileTypeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class. 
    /// </summary>
    /// <param name="world">The World instance, this tile is part of.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile(World world, int x, int y)
    {
        this.world = world;
        X = x;
        Y = y;
    }

    /// <summary>
    /// Register action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get registered.</param>
    public void RegisterTileTypeChangedCallback(Action<Tile> callbackFunction)
    {
        cb_TileTypeChanged += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterTileTypeChangedCallback(Action<Tile> callbackFunction)
    {
        cb_TileTypeChanged -= callbackFunction;
    }

    /// <summary>
    /// Place an InstalledObject on a Tile, if possible
    /// </summary>
    /// <param name="installedObject">The installedObject to place</param>
    /// <returns>Success</returns>
    public bool PlaceInstalledObject(InstalledObject installedObject)
    {
        // Uninstall installedObject
        if (installedObject == null)
        {
            this.installedObject = null;
            return true;
        }

        // Check if there is room to place an installedObject
        if (this.installedObject != null)
        {
            Debug.LogError("Trying to place installedObject to a tile that already has one!");
            return false;
        }

        this.installedObject = installedObject;
        return true;
    }
}
