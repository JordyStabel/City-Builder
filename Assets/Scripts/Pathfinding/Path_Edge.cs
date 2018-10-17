//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

// Generic so this can be used for more than one usecase
public class Path_Edge<T> {

    // Cost to traverse this edge (cost to ENTER this tile)
    public float travelCost;

    public Path_Node<T> path_Node;
}
