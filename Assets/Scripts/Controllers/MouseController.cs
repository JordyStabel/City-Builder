//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {

    [Header("Cursor sprite")]
    public GameObject circleCursor;

    [Header("Prefab of 'cursor' sprite")]
    public GameObject circleCursorPrefab;

    [Header("Player settings")]
    [Tooltip("Default: 1, higher = faster")]
    public float zoomSpeed = 1f;

    bool buildModeIsObjects = false;
    TileTypes buildModeTileType = TileTypes.Floor;

    // The world position of the mouse
    Vector2 currentFrameMousePosition;
    Vector2 lastFrameMousePosition;

    // The world position at start of left mouse drag movement
    Vector2 startDragTilePosition;

    // Create list that holds the 'placement preview objects'
    List<GameObject> dragPreviewGameObjects;

    // Instance of ObjectPooler
    ObjectPooler objectPooler;

    void Start()
    {
        dragPreviewGameObjects = new List<GameObject>();
        objectPooler = ObjectPooler.Instance;
    }

    void Update () {
        currentFrameMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        UpdateCursor();
        UpdateDragMovement();
        UpdateCameraMovement();
    
        // Save the current mouse position again,
        // because the camera might have moved since the start of this update cycle
        lastFrameMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// Check if the mouse if hovering over a tile.
    /// Updating the cursor accordingly.
    /// </summary>
    private void UpdateCursor()
    {
        // Get the tile the mouse is hovering over
        Tile tileHoverOver = WorldController.Instance.GetTileAtWorldCoordinate(currentFrameMousePosition);

        // Check if the mouse is hovering over a tile (not empty tile too)
        if (tileHoverOver != null && tileHoverOver.Type != TileTypes.Empty)
        {
            // Enable and set position cursor if tile isn't null
            circleCursor.SetActive(true); 
            Vector2 cursorPosition = new Vector2(tileHoverOver.X, tileHoverOver.Y);
            circleCursor.transform.position = cursorPosition;
        }
        // Disable if tile is null
        else
            circleCursor.SetActive(false);
    }

    /// <summary>
    /// Create a square of all tiles in a selected area.
    /// Do something with the tiles in set selected area.
    /// </summary>
    private void UpdateDragMovement()
    {
        // Prevent stuff from happening when mouse is over UI elements
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        #region Start drag movement
        // Check if left mouse button was pressed.
        // Prevent stuff from happening when mouse is over UI elements.
        if (Input.GetMouseButtonDown(0))
            // Save starting tile
            startDragTilePosition = currentFrameMousePosition;

        int start_X = Mathf.FloorToInt(startDragTilePosition.x);
        int end_X = Mathf.FloorToInt(currentFrameMousePosition.x);
        int start_Y = Mathf.FloorToInt(startDragTilePosition.y);
        int end_Y = Mathf.FloorToInt(currentFrameMousePosition.y);

        // Swap intergers if players drags from top to bottom or right to left
        if (end_X < start_X)
        {
            int temp = end_X;
            end_X = start_X;
            start_X = temp;
        }
        if (end_Y < start_Y)
        {
            int temp = end_Y;
            end_Y = start_Y;
            start_Y = temp;
        }

        // While there are pooled object in the list, remove them
        while (dragPreviewGameObjects.Count > 0)
        {
            GameObject gameObject = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            objectPooler.DespawnFromPool(gameObject);
        }

        // Display a preview of all selected tiles
        if (Input.GetMouseButton(0))
        {
            // Loop through all selected tiles and change them
            for (int x = start_X; x <= end_X; x++)
            {
                for (int y = start_Y; y <= end_Y; y++)
                {
                    Tile tile = WorldController.Instance.World.GetTileAt(x, y);

                    if (tile != null)
                    {
                        GameObject gameObject = objectPooler.SpawnFromPool("preview_Cursor", new Vector2(x, y), Quaternion.identity);
                        dragPreviewGameObjects.Add(gameObject);
                    }
                }
            }
        }
        #endregion

        #region End drag movement
        // Check if left mouse button was released
        if (Input.GetMouseButtonUp(0))
        {
            // Loop through all selected tiles and change them
            for (int x = start_X; x <= end_X; x++)
            {
                for (int y = start_Y; y <= end_Y; y++)
                {
                    Tile tile = WorldController.Instance.World.GetTileAt(x, y);
                    if (tile != null)
                    {
                        if (buildModeIsObjects)
                        {
                            // Building Objects

                            // FIXME: Right now we can only build walls, nothing else
                        }
                        else
                        {
                            tile.Type = buildModeTileType;
                        }
                    }
                }
            }

            #region Old bit of code
            // if (tileHoverOver != null) // If tile isn't null, flip the tile type
            //{
            //    if (tileHoverOver.Type == Tile.TileType.Empty)
            //        tileHoverOver.Type = Tile.TileType.Floor;
            //    else if (tileHoverOver.Type == Tile.TileType.Floor)
            //        tileHoverOver.Type = Tile.TileType.Empty;
            //}
            #endregion
        }
        #endregion
    }

    /// <summary>
    /// Update the camera position depending on the mouse movement of the player.
    /// Player can use either right- or middle mouse button to drag the camera around.
    /// </summary>
    private void UpdateCameraMovement()
    {
        // Check if right- or middle mousebutton is being pressed
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Vector2 difference = lastFrameMousePosition - currentFrameMousePosition;

            // add '-' to difference to invert the controls
            Camera.main.transform.Translate(difference);
        }

        // Zoom effect, controlled by mouse scrolling
        // The second 'Camera.main.orthographicSize' makes it feel consitant even when zooming out alot
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        // Limit the max zoom in and out
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 2.5f, 30f);
    }

    public void SetMode_BuildFloor()
    {
        buildModeIsObjects = false;
        buildModeTileType = TileTypes.Floor;
    }

    public void SetMode_Destroy()
    {
        buildModeIsObjects = false;
        buildModeTileType = TileTypes.Empty;
    }

    public void SetMode_BuildWall()
    {
        // A wall isn't a Tile type. Wall is an "InstalledObject" that exists on top of a Tile.
        buildModeIsObjects = true;
    }
}
