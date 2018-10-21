//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;

public class Path_AStar{

    // This is the final path a character/machine will take.
    // Calculated with A*, 99% of the time it's the shortest path possible.
    Queue<Tile> path;

    public LineRenderer lineRenderer;

	public Path_AStar(World world, Tile startTile, Tile endTile)
    {
        // Check if there is a valid tile_Graph, otherwise create one
        if (world.path_TileGraph == null)
            world.path_TileGraph = new Path_TileGraph(world);

        // A dictionary of all the valid/walkable nodes
        Dictionary<Tile, Path_Node<Tile>> tileToNodeMap = world.path_TileGraph.tileToNodeMap;

        // Check if startTile and endTile are valid tiles in the list of nodes
        if (tileToNodeMap.ContainsKey(startTile) == false)
        {
            Debug.LogError("Path_AStar: Starting tile isn't in the list of nodes!");
            return;
        }

        if (tileToNodeMap.ContainsKey(endTile) == false)
        {
            Debug.LogError("Path_AStar: Ending tile isn't in the list of nodes!");
            return;
        }

        // Setting start- & goal nodes
        Path_Node<Tile> start = tileToNodeMap[startTile];
        Path_Node<Tile> goal = tileToNodeMap[endTile];

        // Create open- & closed sets
        List<Path_Node<Tile>> closedSet = new List<Path_Node<Tile>>();
        //List<Path_Node<Tile>> openSet = new List<Path_Node<Tile>>();

        // Add starting tile to the openSet
        //openSet.Add(start);

        // Creating a priorityqueue will speed things up quite a bit.
        // Creates a queue where all entries have a priority, in this case the f_Score.
        // This way it's faster to find entries based on their value/priority (f_Score)
        SimplePriorityQueue<Path_Node<Tile>> openSet = new SimplePriorityQueue<Path_Node<Tile>>();
        openSet.Enqueue(start, 0);

        // Dictionary of already navigated nodes (tiles)
        Dictionary<Path_Node<Tile>, Path_Node<Tile>> came_From = new Dictionary<Path_Node<Tile>, Path_Node<Tile>>();

        // Dictionary of the cost assigned to each node (tile)
        Dictionary<Path_Node<Tile>, float> g_Score = new Dictionary<Path_Node<Tile>, float> ();

        // Set the actual cost to get to each node (tile) to infinity (highest posible float value)
        // Set it to Mathf.Infinity, because the system doesn't yet know the actual cost, so lets assume it's insanely high
        // This way all nodes have a cost by default and it's never the faster route to take, because the cost is 'infinite'
        foreach (Path_Node<Tile> node in tileToNodeMap.Values)
        {
            g_Score[node] = Mathf.Infinity;
        }

        // Already on this tile, so no cost to get there
        g_Score[start] = 0;

        // Estimated total cost from start to end
        Dictionary<Path_Node<Tile>, float> f_Score = new Dictionary<Path_Node<Tile>, float>();

        // Set the estimated cost to get to each node (tile) to infinity (highest posible float value)
        // Set it to Mathf.Infinity, because the system doesn't yet know the actual cost, so lets assume it's insanely high
        // This way all nodes have a cost by default and it's never the faster route to take, because the cost is 'infinite'
        foreach (Path_Node<Tile> node in tileToNodeMap.Values)
        {
            f_Score[node] = Mathf.Infinity;
        }

        // Set f_Score for starting node.
        f_Score[start] = Heuristic_Cost_Estimate(start, goal);

        // While there are nodes in the openSet
        while (openSet.Count > 0)
        {
            Path_Node<Tile> current = openSet.Dequeue();

            // Check if the current node is the goal node, then the path is done
            if (current == goal)
            {
                // Goal reached, the shortest path (99% of the time) was found.
                // Convert path of node back to path of tiles.
                ReconstructPath(came_From, current);
                return;
            }

            // Add tile to closedSet
            closedSet.Add(current);

            // Loop through all neighbouring tiles
            foreach (Path_Edge<Tile> edge_Neighbour in current.edges)
            {
                // Convert edge_Neighbour to node and store it for easier use.
                Path_Node<Tile> neighbour = edge_Neighbour.path_Node;

                if (closedSet.Contains(neighbour))
                {
                    // Go to next step in loop
                    continue;
                }
                
                // The g_Score for now (voorlopige g_Score)
                float tentative_gScore = g_Score[current] + (DistanceBetween(current, neighbour) * neighbour.data.MovementCost);

                // If the openSet contains this neighbour AND the tentative_gScore >= than the g_Score of that neighbour, move to next one
                if (openSet.Contains(neighbour) && tentative_gScore >= g_Score[neighbour])
                    continue;

                //
                came_From[neighbour] = current;

                // Set final g_Score & f_Score
                g_Score[neighbour] = tentative_gScore;
                f_Score[neighbour] = g_Score[neighbour] + Heuristic_Cost_Estimate(neighbour, goal);

                if (openSet.Contains(neighbour) == false)
                    openSet.Enqueue(neighbour, f_Score[neighbour]);
            }
        }

        /*  If this point is reached. The entire openSet was looped through without finding current == goal.
            This happends when there is NO path available for the given A to B points.
            For example there are walls/buidlings/machines in the way. */
    }

    /// <summary>
    /// Calculate the "euclidean distance" (shortest lenght from A to B)
    /// </summary>
    /// <param name="start">Beginning point</param>
    /// <param name="goal">Ending point</param>
    /// <returns>Euclidean distance between start & goal</returns>
    float Heuristic_Cost_Estimate(Path_Node<Tile> start, Path_Node<Tile> goal)
    {
        // Pythagorean theorem
        return Mathf.Sqrt(
            Mathf.Pow(start.data.X - goal.data.X, 2) + 
            Mathf.Pow(start.data.Y - goal.data.Y, 2));
    }

    /// <summary>
    /// Return the distance between two points.
    /// </summary>
    /// <param name="a">Starting point</param>
    /// <param name="b">Ending point</param>
    /// <returns>Distance betwee A & B</returns>
    float DistanceBetween(Path_Node<Tile> a, Path_Node<Tile> b)
    {
        // Distance between a and b is either 1 (straight up, down, left or right) or 1.414213562373 (diagonal = square root 2), because it's on a grid.

        // 'Straight' neighbour
        if (Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1)
            return 1;

        // Diagonal neighbour
        if (Mathf.Abs(a.data.X - b.data.X) == 1 && (Mathf.Abs(a.data.Y - b.data.Y) == 1))
            return 1.41421356237f;

        // Otherwise, actually calculate the distance (will take slightly longer/more pc powerrrrr
        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2));
    }

    /// <summary>
    /// Reconstruct the path calculated, this time with actual tiles.
    /// </summary>
    /// <param name="came_From">Map with all nodes the path has gone through</param>
    /// <param name="current">Current node (goal tile)</param>
    void ReconstructPath(Dictionary<Path_Node<Tile>, Path_Node<Tile>> came_From, Path_Node<Tile> current)
    {
        // At this point: current == goal
        // Loop through the came_From map, till the end.
        // That last node will be the starting node.
        Queue<Tile> total_Path = new Queue<Tile>();

        // This is the 'final' step in the path, so the goal!
        total_Path.Enqueue(current.data);

        // came_From is a map where the key => value relation:
        // some_node => we_got_there_from_this_node
        // Loop through came_From till it ends, because the starting tile was never added in the came_From map
        while (came_From.ContainsKey(current))
        {
            current = came_From[current];
            total_Path.Enqueue(current.data);
        }

        // At this point, total_Path is a queue that runs backwards from the END tile to the START tile.
        // Need to reverse it again, to make it work.
        // Set the path equal to the, with A*, created path.
        path = new Queue<Tile>(total_Path.Reverse());
    }

    /// <summary>
    /// Return and dequeue, the next tile in the path
    /// </summary>
    /// <returns>Next tile</returns>
    public Tile DequeueNextTile()
    {
        return path.Dequeue();
    }

    /// <summary>
    /// Get the lenght of the path
    /// </summary>
    /// <returns>Lenght path (int)</returns>
    public int Length()
    {
        if (path == null)
            return 0;

        return path.Count();
    }
}
