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
    protected InstalledObject() { }

    /// <summary>
    /// Create and return 'baseObject' InstalledObject
    /// </summary>
    /// <param name="objectType">The type of object/building</param>
    /// <param name="movementCost">The movementcost. Higher = slower and more expensive movement</param>
    /// <param name="width">Actual width of the object. (visually might be smaller)</param>
    /// <param name="height">Actual height of the object. (visually might be smaller)</param>
    /// <returns>A 'baseObject' of InstalledObject</returns>
    static public InstalledObject CreateBaseObject(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool isLinkedToNeighbour = false)
    {
        InstalledObject installedObject = new InstalledObject
        {
            ObjectType = objectType,
            MovementCost = movementCost,
            width = width,
            height = height,
            IsLinkedToNeighbour = isLinkedToNeighbour
            
        };

        // Add validation function
        installedObject.funcPositionValidation = installedObject.__IsValidPosition;

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
        // Don't place installedObject if it's not allowed
        if (baseObject.funcPositionValidation(tile) == false)
            return null;

        InstalledObject installedObject = new InstalledObject
        {
            ObjectType = baseObject.ObjectType,
            MovementCost = baseObject.MovementCost,
            width = baseObject.width,
            height = baseObject.height,
            IsLinkedToNeighbour = baseObject.IsLinkedToNeighbour,
            Tile = tile
        };

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

            // Check North
            tileToCheck = tile.World.GetTileAt(x, (y + 1));
            if (tileToCheck != null && tileToCheck.InstalledObject != null && tileToCheck.InstalledObject.ObjectType == installedObject.ObjectType)
                tileToCheck.InstalledObject.cb_OnChanged(tileToCheck.InstalledObject); 

            // Check East
            tileToCheck = tile.World.GetTileAt((x + 1), y);
            if (tileToCheck != null && tileToCheck.InstalledObject != null && tileToCheck.InstalledObject.ObjectType == installedObject.ObjectType)
                tileToCheck.InstalledObject.cb_OnChanged(tileToCheck.InstalledObject);

            // Check South
            tileToCheck = tile.World.GetTileAt(x, (y - 1));
            if (tileToCheck != null && tileToCheck.InstalledObject != null && tileToCheck.InstalledObject.ObjectType == installedObject.ObjectType)
                tileToCheck.InstalledObject.cb_OnChanged(tileToCheck.InstalledObject);

            // Check West
            tileToCheck = tile.World.GetTileAt((x - 1), y);
            if (tileToCheck != null && tileToCheck.InstalledObject != null && tileToCheck.InstalledObject.ObjectType == installedObject.ObjectType)
                tileToCheck.InstalledObject.cb_OnChanged(tileToCheck.InstalledObject);
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
