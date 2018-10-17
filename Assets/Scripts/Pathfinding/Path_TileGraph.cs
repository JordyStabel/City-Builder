//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;

/// <summary>
/// This class creates a simple path-finding compatible graph of the World.
/// Each tile is a Node. Each WALKABLE neighbour from a tile is linked via an EDGE connection.
/// </summary>
public class Path_TileGraph {

    Dictionary<Tile, Path_Node<Tile>> tileToNodeMap;

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
                if (tile.MovementCost > 0)
                {
                    Path_Node<Tile> path_Node = new Path_Node<Tile>();
                    tileToNodeMap.Add(tile, path_Node);
                }
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
}
