//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldController : MonoBehaviour {

    // Creating an instance of 'WorldController' which is accessible from all classes
    public static WorldController Instance { get; protected set; }

    // The world, holds all tile data
    public World World { get; protected set; }

    /// <summary>
    /// Create new world
    /// OnEnable instead of start, so it runs first (before any start/update function)
    /// </summary>
    void OnEnable () {

        // Setting the instance equal to this current one (with check)
        if (Instance != null)
            Debug.LogError("There shouldn't be two world controllers.");
        else
            Instance = this;

        // Create new world with empty tiles
        World = new World();

        // Center camera in the world
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);

        // Randomize all tiles in the world this was just created (thus calling the 'OnTileChanged' function for each tile)
        //World.RandomizeTiles();
	}

    void Update()
    {
        World.UpdateWorld(Time.deltaTime);
    }

    /// <summary>
    /// Check which tile the mouse hovers over and then return that tile
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>Return a tile at the current mouse coordinates</returns>
    public Tile GetTileAtWorldCoordinate(Vector2 coordinates)
    {
        // Round float to int, since the tiles are all 1 by 1 unit
        int x = Mathf.FloorToInt(coordinates.x);
        int y = Mathf.FloorToInt(coordinates.y);

        return World.GetTileAt(x, y);
    }
}
