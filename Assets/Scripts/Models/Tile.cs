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
    public InstalledObject InstalledObject { get; protected set; }

    // Assign job to tile, prevent double jobs on a tile
    public Job pendingInstalledObjectJob;

    // Properties a Tile (and other classes/objects) need to have access to
    public World World { get; protected set; }
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
        World = world;
        X = x;
        Y = y;
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
            InstalledObject = null;
            return true;
        }

        // Check if there is room to place an installedObject
        if (InstalledObject != null)
        {
            Debug.LogError("Trying to place installedObject to a tile that already has one!");
            return false;
        }

        InstalledObject = installedObject;
        return true;
    }

    #region (Un)Register callback(s)
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
    #endregion
}
