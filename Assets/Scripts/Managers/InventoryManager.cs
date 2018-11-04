//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;

public class InventoryManager {

    // List of all inventories in the world
    // In the future this might get split up into seperate list per room

    public Dictionary<string, List<LooseObject>> inventory;

    /// <summary>
    /// InventoryManager constructor. Will create a new inventory
    /// </summary>
    public InventoryManager()
    {
        inventory = new Dictionary<string, List<LooseObject>>();
    }

    public bool PlaceLooseObject(Tile tile, LooseObject looseObject)
    {
        // Check if the tile already has a LooseObjbect
        bool tileWasEmpty = (tile.LooseObject == null);

        // The tile didn't 'accept' the looseObject, thus stop
        if (tile.PlaceLooseObject(looseObject) == false)
            return false;

        // looseObject might be 'empty' thus remove it
        if (looseObject.stackSize == 0 && inventory.ContainsKey(tile.LooseObject.objectType) == true)
            inventory[looseObject.objectType].Remove(looseObject);

        // Might have created a new stack on the tile, if the tile was previously empty.
        if (tileWasEmpty)
        {
            // If there is no objectType on the dictionary already, add it
            if (inventory.ContainsKey(tile.LooseObject.objectType) == false)
                inventory[tile.LooseObject.objectType] = new List<LooseObject>();

            inventory[looseObject.objectType].Add(tile.LooseObject);
        }

        return true;
    }
}
