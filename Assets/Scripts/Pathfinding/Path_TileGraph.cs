//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class creates a simple path-finding compatible graph of the World.
/// Each tile is a Node. Each WALKABLE neighbour from a tile is linked via an EDGE connection.
/// </summary>
public class Path_TileGraph {

    public Dictionary<Tile, Path_Node<Tile>> tileToNodeMap;

	public Path_TileGraph(World world)
    {
        // Loop through all tiles in the World
        // For each tile, create a node
        // Do we create nodes for non-floor tiles? Maybe...not sure yet
        // Do we create nodes for tiles that are completely unwalkalbe? (walls/buildings) Probably NO

        tileToNodeMap = new Dictionary<Tile, Path_Node<Tile>>();

        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile tile = world.GetTileAt(x, y);

                // Tiles with movementCost of 0 are impassable, 1 is normal & 1> is slower than normal movement
                //if (tile.MovementCost > 0)
                //{

                //}

                Path_Node<Tile> path_Node = new Path_Node<Tile>();
                path_Node.data = tile;
                tileToNodeMap.Add(tile, path_Node);
            }
        }

        // Now loop through all tiles again
        // Create edges for neighbours
        foreach (Tile tile in tileToNodeMap.Keys)
        {
            // Get a list of neighbours for the tile
            // If neighbour is walkable, create an edge to the relevant node.
            Path_Node<Tile> path_Node = tileToNodeMap[tile];

            List<Path_Edge<Tile>> path_Edges = new List<Path_Edge<Tile>>();

            // Some spots in array might by null (no neighbours, e.g. corner tiles in the World)
            Tile[] neighbours = tile.GetNeighbours(true);

            for (int i = 0; i < neighbours.Length; i++)
            {
                // There is a neighbour and the movement cost is more than 0 so it's walkable
                if (neighbours[i] != null && neighbours[i].MovementCost > 0)
                {
                    // Check for invalid diagonal clipping/movement, like squeezing through two diagonal walls. 
                    if (IsClippingCorner(tile, neighbours[i]))
                    {
                        // Skip to next neighbour, without creating an edge
                        continue;
                    }

                    // Create new path_Edge of type Tile
                    Path_Edge<Tile> path_Edge = new Path_Edge<Tile>();

                    // Set travelcost equal to neighbouring tile's movementcost
                    path_Edge.travelCost = neighbours[i].MovementCost;

                    // Pair a path_Node, grab that from the dictionary
                    path_Edge.path_Node = tileToNodeMap[neighbours[i]];

                    // Add the created edge to the list
                    path_Edges.Add(path_Edge);
                }
            }

            // Set path_Node's array equal to the path_Edges list, casted to an array
            path_Node.edges = path_Edges.ToArray();
        }
    }

    /// <summary>
    /// If the movement from currentTile to neighbourTile is diagonal, like: N-E
    /// Then check for invalid clipping. Like: If N and E are walls, movement directly to N-E is not allowed
    /// </summary>
    /// <param name="currentTile">Starting tile</param>
    /// <param name="neighbourTile">Tile to check</param>
    /// <returns>Is clipping corners?</returns>
    private bool IsClippingCorner(Tile currentTile, Tile neighbourTile)
    {
        // If true, the tiles are diagonal
        if (Mathf.Abs(currentTile.X - neighbourTile.X) + Mathf.Abs(currentTile.Y - neighbourTile.Y) == 2)
        {
            int delta_X = currentTile.X - neighbourTile.X;
            int delta_Y = currentTile.Y - neighbourTile.Y;

            // Either the East or West tile is impassable, thus this would be a 'clipped' movement. (invalid movement)
            if (currentTile.World.GetTileAt(currentTile.X - delta_X, currentTile.Y).MovementCost == 0)
                return true;

            // Either the North or South tile is impassable, thus this would be a 'clipped' movement. (invalid movement)
            if (currentTile.World.GetTileAt(currentTile.X, currentTile.Y - delta_Y).MovementCost == 0)
                return true;
        }
        // Default: not diagonal and movement is thus allowed
        return false;
    }
}
