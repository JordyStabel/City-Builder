//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;
using System.Collections.Generic;

// Things like buildings, machines, roads, ect.
public class InstalledObject : IXmlSerializable {

    // Dictionary that maps functions to a given InstalledObject
    public Dictionary<string, float> installedObjectParameters;

    // Actions that need to run during a update tick
    public Action<InstalledObject, float> updateActions;

    /// <summary>
    /// Update tick for InstalledObjects, need deltaTime from somewhere else.
    /// </summary>
    /// <param name="deltaTime">The time between update ticks.</param>
    public void Update_InstalledObject(float deltaTime)
    {
        // If there are update actions....run them
        if (updateActions != null)
            updateActions(this, deltaTime);
    }

    // This represents the BASE tile of the object -- but large objects might require more tiles of space around it
    public Tile Tile { get; protected set; }

    // What sprite will be used for the object
    public string ObjectType { get; protected set; }
    
    /* Movement multiplier. Value of 2 would mean:
    someone/something moves twice as slowly or cost double the amount of fuel.
    Tile types and other enviromental effects may be combined aswell.
    For example, a 'rough' tile (cost 2) with a resource pile on it (cost 3) that's on fire (cost 3)
    would have a total movement cost  of 8 (2 + 3 + 3), so you'd move through this tile at 1/8th normal movement speed 
    and cost 8 times more fuel.

    SPECIAL: If movementCost = 0, then this tile is impassable. (e.g. walls, buildings, large machines)
    */ 
    public float MovementCost { get; protected set; }
    
    // A machine might be 3x2, but the graphic is only 3x1, the extra space is for walking/driving
    int width;
    int height;

    public bool IsLinkedToNeighbour { get; protected set; }

    // Callback action for changing something on or with InstalledObject
    Action<InstalledObject> cb_OnChanged;

    Func<Tile, bool> funcPositionValidation;

    // TODO: Implement larger objects
    // TODO: Implement object rotation

    // Protected constructor, so that in other classes no 'empty' InstalledObjects can get created.
    // EDIT: Changed the public, because it's needed for loading and saving.
    public InstalledObject()
    {
        installedObjectParameters = new Dictionary<string, float>();
    }

    /// <summary>
    /// Create InstalledObject from the given parameters -- this will probably ONLY ever be used for 'baseInstalledObjects'
    /// </summary>
    /// <param name="objectType">The type of object/building</param>
    /// <param name="movementCost">The movementcost. Higher = slower and more expensive movement</param>
    /// <param name="width">Actual width of the object. (visually might be smaller)</param>
    /// <param name="height">Actual height of the object. (visually might be smaller)</param>
    public InstalledObject (string objectType, float movementCost = 1f, int width = 1, int height = 1, bool isLinkedToNeighbour = false)
    {
        ObjectType = objectType;
        MovementCost = movementCost;
        this.width = width;
        this.height = height;
        IsLinkedToNeighbour = isLinkedToNeighbour;

        // Add validation function
        funcPositionValidation = __IsValidPosition;

        // Give each InstalledObject its own list installedObjectParameters
        installedObjectParameters = new Dictionary<string, float>();
    }

    /// <summary>
    /// Copy constructor, protected so only callable fromt this (and dereved classes)
    /// </summary>
    /// <param name="other">The object to copy</param>
    protected InstalledObject(InstalledObject other)
    {
        ObjectType = other.ObjectType;
        MovementCost = other.MovementCost;
        width = other.width;
        height = other.height;
        IsLinkedToNeighbour = other.IsLinkedToNeighbour;

        // Will make a copy of the dictionary form the 'other' installedObject. 
        // So that in the future each installedObject can add and remove installedObjectParameters

        // Will make a copy of the updateActions form the 'other' installedObject. 
        // So that in the future each installedObject can add and remove updateActions
        installedObjectParameters = new Dictionary<string, float>(other.installedObjectParameters);

        if (other.updateActions != null)
            updateActions = (Action<InstalledObject, float>)other.updateActions.Clone();
    }

    /// <summary>
    /// Make a copy of the given 'input' object
    /// Virtual so that it can be overriden by other derived classes.
    /// Also, this way we always call the correct 'InstalledObject' constructor.
    /// </summary>
    /// <param name="other">New installedObject</param>
    virtual public InstalledObject Clone()
    {
        return new InstalledObject(this);
    }

    /// <summary>
    /// Create full fletched InstalledObject
    /// </summary>
    /// <param name="baseObject">baseObject</param>
    /// <param name="tile">The tile, this InstalledObject will 'sit' on.</param>
    /// <returns>InstalledObject</returns>
    static public InstalledObject PlaceObject(InstalledObject baseObject, Tile tile)
    {
        // Don't place installedObject if it's not allowed
        if (baseObject.funcPositionValidation(tile) == false)
        {
            Debug.LogError("InstalledObject::PlaceObject -- Position invalid!");
            return null;
        }

        // Create new InstalledObject from baseObject using a cloning method
        InstalledObject installedObject = baseObject.Clone();

        // Set its Tile
        installedObject.Tile = tile;

        // FIXME: Only works for 1x1 objects!

        // Can't place the object. Spot was likely already occupied. Don't return the created Object!
        if (tile.PlaceInstalledObject(installedObject) == false)
            return null;

        // TODO: Can this code get cleaner? Simular bit of code in 'GetSpriteForInstalledObject' in WorldController
        // This installedObject is links itself to neighbours
        // Tell other neighbouring InstalledObject of the same type to change/update
        // Triggering it's OnChangeCallbackFunction
        if (installedObject.IsLinkedToNeighbour)
        {
            Tile tileToCheck;
            int x = tile.X;
            int y = tile.Y;

            // Check all 4 sides, not sure if this does the same as the monster chunk of code underneath it
            for (int _x = (x - 1); _x <= (x + 1); _x++)
            {
                for (int _y = (y - 1); _y <= (y + 1); _y++)
                {
                    // If North, East, South or West => run check
                    // Remove this if statement to also check diagonal neighbours
                    if ((_x == (x-1) && _y == y) || (_x == (x + 1) && _y == y) || (_x == x && _y == (y - 1)) || (_x == x && _y == (y + 1)))
                    {
                        tileToCheck = tile.World.GetTileAt(_x, _y);
                        if (tileToCheck != null && tileToCheck.InstalledObject != null
                            && tileToCheck.InstalledObject.cb_OnChanged != null
                            && tileToCheck.InstalledObject.ObjectType == installedObject.ObjectType)
                            tileToCheck.InstalledObject.cb_OnChanged(tileToCheck.InstalledObject);
                    }
                }
            }

            /*
            // Check North
            tileToCheck = tile.World.GetTileAt(x, (y + 1));
            if (tileToCheck != null && tileToCheck.InstalledObject != null 
                && tileToCheck.InstalledObject.cb_OnChanged != null 
                && tileToCheck.InstalledObject.ObjectType == installedObject.ObjectType)
                tileToCheck.InstalledObject.cb_OnChanged(tileToCheck.InstalledObject); 

            // Check East
            tileToCheck = tile.World.GetTileAt((x + 1), y);
            if (tileToCheck != null && tileToCheck.InstalledObject != null 
                && tileToCheck.InstalledObject.cb_OnChanged != null 
                && tileToCheck.InstalledObject.ObjectType == installedObject.ObjectType)
                tileToCheck.InstalledObject.cb_OnChanged(tileToCheck.InstalledObject);

            // Check South
            tileToCheck = tile.World.GetTileAt(x, (y - 1));
            if (tileToCheck != null && tileToCheck.InstalledObject != null 
                && tileToCheck.InstalledObject.cb_OnChanged != null 
                && tileToCheck.InstalledObject.ObjectType == installedObject.ObjectType)
                tileToCheck.InstalledObject.cb_OnChanged(tileToCheck.InstalledObject);

            // Check West
            tileToCheck = tile.World.GetTileAt((x - 1), y);
            if (tileToCheck != null && tileToCheck.InstalledObject != null 
                && tileToCheck.InstalledObject.cb_OnChanged != null 
                && tileToCheck.InstalledObject.ObjectType == installedObject.ObjectType)
                tileToCheck.InstalledObject.cb_OnChanged(tileToCheck.InstalledObject);
                */
        }

        return installedObject;
    }

    /// <summary>
    /// Public function that checks if placement is valid
    /// </summary>
    /// <param name="tile">The tile to check</param>
    /// <returns>isValid</returns>
    public bool IsValidPosition(Tile tile)
    {
        return funcPositionValidation(tile);
    }

    /// <summary>
    /// Validate whether it's allowed to place an installedObject on a certain tile.
    /// </summary>
    /// <param name="tile">Tile the validate.</param>
    /// <returns>isValid</returns>
    public bool __IsValidPosition(Tile tile)
    {
        // Make sure tile is of type Floor
        // Make sure tile doesn't already have installedObject

        if (tile.Type != TileType.Floor)
            return false;

        if (tile.InstalledObject != null)
            return false;

        return true;
    }

    /// <summary>
    /// Validation for placing a door
    /// </summary>
    /// <param name="tile">Tile the validate.</param>
    /// <returns>isValid</returns>
    public bool __IsValidPosition_Door(Tile tile)
    {
        // Run 'normal' validation first
        if (__IsValidPosition(tile) == false)
            return false;

        // Make sure there is either a NS or SW wall, to place the door in.
        return true;
    }

    #region Saving & Loading
    public XmlSchema GetSchema()
    {
        // Just here so IXmlSerializable doesn't throw an error :)
        return null;
    }

    /// <summary>
    /// Save 'ObjectType' & 'MovementCost' to Xml-file
    /// </summary>
    /// <param name="writer">Writer needed</param>
    public void WriteXml(XmlWriter writer)
    {
        // Save data here
        writer.WriteAttributeString("X", Tile.X.ToString());
        writer.WriteAttributeString("Y", Tile.Y.ToString());
        writer.WriteAttributeString("ObjectType", ObjectType);
        //writer.WriteAttributeString("MovementCost", MovementCost.ToString());

        foreach (string key in installedObjectParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("Name", key);
            writer.WriteAttributeString("Value", installedObjectParameters[key].ToString());
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Read and set MovementConst and installedObjectParameters from a Xml-file
    /// </summary>
    /// <param name="reader">Needs XmlReader, so it's all from the same reader</param>
    public void ReadXml(XmlReader reader)
    {
        // Read and set MovementCost
        //MovementCost = float.Parse(reader.GetAttribute("MovementCost"));

        // If true, there is at least one paramter
        if (reader.ReadToDescendant("Param"))
        {
            // Do-while loop, will run at least once, but will stop if the while statement isn't true anymore
            // So if there are no more "Param" siblings left
            do
            {
                // Get the parameters and values for each InstalledObject.
                string key = reader.GetAttribute("Name");
                float value = float.Parse(reader.GetAttribute("Value"));

                // Map the key and value with each other
                installedObjectParameters[key] = value;
            }
            // Loop through all sibling elements
            while (reader.ReadToNextSibling("Param"));
        }
    }
    #endregion

    #region (Un)Register callback(s)
    /// <summary>
    /// Register action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get registered.</param>
    public void RegisterOnChangedCallback(Action<InstalledObject> callbackFunction)
    {
        cb_OnChanged += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterOnChangedCallback(Action<InstalledObject> callbackFunction)
    {
        cb_OnChanged -= callbackFunction;
    }
    #endregion
}
