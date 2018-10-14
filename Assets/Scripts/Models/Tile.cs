using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile {

	public enum TileType { Empty, Floor };

    TileType tileType = TileType.Empty;

    LooseObject loose;
    InstalledObject installed;

    World world;
    int x;
    int y;

    public Tile(World world, int x, int y)
    {
        this.world = world;
        this.x = x;
        this.y = y;
    }
}
