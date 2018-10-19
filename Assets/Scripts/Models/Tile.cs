//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Tile : IXmlSerializable {
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
    public float MovementCost {
        get {

            if (Type == TileType.Empty)
                return 0;

            if (InstalledObject == null)
                return 1;

            return InstalledObject.MovementCost;
        } }

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

    /// <summary>
    /// Check if two tiles are adjacent to each other.
    /// Check to if the differenc between two tiles is 1.
    /// If so, they're either vertical or horizontal neighbour.
    /// Diagonal, same thing but moved 1 to left and 1 to right.
    /// Then check again. Only if diagonalAllowed == true
    /// </summary>
    /// <param name="tile">Tile to check.</param>
    /// <returns>true if it's a neighbour, false if not.</returns>
    public bool IsAdjacent(Tile tile, bool diagonalAllowed = false)
    {
        return Mathf.Abs(X - tile.X) + Mathf.Abs(Y - tile.Y) == 1 || 
            (diagonalAllowed && (Mathf.Abs(X - tile.X) == 1 && (Mathf.Abs(Y - tile.Y) == 1)));
    }

    /// <summary>
    /// Get all the neighbouring tiles of a given tile.
    /// Also get the diagonal neighbours if needed.
    /// </summary>
    /// <param name="diagonalAllowed">Also need the diagonal neighbours.</param>
    /// <returns>All the neighbouring tiles.</returns>
    public Tile[] GetNeighbours(bool diagonalAllowed = false)
    {
        Tile[] neighbouringTiles;

        if (diagonalAllowed == false)
            neighbouringTiles = new Tile[4];    // Tile order: N E S W
        else
            neighbouringTiles = new Tile[8];    // Tilde order: N E S W NE SE SW NW

        Tile neighbouringTile;

        // Can return nulls, but that's fine. It's fine
        // North
        neighbouringTile = World.GetTileAt(X, Y + 1);
        neighbouringTiles[0] = neighbouringTile;

        // East
        neighbouringTile = World.GetTileAt(X + 1, Y);
        neighbouringTiles[1] = neighbouringTile;

        // South
        neighbouringTile = World.GetTileAt(X, Y - 1);
        neighbouringTiles[2] = neighbouringTile;

        // West
        neighbouringTile = World.GetTileAt(X - 1, Y);
        neighbouringTiles[3] = neighbouringTile;

        if (diagonalAllowed == true)
        {
            // North-East
            neighbouringTile = World.GetTileAt(X + 1, Y + 1);
            neighbouringTiles[4] = neighbouringTile;

            // South-East
            neighbouringTile = World.GetTileAt(X + 1, Y - 1);
            neighbouringTiles[5] = neighbouringTile;

            // South-West
            neighbouringTile = World.GetTileAt(X - 1, Y - 1);
            neighbouringTiles[6] = neighbouringTile;

            // Norh-West
            neighbouringTile = World.GetTileAt(X - 1, Y + 1);
            neighbouringTiles[7] = neighbouringTile;
        }

        return neighbouringTiles; ;
    }

    #region Saving & Loading
    public XmlSchema GetSchema()
    {
        // Just here so IXmlSerializable doesn't throw an error :)
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // Save data here
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("TileType", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader)
    {
        // Load data here
        //X = int.Parse(reader.GetAttribute("X"));
        //Y = int.Parse(reader.GetAttribute("Y"));
        
        // Cast the int (parsed, from 'TileType') to the correct TileType enum value
        // Parsing might throw error
        Type = (TileType)int.Parse(reader.GetAttribute("TileType"));
    }
    #endregion

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
