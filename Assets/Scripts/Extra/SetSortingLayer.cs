//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class SetSortingLayer : MonoBehaviour {

    [SerializeField]
    private string sortingLayerName = "Default";

	void Start () {
        GetComponent<Renderer>().sortingLayerName = sortingLayerName;
	}
}
