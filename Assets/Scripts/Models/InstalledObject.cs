//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System;

// Things like buildings, machines, roads, ect.
public class InstalledObject {
    
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
    float movementCost;
    
    // A machine might be 3x2, but the graphic is only 3x1, the extra space is for walking/driving
    int width;
    int height;

    // Callback action for changing something on or with InstalledObject
    Action<InstalledObject> cb_OnChanged;

    // TODO: Implement larger objects
    // TODO: Implement object rotation

    // Protected constructor, so that in other classes no 'empty' InstalledObjects can get created.
    protected InstalledObject() { }

    /// <summary>
    /// Create and return 'baseObject' InstalledObject
    /// </summary>
    /// <param name="objectType">The type of object/building</param>
    /// <param name="movementCost">The movementcost. Higher = slower and more expensive movement</param>
    /// <param name="width">Actual width of the object. (visually might be smaller)</param>
    /// <param name="height">Actual height of the object. (visually might be smaller)</param>
    /// <returns>A 'baseObject' of InstalledObject</returns>
    static public InstalledObject CreateBaseObject(string objectType, float movementCost = 1f, int width = 1, int height = 1)
    {
        InstalledObject installedObject = new InstalledObject
        {
            ObjectType = objectType,
            movementCost = movementCost,
            width = width,
            height = height
        };

        return installedObject;
    }

    /// <summary>
    /// Create full fletched InstalledObject
    /// </summary>
    /// <param name="baseObject">baseObject</param>
    /// <param name="tile">The tile, this InstalledObject will 'sit' on.</param>
    /// <returns>InstalledObject</returns>
    static public InstalledObject PlaceObject(InstalledObject baseObject, Tile tile)
    {
        InstalledObject installedObject = new InstalledObject
        {
            ObjectType = baseObject.ObjectType,
            movementCost = baseObject.movementCost,
            width = baseObject.width,
            height = baseObject.height,
            Tile = tile
        };

        // FIXME: Only works for 1x1 objects!

        // Can't place the object. Spot was likely already occupied. Don't return the created Object!
        if (tile.PlaceInstalledObject(installedObject) == false)
            return null;

        return installedObject;
    }

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
}
