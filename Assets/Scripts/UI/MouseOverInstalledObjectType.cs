//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine.UI;
using UnityEngine;

public class MouseOverInstalledObjectTypeText : MonoBehaviour {

    [SerializeField]
    private Text textObject;

    MouseController mouseController;

    void Start()
    {
        textObject = GetComponent<Text>();

        if (textObject == null)
        {
            Debug.LogError("MouseOverTileTypeText: No 'Text' UI component found!");
            this.enabled = false;
            return;
        }

        mouseController = GameObject.FindObjectOfType<MouseController>();

        if (mouseController == null)
        {
            Debug.LogError("No instance of MouseController found!");
            return;
        }
    }
    
	void Update ()
    {
        Tile tile = mouseController.GetTileUnderMouse();

        string s = "NULL";

        if (tile.InstalledObject != null)
            s = tile.InstalledObject.ObjectType;

        textObject.text = "InstalledObject: " + s;
	}
}
