//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System;

public class Tile {

    // TileType
    public enum TileType { Empty, Floor };

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
            if (cbTileTypeChanged != null && previousType != type)
                cbTileTypeChanged(this);
        }
    }

    // LooseObject: static buildings, pile of resources, equipment, etc.
    LooseObject loose;

    // InstalledObject: wall, door, etc.
    InstalledObject installed;

    // Data a Tile needs have access to
    World world;
    public int X { get; protected set; }
    public int Y { get; protected set; }

    // Callback action for changing tile type
    Action<Tile> cbTileTypeChanged;


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
    /// <param name="callback">The function that is going to get registered.</param>
    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged += callback;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callback">The function that is going to get unregistered.</param>
    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged -= callback;
    }
}
