//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InstalledObjectSpriteController : MonoBehaviour {

    // Bind data to a GameObject
    Dictionary<InstalledObject, GameObject> installedObjectGameObjectMap;
    Dictionary<string, Sprite> installedObjectSpritesMap;

    // Sprites array
    Sprite[] sprites;

    // Get reference to the World
    World World { get { return WorldController.Instance.World; } }

    // Creating an instance of 'WorldController' which is accessible from all classes
    public static InstalledObjectSpriteController Instance { get; protected set; }

    void Start () {

        // Setting the instance equal to this current one (with check)
        if (Instance != null)
            Debug.LogError("There shouldn't be two world controllers.");
        else
            Instance = this;

        // Instatiate dictionary that binds a GameObject with data
        installedObjectGameObjectMap = new Dictionary<InstalledObject, GameObject>();
        installedObjectSpritesMap = new Dictionary<string, Sprite>();

        // Load all sprites on start
        LoadSprites();

        // Register function
        World.RegisterInstalledObjectCreated(OnInstalledObjectCreated);

        // Loop through any PRE-EXISTING installedObjects (installedObject that were loaded in OnEnable) and call the OnCreatedCallback manually
        foreach (InstalledObject installedObject in World.installedObjects)
            OnInstalledObjectCreated(installedObject);
    }

    /// <summary>
    /// Load sprite from resources folder
    /// </summary>
    private void LoadSprites()
    {
        // Loading all sprites and adding them to the dictionary
        sprites = Resources.LoadAll<Sprite>("Sprites/Walls");
        foreach (Sprite sprite in sprites)
            installedObjectSpritesMap[sprite.name] = sprite;
    }

    /// <summary>
    /// Create the visual component linked to the given data
    /// </summary>
    /// <param name="installedObject">Functions as the data.</param>
    public void OnInstalledObjectCreated(InstalledObject installedObject)
    {
        // Creating new gameObject
        GameObject installedObject_GameObject = new GameObject();

        // Add data and installedObject_GameObject to the dictionary (data is the key)
        installedObjectGameObjectMap.Add(installedObject, installedObject_GameObject);

        // Adding a name and position to each installedObject_GameObject
        installedObject_GameObject.name = installedObject.ObjectType + "_" + installedObject.Tile.X + "_" + installedObject.Tile.Y;
        installedObject_GameObject.transform.position = new Vector2(installedObject.Tile.X, installedObject.Tile.Y);
        // Setting the new tile as a child, maintaining a clean hierarchy
        installedObject_GameObject.transform.SetParent(this.transform, true);
        
        SpriteRenderer spriteRenderer = installedObject_GameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSpriteForInstalledObject(installedObject);
        spriteRenderer.sortingLayerName = "TileUI";

        // Register action, which will run the funtion when 'tile' gets changed
        installedObject.RegisterOnChangedCallback(OnInstalledObjectChanged);
    }

    /// <summary>
    /// Return the correct sprite for a given installedObject
    /// </summary>
    /// <param name="installedObject">The installedObject that needs a sprite.</param>
    /// <returns>Sprite</returns>
    public Sprite GetSpriteForInstalledObject(InstalledObject installedObject)
    {
        // Return sprite with the same name as installedObject.ObjectType
        if (installedObject.IsLinkedToNeighbour == false)
        {
            return installedObjectSpritesMap[installedObject.ObjectType];
        }

        string spriteName = installedObject.ObjectType + "_";

        /* Check for neighbours: North, East, South & West (in that order)
         * Check if: there are neighbouring tiles, 
         * if those tiles have installedObject on them 
         * if those objects are of the same type. */
        Tile tile;
        int x = installedObject.Tile.X;
        int y = installedObject.Tile.Y;

        // Check North
        tile = World.GetTileAt(x, (y + 1));
        if (TileCheck(tile, installedObject))
            spriteName += "N";

        // Check East
        tile = World.GetTileAt((x + 1), y);
        if (TileCheck(tile, installedObject))
            spriteName += "E";

        // Check South
        tile = World.GetTileAt(x, (y - 1));
        if (TileCheck(tile, installedObject))
            spriteName += "S";

        // Check West
        tile = World.GetTileAt((x - 1), y);
        if (TileCheck(tile, installedObject))
            spriteName += "W";

        // If there isn't a sprite with this current spritename, throw error and return null
        if (installedObjectSpritesMap.ContainsKey(spriteName) == false)
        {
            Debug.LogError("installedObjectSpritesMap doesn't contain a sprite with the name: " + spriteName);
            return null;
        }

        return installedObjectSpritesMap[spriteName];
    }


    public Sprite GetSpriteForInstalledObject(string installedObjectType)
    {
        if (installedObjectSpritesMap.ContainsKey(installedObjectType))
            return installedObjectSpritesMap[installedObjectType];

        // Needed for walls (and maybe in the future other objects) because of the naming of walls
        if (installedObjectSpritesMap.ContainsKey(installedObjectType + "_"))
            return installedObjectSpritesMap[installedObjectType + "_"];

        Debug.LogError("installedObjectSpritesMap doesn't contain a sprite with the name: " + installedObjectType);
        return null;
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

    /// <summary>
    /// Function that needs to run after changing an InstalledObject.
    /// </summary>
    /// <param name="installedObject">InstalledObject</param>
    private void OnInstalledObjectChanged(InstalledObject installedObject)
    {
        if (installedObjectGameObjectMap.ContainsKey(installedObject) == false)
        {
            Debug.LogError("OnInstalledObjectChanged -- Trying to change visuals for InstalledObject not in dictionary!");
            return;
        }

        GameObject installedObject_GameObject = installedObjectGameObjectMap[installedObject];
        installedObject_GameObject.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(installedObject);
    }
}
