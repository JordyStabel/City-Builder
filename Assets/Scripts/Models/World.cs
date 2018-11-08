//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class World : IXmlSerializable {

    // A 2D array that holds the tile data.
    Tile[,] tiles;

    // Lists of all objects in the world
    public List<Character> characters;
    public List<InstalledObject> installedObjects;
    public List<Room> rooms;
    public InventoryManager inventoryManager;

    // The pathfinding graph used to navigate the world
    public Path_TileGraph path_TileGraph;

    // Bind ObjectType to a InstalledObject
    Dictionary<string, InstalledObject> installedBaseObjects;

    // Bind ObjectType to a Job
    public Dictionary<string, Job> installedJobBaseObjects;

    // Number of tiles as width in the world
    public int Width { get; protected set; }

    // Number of tiles as height in the world
    public int Height { get; protected set; }

    // Callback action for creating installedObject and changing tile
    Action<InstalledObject> cb_InstalledObjectCreated;
    Action<Tile> cb_TileChanged;
    Action<Character> cb_CharacterCreated;
    Action<LooseObject> cb_LooseObjectCreated;

    public JobQueue jobQueue;

    /// <summary>
    /// Create a new World
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public World (int width, int height)
    {
        // Create an empty world
        SetUpWorld(width, height);

        // Create one character
        CreateCharacter(tiles[Width / 2, Height / 2]);
    }

    // Empty constructor used for saving and loading
    public World()
    {

    }

    /// <summary>
    /// Get the 'world' room which is always the first room in the list
    /// </summary>
    /// <returns>'world' room</returns>
    public Room GetWorldRoom()
    {
        return rooms[0];
    }

    /// <summary>
    /// Add a room to the world rooms list
    /// </summary>
    /// <param name="room">The room to add</param>
    public void AddRoom(Room room)
    {
        rooms.Add(room);
    }

    /// <summary>
    /// Delete a room unless it's the 'world' room
    /// </summary>
    /// <param name="room">The room that needs to be deleted</param>
    public void DeleteRoom(Room room)
    {
        if (room == GetWorldRoom())
        {
            Debug.LogError("Can't delete the 'world' room!");
            return;
        }

        // Remove this room from the rooms list
        rooms.Remove(room);

        // All tiles that belong to this room need to be re-assigned to the 'world' room
        room.UnAssignAllTiles();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// Not directly done in World constructor, so that the 'load from world' World constructer can use the same function.
    /// </summary>
    /// <param name="width">Width in number of tiles</param>
    /// <param name="height">Height in number of tiles</param>
    void SetUpWorld(int width, int height)
    {
        jobQueue = new JobQueue();
        characters = new List<Character>();
        installedObjects = new List<InstalledObject>();
        rooms = new List<Room>();
        inventoryManager = new InventoryManager();
        // Add the 'world' room
        rooms.Add(new Room());

        // Set width & height
        Width = width;
        Height = height;

        // Create new tile array. Default size: 100 * 100 = 10,000 tiles
        tiles = new Tile[width, height];

        // Instantiate tiles and adding them to the tiles array
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
                // Add tile to room number 0 (the world itself)
                tiles[x, y].room = rooms[0];
            }
        }
        Debug.Log("World created with: " + width * height + " tiles -- width: " + width + ", height: " + height);

        // Create new dictionary of baseInstalledObjects
        installedBaseObjects = new Dictionary<string, InstalledObject>();
        installedJobBaseObjects = new Dictionary<string, Job>();
        CreateBaseInstalledObjects();
    }

    /// <summary>
    /// Update function for the world. Needs a delta time from somewhere else.
    /// </summary>
    /// <param name="deltaTime">The amount of time passed since last update. Higher = faster.</param>
    public void UpdateWorld(float deltaTime)
    {
        // Update each Character in the list
        foreach (Character character in characters)
            character.UpdateCharacter(deltaTime);

        // Update each InstalledObject in the list
        foreach (InstalledObject installedObject in installedObjects)
            installedObject.Update_InstalledObject(deltaTime);
    }

    /// <summary>
    /// Create a new character
    /// </summary>
    /// <param name="tile">Tile that the character will get spawned on.</param>
    public Character CreateCharacter(Tile tile)
    {
        // Spawn character in center of the world
        Character character = new Character(tile);
        characters.Add(character);

        if (cb_CharacterCreated != null)
            cb_CharacterCreated(character);

        return character;
    }

    /// <summary>
    /// Create InstalledObject and add it to the dictionary
    /// </summary>
    void CreateBaseInstalledObjects()
    {
        // In the future this will get replaced by a function that reads all the data from a save-file/database

        // Create and add InstalledObject to the dictionary
        installedBaseObjects.Add("Wall", new InstalledObject(
            "Wall",     // InstalledObject ID (type)
            0,          // Movementcost: 0 = imappable, default = 1
            1,          // Width, default = 1
            1,          // Height, default = 1
            true,       // Links to neighbours and 'forms' one large object, default = false
            true        // Can enclose rooms
            ));
        // Add job requirements to the dictionary
        installedJobBaseObjects.Add("Wall", new Job(null, "Wall", InstalledObjectActions.JobComplete_InstalledObject, 1f, new LooseObject[] { new LooseObject("Bricks", 5, 0) }));

        installedBaseObjects.Add("Door", new InstalledObject(
            "Door",     // InstalledObject ID (type)
            1,          // Movementcost: 0 = imappable, default = 1
            1,          // Width, default = 1
            1,          // Height, default = 1
            false,      // Links to neighbours and 'forms' one large object, default = false
            true        // Can enclose rooms
            ));

        installedBaseObjects.Add("Road", new InstalledObject(
            "Road",     // InstalledObject ID (type)
            0.5f,       // Movementcost: 0 = imappable, default = 1
            1,          // Width, default = 1
            1,          // Height, default = 1
            false,      // Links to neighbours and 'forms' one large object, default = false
            false       // Can't enclose rooms
            ));

        installedBaseObjects.Add("Stockpile", new InstalledObject(
            "Stockpile",     // InstalledObject ID (type)
            1,          // Movementcost: 0 = imappable, default = 1
            1,          // Width, default = 1
            1,          // Height, default = 1
            true,       // Links to neighbours and 'forms' one large object, default = false
            false        // Can enclose rooms
            ));
        // Add update action
        installedBaseObjects["Stockpile"].RegisterUpdateAction(InstalledObjectActions.Stockpile_UpdateAction);
        installedBaseObjects["Stockpile"].color = new Color(255, 255, 255);
        // Add job requirements to the dictionary
        installedJobBaseObjects.Add("Stockpile", new Job(null, "Stockpile", InstalledObjectActions.JobComplete_InstalledObject, -1f, null));

        // Add parameters and update-actions for newly created installedObjects
        installedBaseObjects["Door"].SetParameter("OpenValue", 0);
        installedBaseObjects["Door"].SetParameter("isOpening", 0);
        installedBaseObjects["Door"].RegisterUpdateAction(InstalledObjectActions.Door_UpdateAction);
        installedBaseObjects["Door"].IsEnterable = InstalledObjectActions.Door_IsEnterable;
    }

    /// <summary>
    /// Returns tile at certain coordinates
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>The tile: <see cref="Tile"/>.</returns>
    public Tile GetTileAt(int x, int y)
    {
        // Check if coordinates are in range of the world
        if (x >= Width || x < 0 || y >= Height || y < 0)
            return null;

        return tiles[x, y];
    }

    /// <summary>
    /// Quick debug function, generate pre-made map with installedObjects and floors
    /// </summary>
    public void SetupPathfindingExample()
    {
        Debug.Log("SetupPathfindingExample");

        // Make a set of floors/walls to test pathfinding with.

        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l - 5; x < l + 15; x++)
        {
            for (int y = b - 5; y < b + 15; y++)
            {
                tiles[x, y].Type = TileType.Floor;


                if (x == l || x == (l + 9) || y == b || y == (b + 9))
                {
                    if (x != (l + 9) && y != (b + 4))
                    {
                        PlaceInstalledObject("Wall", tiles[x, y]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Test function, randomize tile type
    /// </summary>
    public void RandomizeTiles()
    {
        Debug.Log("Tiles randomized!");

        // For each tile in the world, give it a random tileType
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                    tiles[x, y].Type = TileType.Empty;
                else
                    tiles[x, y].Type = TileType.Floor;
            }
        }
    }

    /// <summary>
    /// Search for correct InstalledObject in dictionary and place it on a tile
    /// </summary>
    /// <param name="objectType">InstalledObject key.</param>
    /// <param name="tile">Tile to place InstalledObject on.</param>
    public InstalledObject PlaceInstalledObject(string objectType, Tile tile)
    {
        // TODO: Implement multiple tiles support
        // TODO: Implement rotation

        // Check if the dictionary contains an object with the given key (objectType)
        if (installedBaseObjects.ContainsKey(objectType) == false)
        {
            Debug.LogError("installedBaseObjects doesn't contain a baseObject for key: " + objectType);
            return null;
        }

        // Actually place the object on a tile
        InstalledObject installedObject = InstalledObject.PlaceObject(installedBaseObjects[objectType], tile);

        // Does the roomGraph need to recalculated? Only needs to happen if installedObject CAN create rooms
        if (installedObject.RoomEnclosure == true)
        {
            Room.RunRoomFloodFill(installedObject);
        }

        if (installedObject == null)
        {
            // Failed to place installedObject -- most likely there was already something there.
            return null;
        }

        // If the callback action has registered function, call/execute them
        if (cb_InstalledObjectCreated != null)
        {
            cb_InstalledObjectCreated(installedObject);
            // Flag current pathfinding graph as invalid, because a new object has been placed.
            // Only if the movementcost isn't 1 because that's the default value
            if (installedObject.MovementCost != 1)
                InvalidateTileGraph();
        }

        // Add it to the global list
        installedObjects.Add(installedObject);

        return installedObject;
    }

    /// <summary>
    /// Gets called when tile changes/updates, triggers callback actions.
    /// </summary>
    /// <param name="tile">Tile to change/update.</param>
    void OnTileChanged(Tile tile)
    {
        // Return if there are no registered functions
        if (cb_TileChanged == null)
            return;

        cb_TileChanged(tile);

        InvalidateTileGraph();
    }

    /// <summary>
    /// Needs to get called whenever a change to the world means the old pathfinding info is no longer valid.
    /// </summary>
    public void InvalidateTileGraph()
    {
        path_TileGraph = null;
    }

    /// <summary>
    /// Validate if its allowed to place a installedObject on a certain tile
    /// </summary>
    /// <param name="installedObjectType">The objectType to validate.</param>
    /// <param name="tile">The tile to validate.</param>
    /// <returns>isValid</returns>
    public bool IsInstalledObjectPlacementValid(string installedObjectType, Tile tile)
    {
        return installedBaseObjects[installedObjectType].IsValidPosition(tile);
    }

    /// <summary>
    /// Ask for a baseInstalledObject from dictionary
    /// </summary>
    /// <param name="installedObjectType">Key for object</param>
    /// <returns>baseInstalledObject</returns>
    public InstalledObject GetInstalledBaseObject(string installedObjectType)
    {
        // Check if dictionary contains something with the given key
        if (installedBaseObjects.ContainsKey(installedObjectType) == false)
        {
            Debug.LogError("No InstalledObject with type: " + installedObjectType);
            return null;
        }
            
        return installedBaseObjects[installedObjectType];
    }

    public void OnLooseObjectCreated(LooseObject looseObject)
    {
        if (cb_LooseObjectCreated != null)
            cb_LooseObjectCreated(looseObject);
    }

    #region Saving & Loading
    public XmlSchema GetSchema()
    {
        // Just here so IXmlSerializable doesn't throw an error :)
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {     
        // Save data here
        writer.WriteAttributeString("Width", Width.ToString() );
        writer.WriteAttributeString("Height", Height.ToString() );

        writer.WriteStartElement("Tiles");
        // Loop through all tiles in the world
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // Only save tiles of type 'empty'
                if (tiles[x, y].Type != TileType.Empty)
                {
                    writer.WriteStartElement("Tile");
                    tiles[x, y].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("InstalledObjects");
        // Loop through all installedObjects in the world
        foreach (InstalledObject installedObject in installedObjects)
        {
            writer.WriteStartElement("InstalledObject");
            installedObject.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Characters");
        // Loop through all the characters in the world
        foreach (Character character in characters)
        {
            writer.WriteStartElement("Character");
            character.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    /// <summary>
    /// Read everything from a Xml-file
    /// </summary>
    /// <param name="reader">Needs XmlReader, so it's all from the same reader</param>
    public void ReadXml(XmlReader reader)
    {
        // Load data here
        Debug.Log("World::ReadXml -- fired");

        // Get width and height from XML file 
        // (parsing to int could throw an error if typos are made or (expected) data isn't there)
        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));
        
        // Create a World with width & height values from the Xml-file
        SetUpWorld(Width, Height);

        // Loop through all data from the Xml-file and read all Tiles
        while (reader.Read())
        {
            // If reader is reading 'Tiles', let each tile read & set its own data
            switch (reader.Name)
            {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "InstalledObjects":
                    ReadXml_IstalledObjects(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
            }
        }

        // DEBUG ONLY! REMOVE LATER!
        LooseObject looseObject = new LooseObject("Bricks", 64, 64);
        Tile temp = GetTileAt(Width / 2, Height / 2);
        inventoryManager.PlaceLooseObjectOnTile(temp, looseObject);
        if (cb_LooseObjectCreated != null)
            cb_LooseObjectCreated(temp.LooseObject);

        looseObject = new LooseObject("Bricks", 64, 9);
        temp = GetTileAt((Width / 2) + 2, Height / 2);
        inventoryManager.PlaceLooseObjectOnTile(temp, looseObject);
        if (cb_LooseObjectCreated != null)
            cb_LooseObjectCreated(temp.LooseObject);

        looseObject = new LooseObject("Bricks", 64, 2);
        temp = GetTileAt(Width / 2, (Height / 2) + 1);
        inventoryManager.PlaceLooseObjectOnTile(temp, looseObject);
        if (cb_LooseObjectCreated != null)
            cb_LooseObjectCreated(temp.LooseObject);
    }

    /// <summary>
    /// Loop through all 'Tile' nodes in the 'Tiles' element of the Xml-file
    /// </summary>
    /// <param name="reader">Needs XmlReader, so it's all from the same reader</param>
    void ReadXml_Tiles(XmlReader reader)
    {
        // If true, there is at least one tile
        if (reader.ReadToDescendant("Tile"))
        {
            // Do-while loop, will run at least once, but will stop if the while statement isn't true anymore
            // So if there are no more "Tile" siblings left
            do
            {
                // Get the X & Y values for each tile (again, parsing to int could throw an error)
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                // Let each tile read & set its own data from the Xml-file
                // Also needs to same reader
                tiles[x, y].ReadXml(reader);
            }
            // Loop through all sibling elements
            while (reader.ReadToNextSibling("Tile"));
        }
    }

    /// <summary>
    /// Loop through all 'InstalledObject' nodes in the 'InstalledObjects' element of the Xml-file
    /// </summary>
    /// <param name="reader">Needs XmlReader, so it's all from the same reader</param>
    void ReadXml_IstalledObjects(XmlReader reader)
    {
        // If true, there is at least one tile
        if (reader.ReadToDescendant("InstalledObject"))
        {
            // Do-while loop, will run at least once, but will stop if the while statement isn't true anymore
            // So if there are no more "InstalledObject" siblings left
            do
            {
                // Get the InstalledObject position for each InstalledObject
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                // Place installedObject from Xml-file
                InstalledObject installedObject = PlaceInstalledObject(reader.GetAttribute("ObjectType"), tiles[x, y]);
                installedObject.ReadXml(reader);
            }
            // Loop through all sibling elements
            while (reader.ReadToNextSibling("InstalledObject"));
        }
    }

    /// <summary>
    /// Loop through all 'Character' nodes in the 'Characters' element of the Xml-file
    /// </summary>
    /// <param name="reader">Needs XmlReader, so it's all from the same reader</param>
    void ReadXml_Characters(XmlReader reader)
    {
        // If true, there is at least one tile
        if (reader.ReadToDescendant("Character"))
        {
            // Do-while loop, will run at least once, but will stop if the while statement isn't true anymore
            // So if there are no more "Character" siblings left
            do
            {
                // Get the Character position for each Character
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                // Create character from Xml-file
                Character character = CreateCharacter(tiles[x, y]);
                character.ReadXml(reader);
            }
            // Loop through all sibling elements
            while (reader.ReadToNextSibling("Character"));
        }
    }
    #endregion

    #region (Un)Register callback(s)
    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void RegisterInstalledObjectCreated(Action<InstalledObject> callbackFunction)
    {
        cb_InstalledObjectCreated += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterInstalledObjectCreated(Action<InstalledObject> callbackFunction)
    {
        cb_InstalledObjectCreated -= callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void RegisterCharacterCreatedCallback(Action<Character> callbackFunction)
    {
        cb_CharacterCreated += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterCharacterCreatedCallback(Action<Character> callbackFunction)
    {
        cb_CharacterCreated -= callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void RegisterTileChanged(Action<Tile> callbackFunction)
    {
        cb_TileChanged += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterTileChanged(Action<Tile> callbackFunction)
    {
        cb_TileChanged -= callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void RegisterLooseObjectCreated(Action<LooseObject> callbackFunction)
    {
        cb_LooseObjectCreated += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterLooseObjectCreated(Action<LooseObject> callbackFunction)
    {
        cb_LooseObjectCreated -= callbackFunction;
    }
    #endregion
}
