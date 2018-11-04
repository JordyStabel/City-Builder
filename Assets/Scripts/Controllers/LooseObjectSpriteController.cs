//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LooseObjectSpriteController : MonoBehaviour {

    [SerializeField]
    private GameObject loosObjectUIPrefab;

    // Bind data to a GameObject
    Dictionary<LooseObject, GameObject> looseObjectGameObjectMap;
    Dictionary<string, Sprite> characterSpritesMap;

    // Sprites array
    Sprite[] sprites;

    // Get reference to the World
    World World { get { return WorldController.Instance.World; } }

    void Start()
    {
        // Instatiate dictionary that binds a GameObject with data
        looseObjectGameObjectMap = new Dictionary<LooseObject, GameObject>();
        characterSpritesMap = new Dictionary<string, Sprite>();

        // Load all sprites on start
        LoadSprites();

        // Register function
        World.RegisterLooseObjectCreated(OnLooseObjectCreated);

        // Loop through any PRE-EXISTING looseObjects
        foreach (string objectType in World.inventoryManager.inventory.Keys)
        {
            foreach (LooseObject looseObject in World.inventoryManager.inventory[objectType])
                OnLooseObjectCreated(looseObject);
        }
    }

    /// <summary>
    /// Load sprite from resources folder
    /// </summary>
    private void LoadSprites()
    {
        // Loading all sprites and adding them to the dictionary
        sprites = Resources.LoadAll<Sprite>("Sprites/LooseObjects");
        foreach (Sprite sprite in sprites)
            characterSpritesMap[sprite.name] = sprite;
    }

    /// <summary>
    /// Create the visual component linked to the given data
    /// </summary>
    /// <param name="looseObject">Functions as the data.</param>
    public void OnLooseObjectCreated(LooseObject looseObject)
    {
        // Creating new gameObject
        GameObject looseObject_GameObject = new GameObject();

        // Add data and installedObject_GameObject to the dictionary (data is the key)
        looseObjectGameObjectMap.Add(looseObject, looseObject_GameObject);

        // Adding a name and position to each installedObject_GameObject
        looseObject_GameObject.name = looseObject.objectType;
        looseObject_GameObject.transform.position = new Vector2(looseObject.tile.X, looseObject.tile.Y);
        // Setting the new tile as a child, maintaining a clean hierarchy
        looseObject_GameObject.transform.SetParent(this.transform, true);

        SpriteRenderer spriteRenderer = looseObject_GameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = characterSpritesMap[looseObject.objectType];
        spriteRenderer.sortingLayerName = "LooseObjects";

        // Create an UI element if the stacksize is more than 1
        if (looseObject.maxStackSize > 1)
        {
            GameObject ui_Element = Instantiate(loosObjectUIPrefab);
            ui_Element.transform.SetParent(looseObject_GameObject.transform);
            ui_Element.transform.localPosition = Vector2.zero;
            ui_Element.GetComponentInChildren<Text>().text = looseObject.stackSize.ToString();
        }

        // FIXME: Add on change callback actions
        // Register action, which will run the funtion when 'tile' gets changed
        //looseObject.RegisterCharacterChangedCallback(OnCharacterChanged);
    }

    /// <summary>
    /// Return the correct sprite for a given installedObject
    /// </summary>
    /// <param name="installedObject">The installedObject that needs a sprite.</param>
    /// <returns>Sprite</returns>
    //public Sprite GetSpriteForInstalledObject(InstalledObject installedObject)
    //{
    //    // Return sprite with the same name as installedObject.ObjectType
    //    if (installedObject.IsLinkedToNeighbour == false)
    //    {
    //        return installedObjectSpritesMap[installedObject.ObjectType];
    //    }

    //    string spriteName = installedObject.ObjectType + "_";

    //    /* Check for neighbours: North, East, South & West (in that order)
    //     * Check if: there are neighbouring tiles, 
    //     * if those tiles have installedObject on them 
    //     * if those objects are of the same type. */
    //    Tile tile;
    //    int x = installedObject.Tile.X;
    //    int y = installedObject.Tile.Y;

    //    // Check North
    //    tile = World.GetTileAt(x, (y + 1));
    //    if (TileCheck(tile, installedObject))
    //        spriteName += "N";

    //    // Check East
    //    tile = World.GetTileAt((x + 1), y);
    //    if (TileCheck(tile, installedObject))
    //        spriteName += "E";

    //    // Check South
    //    tile = World.GetTileAt(x, (y - 1));
    //    if (TileCheck(tile, installedObject))
    //        spriteName += "S";

    //    // Check West
    //    tile = World.GetTileAt((x - 1), y);
    //    if (TileCheck(tile, installedObject))
    //        spriteName += "W";

    //    // If there isn't a sprite with this current spritename, throw error and return null
    //    if (installedObjectSpritesMap.ContainsKey(spriteName) == false)
    //    {
    //        Debug.LogError("installedObjectSpritesMap doesn't contain a sprite with the name: " + spriteName);
    //        return null;
    //    }

    //    return installedObjectSpritesMap[spriteName];
    //}


    //public Sprite GetSpriteForInstalledObject(string installedObjectType)
    //{
    //    if (installedObjectSpritesMap.ContainsKey(installedObjectType))
    //        return installedObjectSpritesMap[installedObjectType];

    //    // Needed for walls (and maybe in the future other objects) because of the naming of walls
    //    if (installedObjectSpritesMap.ContainsKey(installedObjectType + "_"))
    //        return installedObjectSpritesMap[installedObjectType + "_"];

    //    Debug.LogError("installedObjectSpritesMap doesn't contain a sprite with the name: " + installedObjectType);
    //    return null;
    //}

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
    /// Function that needs to run after changing an character.
    /// </summary>
    /// <param name="character">Character</param>
    private void OnLooseObjectChanged(LooseObject looseObject)
    {
        // FIXME: Make this function work and call it

        if (looseObjectGameObjectMap.ContainsKey(looseObject) == false)
        {
            Debug.LogError("OnCharacterChanged -- Trying to change visuals for Character not in dictionary!");
            return;
        }

        GameObject character_GameObject = looseObjectGameObjectMap[looseObject];
        //character_GameObject.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(installedObject);

        //character_GameObject.transform.position = new Vector2(character.X, character.Y);
    }
}
