//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileSpriteController : MonoBehaviour {

    // Bind data to a GameObject
    Dictionary<Tile, GameObject> tileGameObjectMap;

    [Header("Floor tile sprite")]
    public Sprite floorSprite;

    [Header("Empty tile sprite")]
    public Sprite emptySprite;

    // Get reference to the World
    World World { get { return WorldController.Instance.World; } }

    void Start () {

        // Instatiate dictionary that binds a GameObject with data 
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Create a GameObject for each tile
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                // Get the tile data
                Tile tile_Data = World.GetTileAt(x, y);

                // Creating new gameObject
                GameObject tile_GameObject = new GameObject();

                // Add tile_Data and tile_GameObject to the dictionary (tile_Data is the key)
                tileGameObjectMap.Add(tile_Data, tile_GameObject);

                // Adding a name and position to each tile_gameObject
                tile_GameObject.name = "Tile_" + x + "_" + y;
                tile_GameObject.transform.position = new Vector2(tile_Data.X, tile_Data.Y);
                // Setting the new tile as a child, maintaining a clean hierarchy
                tile_GameObject.transform.SetParent(this.transform, true);

                // Add SpriteRenderer and default empty sprite to each tile_gameObject
                tile_GameObject.AddComponent<SpriteRenderer>().sprite = emptySprite;
            }
        }

        // Register action, which will run the funtion when tile_data gets changed
        World.RegisterTileChanged(OnTileChanged);
	}

    #region CURRENTLY NOT IN USE - Unbind pairs in dictionary
    /// <summary>
    /// Unpair all tile_Data and tile GameObjects from the dictionary.
    /// Destroy tile GameObject, the visual part in-game.
    /// Can be used when creating new level and the old levels need to be removed.
    /// </summary>
    void DestroyAllTileGameObjects()
    {
        // Run while there are pair in the dictionary
        while (tileGameObjectMap.Count > 0)
        {
            // Grab first pair from dictionary
            Tile tile_Data = tileGameObjectMap.Keys.First();
            GameObject tile_GameObject = tileGameObjectMap[tile_Data];

            // Unpair tile_Data and tile_GameObject from dictionary, thus shrinking the dictionary
            tileGameObjectMap.Remove(tile_Data);

            tile_Data.UnregisterTileTypeChangedCallback(OnTileChanged);
            Destroy(tile_GameObject);
        }

        // TODO: Create new level...or something else
    }
    #endregion

    /// <summary>
    /// Change the tile sprite upon changing its tileType.
    /// </summary>
    /// <param name="tile_Data">The Tile</param>
    /// <param name="tile_GameObject">The GameObject of the Tile</param>
    void OnTileChanged(Tile tile_Data)
    {
        // Check if tileGameObjectMap actually contains the tile_Data key
        if (!tileGameObjectMap.ContainsKey(tile_Data))
        {
            Debug.LogError("tileGameObjectMap doesn't contain the tile_Data -- did you forget to add the tile to the dictionary? Or forgot to unregister a callback?");
            return;
        }

        // Grabbing tile_GameObject from the dictionary. Using the tile_Data as the key
        GameObject tile_GameObject = tileGameObjectMap[tile_Data];

        // Check if the returned GameObject from the dictionary isn't null
        if (tile_GameObject == null)
        {
            Debug.LogError("tileGameObjectMap's returned GameObject is null -- did you forget to add the tile to the dictionary? Or forgot to unregister a callback?");
            return;
        }

        // Change to correct tile Sprite
        if (tile_Data.Type == TileType.Floor)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = floorSprite;
        else if (tile_Data.Type == TileType.Empty)
            tile_GameObject.GetComponent<SpriteRenderer>().sprite = null;
        else
            Debug.LogError("OnTileTypeChanged - Unknown tile type.");
    }

    /// <summary>
    /// Sub function, to make code little cleaner.
    /// Check if: there are neighbouring tiles, if those tiles have installedObject on them & if those objects are of the same type.
    /// </summary>
    /// <param name="tile">Tile to check.</param>
    /// <param name="installedObject">InstalledObject to compare with.</param>
    /// <returns>True or false</returns>
    public bool TileCheck(Tile tile, InstalledObject installedObject)
    {
        return (tile != null && tile.InstalledObject != null && tile.InstalledObject.ObjectType == installedObject.ObjectType);
    }
}
