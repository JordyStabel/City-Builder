using System;

public class Tile {

    // Tiletype
	public enum TileType { Empty, Floor };

    // Tiletype default = 'Empty'
    TileType tileType = TileType.Empty;

    // callback action for changing tile type
    Action<Tile> cbTileTypeChanged;

    // Fields
    LooseObject loose;
    InstalledObject installed;
    World world;
    int x;
    int y;

    // Properties
    public TileType Type {
        get { return tileType; }
        set {
            // Add previousType to prevent calling 'cbTileTypeChanged' if type didn't change
            TileType previousType = tileType;
            tileType = value;
            if (cbTileTypeChanged != null && previousType != tileType)
                cbTileTypeChanged(this);
        }
    }

    public int X
    {
        get { return x; }
    }

    public int Y
    {
        get { return y; }
    }

    // Tile constructor
    public Tile(World world, int x, int y)
    {
        this.world = world;
        this.x = x;
        this.y = y;
    }

    // Register action with given function
    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged += callback;
    }

    // Unregister action with given function
    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileTypeChanged -= callback;
    }
}
