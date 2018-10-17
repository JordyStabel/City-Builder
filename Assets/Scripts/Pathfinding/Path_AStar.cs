//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;

public class Path_AStar{

    Queue<Tile> path;

	public Path_AStar(World world, Tile startTile, Tile endTile)
    {

    }

    public Tile GetNextTileInPath()
    {
        return path.Dequeue();
    }
}
