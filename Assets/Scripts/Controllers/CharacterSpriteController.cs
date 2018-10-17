//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour {

    // Bind data to a GameObject
    Dictionary<Character, GameObject> characterGameObjectMap;
    Dictionary<string, Sprite> characterSpritesMap;

    // Sprites array
    Sprite[] sprites;

    // Get reference to the World
    World World { get { return WorldController.Instance.World; } }

    void Start()
    {
        // Instatiate dictionary that binds a GameObject with data
        characterGameObjectMap = new Dictionary<Character, GameObject>();
        characterSpritesMap = new Dictionary<string, Sprite>();

        // Load all sprites on start
        LoadSprites();

        // Register function
        World.RegisterCharacterCreatedCallback(OnCharacterCreated);

        // DEBUG ONLY!
        Character character = World.CreateCharacter(World.GetTileAt(World.Width / 2, World.Height / 2));
        //character.SetDestination(World.GetTileAt((World.Width / 2) + 1, World.Height / 2));
    }

    /// <summary>
    /// Load sprite from resources folder
    /// </summary>
    private void LoadSprites()
    {
        // Loading all sprites and adding them to the dictionary
        sprites = Resources.LoadAll<Sprite>("Sprites");
        foreach (Sprite sprite in sprites)
            characterSpritesMap[sprite.name] = sprite;
    }

    /// <summary>
    /// Create the visual component linked to the given data
    /// </summary>
    /// <param name="character">Functions as the data.</param>
    public void OnCharacterCreated(Character character)
    {
        // Creating new gameObject
        GameObject character_GameObject = new GameObject();

        // Add data and installedObject_GameObject to the dictionary (data is the key)
        characterGameObjectMap.Add(character, character_GameObject);

        // Adding a name and position to each installedObject_GameObject
        character_GameObject.name = "Character_" + Random.Range(0, 1000);
        character_GameObject.transform.position = new Vector2(character.X, character.Y);
        // Setting the new tile as a child, maintaining a clean hierarchy
        character_GameObject.transform.SetParent(this.transform, true);

        SpriteRenderer spriteRenderer = character_GameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = characterSpritesMap["AI_Helper_PlaceHolder"];
        spriteRenderer.sortingLayerName = "Characters";

        // Register action, which will run the funtion when 'tile' gets changed
        character.RegisterCharacterChangedCallback(OnCharacterChanged);
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
    private void OnCharacterChanged(Character character)
    {
        if (characterGameObjectMap.ContainsKey(character) == false)
        {
            Debug.LogError("OnCharacterChanged -- Trying to change visuals for Character not in dictionary!");
            return;
        }

        GameObject character_GameObject = characterGameObjectMap[character];
        //character_GameObject.GetComponent<SpriteRenderer>().sprite = GetSpriteForInstalledObject(installedObject);

        character_GameObject.transform.position = new Vector2(character.X, character.Y);
    }
}
