//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class MouseController : MonoBehaviour {

    [Header("Cursor sprite")]
    public GameObject circleCursor;

    Vector2 lastFrameMousePosition;
    Vector2 startDragTilePosition;

	void Start () {

    }
	
	void Update () {

        Vector2 currentFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        #region Update circle cursor position
        Tile tileHoverOver = GetTileAtWorldCoordinate(currentFramePosition);                // Get the tile the mouse is hovering over
        if (tileHoverOver != null && tileHoverOver.Type != Tile.TileType.Empty)             // Check if the mouse is hovering over a tile (not empty tile too)
        {
            circleCursor.SetActive(true);                                                   // Enable and set position cursor if tile isn't null 
            Vector2 cursorPosition = new Vector2(tileHoverOver.X, tileHoverOver.Y);
            circleCursor.transform.position = cursorPosition;
        }
        else
            circleCursor.SetActive(false);                                                  // Disable the cursor if tile is null
        #endregion

        #region Handle left mouse clicks
        // Start drag movement
        if (Input.GetMouseButtonDown(0))                                                    // Check if left mouse button was pressed
            startDragTilePosition = currentFramePosition;                                                  // Save starting tile

        // End drag movement
        if (Input.GetMouseButtonUp(0))                                                      // Check if left mouse button was released
        {
            int start_X = Mathf.FloorToInt(startDragTilePosition.x);
            int end_X = Mathf.FloorToInt(currentFramePosition.x);
            if (end_X < start_X)                                                            // Swap intergers if players drags from top to bottom or right to left
            {
                int temp = end_X;
                end_X = start_X;
                start_X = temp;
            }

            int start_Y = Mathf.FloorToInt(startDragTilePosition.y);
            int end_Y = Mathf.FloorToInt(currentFramePosition.y);
            if (end_Y < start_Y)                                                            // Swap intergers if players drags from top to bottom or right to left
            {
                int temp = end_Y;
                end_Y = start_Y;
                start_Y = temp;
            }

            for (int x = start_X; x <= end_X; x++)
            {
                for (int y = start_Y; y <= end_Y; y++)
                {
                    Tile tile = WorldController.Instance.World.GetTileAt(x, y);
                    if (tile != null)
                        tile.Type = Tile.TileType.Floor;
                }
            }
            // if (tileHoverOver != null)                                                      // If tile isn't null, flip the tile type
            //{
            //    if (tileHoverOver.Type == Tile.TileType.Empty)
            //        tileHoverOver.Type = Tile.TileType.Floor;
            //    else if (tileHoverOver.Type == Tile.TileType.Floor)
            //        tileHoverOver.Type = Tile.TileType.Empty;
            //}
        }
        #endregion

        #region Screen movement with mouse
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))                             // Check if right- or middle mousebutton is being pressed
        {
            Vector2 difference = lastFrameMousePosition - currentFramePosition;
            Camera.main.transform.Translate(difference);                                    // add '-' to difference to invert the controls
        }
        lastFrameMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        #endregion
    }

    /// <summary>
    /// Check which tile the mouse hovers over and then return that tile
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>Return a tile at the current mouse coordinates</returns>
    Tile GetTileAtWorldCoordinate(Vector2 coordinates)
    {
        // Round float to int
        int x = Mathf.FloorToInt(coordinates.x);
        int y = Mathf.FloorToInt(coordinates.y);

        return WorldController.Instance.World.GetTileAt(x, y);
    }
}
