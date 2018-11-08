//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;
using UnityEngine;

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

    /// <summary>
    /// Place LooseObject onto a tile.
    /// Will check if there was already a looseObject of the same type there and add it if that's the case.
    /// Otherwise it will create a new LooseObject.
    /// </summary>
    /// <param name="tile">Tile to place the LooseObject on</param>
    /// <param name="looseObject">LooseObject to place</param>
    /// <returns>Is success</returns>
    public bool PlaceLooseObjectOnTile(Tile tile, LooseObject looseObject)
    {
        // Check if the tile already has a LooseObjbect
        bool tileWasEmpty = (tile.LooseObject == null);

        // The tile didn't 'accept' the looseObject, thus stop
        if (tile.PlaceLooseObject(looseObject) == false)
            return false;

        // looseObject might be 'empty' thus remove it
        CleanUpLooseObject(looseObject);

        // Might have created a new stack on the tile, if the tile was previously empty.
        if (tileWasEmpty)
        {
            // If there is no objectType on the dictionary already, add it
            if (inventory.ContainsKey(tile.LooseObject.objectType) == false)
                inventory[tile.LooseObject.objectType] = new List<LooseObject>();

            inventory[looseObject.objectType].Add(tile.LooseObject);

            // Notify the world a looseObject was created
            tile.World.OnLooseObjectCreated(tile.LooseObject);
        }

        return true;
    }

    /// <summary>
    /// Place LooseObject onto a character.
    /// Will check if there was already a looseObject of the same type there and add it if that's the case.
    /// Otherwise it will create a new LooseObject.
    /// </summary>
    /// <param name="character">The character to place the looseObject on</param>
    /// <param name="sourceLooseObject">The LooseObject to place</param>
    /// <returns>Is success</returns>
    public bool PlaceLooseObjectOnCharacter(Character character, LooseObject sourceLooseObject, int amount = -1)
    {
        // If amount is less than 0, add the whole stack
        if (amount < 0)
        {
            // Amount is the amount left in the stack
            amount = sourceLooseObject.StackSize;
        }
        else
        {
            // Amount is either the asked for amount or the amount left in the stack.
            // Whichever value is the LOWEST!
            amount = Mathf.Min(amount, sourceLooseObject.StackSize);
        }

        // Check if character has a looseObject.
        // If it doesn't => create one and add it the inventory list
        if (character.CurrentLooseObject == null)
        {
            character.CurrentLooseObject = sourceLooseObject.Clone();
            character.CurrentLooseObject.StackSize = 0;
            inventory[character.CurrentLooseObject.objectType].Add(character.CurrentLooseObject);
        }
        else if (character.CurrentLooseObject.objectType != sourceLooseObject.objectType)
        {
            Debug.LogError("Character is trying to pick up a mismatched looseObject objectType!");
            return false;
        }

        // Else just add the required amount
        character.CurrentLooseObject.StackSize += amount;

        // If there is still room for materials, add them. Else, empty the looseObject
        if (character.CurrentLooseObject.maxStackSize < character.CurrentLooseObject.StackSize)
        {
            // Add as much material as the character will accept. Rest will remain in the looseObject
            sourceLooseObject.StackSize = character.CurrentLooseObject.StackSize - character.CurrentLooseObject.maxStackSize;

            // Character will now contain the required amount of materials of this type.
            character.CurrentLooseObject.StackSize = character.CurrentLooseObject.maxStackSize;
        }
        else
            sourceLooseObject.StackSize -= amount;

        // looseObject might be 'empty' thus remove it
        CleanUpLooseObject(sourceLooseObject);

        return true;
    }

    /// <summary>
    /// Place LooseObject onto a job.
    /// Will check if there was already a looseObject of the same type there and add it if that's the case.
    /// Otherwise it will create a new LooseObject.
    /// </summary>
    /// <param name="job">Tile to place the LooseObject on</param>
    /// <param name="looseObject">LooseObject to place</param>
    /// <returns>Is success</returns>
    public bool PlaceLooseObjectOnJob(Job job, LooseObject looseObject)
    {
        if (job.looseObjectRequirements.ContainsKey(looseObject.objectType) == false)
        {
            Debug.LogError("Trying to place wrong type of materials onto job-side!");
            return false;
        }

        // Add the whole looseObject to the looseObject of the job-side
        job.looseObjectRequirements[looseObject.objectType].StackSize += looseObject.StackSize;

        // If there is still room for materials, add them. Else, empty the looseObject
        if (job.looseObjectRequirements[looseObject.objectType].maxStackSize < job.looseObjectRequirements[looseObject.objectType].StackSize)
        {
            // Add as much material as the job will accept. Rest will remain in the looseObject
            looseObject.StackSize = job.looseObjectRequirements[looseObject.objectType].StackSize - job.looseObjectRequirements[looseObject.objectType].maxStackSize;

            // Job will now contain the required amount of materials of this type.
            job.looseObjectRequirements[looseObject.objectType].StackSize = job.looseObjectRequirements[looseObject.objectType].maxStackSize;
        }
        else
            looseObject.StackSize = 0;

        // looseObject might be 'empty' thus remove it
        CleanUpLooseObject(looseObject);

        return true;
    }

    /// <summary>
    /// Checks if a LooseObject stack is 0. If so removes it and cleans up the tile and character.
    /// </summary>
    /// <param name="looseObject">LooseObject to cleanup</param>
    private void CleanUpLooseObject(LooseObject looseObject)
    {
        // looseObject might be 'empty' thus remove it
        if (looseObject.StackSize == 0 && inventory.ContainsKey(looseObject.objectType) == true)
        {
            inventory[looseObject.objectType].Remove(looseObject);

            // Reset looseObject of tile
            if (looseObject.tile != null)
            {
                looseObject.tile.LooseObject = null;
                looseObject.tile = null;
            }

            // Reset looseObject of character
            if (looseObject.character != null)
            {
                looseObject.character.CurrentLooseObject = null;
                looseObject.character = null;
            }
        }
    }

    /// <summary>
    /// Will return a required LooseObject that is the least far away from the destination tile.
    /// </summary>
    /// <param name="objectType">ObjectType required</param>
    /// <param name="tile">Destination tile</param>
    /// <param name="requiredAmount">Required amount. If no stack has enough, it instead returns the largest stack.</param>
    /// <returns></returns>
    public LooseObject GetNearestLooseObjectOfType(string objectType, Tile tile, int requiredAmount, bool canTakeFromStockpile)
    {
        /// Things to FIX:
        ///     A) Return the actual nearest item
        ///     B) There is currently no way of knowing what the actual nearest tile of required items is

        if (inventory.ContainsKey(objectType) == false)
        {
            Debug.LogError("GetNearestLooseObjectOfType -- no items of desired type found!");
            return null;
        }

        foreach (LooseObject looseObject in inventory[objectType])
            if (looseObject.tile != null &&
                // Either this can take stuff from a stockpile OR the currentTile has no installedObject OR it's NOT a stockpile
                (canTakeFromStockpile == true || looseObject.tile.InstalledObject == null || looseObject.tile.InstalledObject.IsStockpile() == false))
                return looseObject;

        return null;
    }
}
