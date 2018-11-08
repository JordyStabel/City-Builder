//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine.UI;
using UnityEngine;

public class MouseOverTileTypeText : MonoBehaviour {

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
        if (tile != null)
            textObject.text = "Tile Type: " + tile.Type.ToString();
        else
            textObject.text = "Tile Type: NULL";
    }
}
