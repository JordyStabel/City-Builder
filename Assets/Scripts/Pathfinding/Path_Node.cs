//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

// Generic so this can be used for more than one usecase
public class Path_Node<T> {

    public T data;

    // Nodes leading OUT from this node
    public Path_Edge<T>[] edges;
}
