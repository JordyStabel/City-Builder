//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public static class InstalledObjectActions {

	public static void Door_UpdateAction(InstalledObject installedObject, float deltaTime)
    {
        // If the door isOpening is 'true' open the door a little bit more
        if (installedObject.GetParameter("isOpening") >= 1)
        {
            installedObject.ChangeParameter("OpenValue", (deltaTime * 4));

            // If door is fully opened, close it again (right away)
            if (installedObject.GetParameter("OpenValue") >= 1)
                installedObject.SetParameter("isOpening", 0);
        }
        // Close door again
        else
            installedObject.ChangeParameter("OpenValue", (deltaTime * -4));

        // Clamp value between 0 & 1
        installedObject.SetParameter("OpenValue", Mathf.Clamp01(installedObject.GetParameter("OpenValue")));

        // Call the callback if there is any
        if (installedObject.cb_OnChanged != null)
            installedObject.cb_OnChanged(installedObject);
    }

    public static EnterAbility Door_IsEnterable(InstalledObject installedObject)
    {
        // Door 'isOpening' = 1, means door is opening = true
        installedObject.SetParameter("isOpening", 1);

        // If door is fully open, character can enter.
        if (installedObject.GetParameter("OpenValue") >= 1)
            return EnterAbility.Yes;

        // Soon, door is going to open soonTM
        return EnterAbility.Soon;
    }

    /// <summary>
    /// Job completed callback action
    /// </summary>
    /// <param name="job">The job that will call this function</param>
    public static void JobComplete_InstalledObject(Job job)
    {
        // Place the installedObject in the world
        WorldController.Instance.World.PlaceInstalledObject(job.JobObjectType, job.tile);

        // Reset the job of the tile that this job WAS on before
        job.tile.pendingInstalledObjectJob = null;
    }


    public static LooseObject[] Stockpile_GetItemsFromFilter()
    {
        // TODO: This should be reading from some kind of UI for this stockpile

        // Since jobs copy arrays automatically, we could already have an looseObject[] prepared and just return that
        return new LooseObject[1] { new LooseObject("Bricks", 64, 0) };
    }

    public static void Stockpile_UpdateAction(InstalledObject installedObject, float deltaTime)
    {
        /// Ensure that there is a job in the queue asking for either:
        ///     (if this.stockpile empty): That ANY material/items/looseObjects can be brought to us.
        ///     (if this.stockpile contains something already): If there is still room for more, bring it to this.stockpile
    
        // TODO: This function doesn't need to run each update.
        // Instead it only needs to run when: 
        //      - It gets created
        //      - A good get delivered (at which point we reset the job)
        //      - A good gets picked up (at which point we reset the job)
        //      - The UI's filter of allowed items gets changed/updated

        // Stockpile is full
        if (installedObject.Tile.LooseObject != null && installedObject.Tile.LooseObject.StackSize >= installedObject.Tile.LooseObject.maxStackSize)
        {
            installedObject.ClearJobs();
            return;
        }

        // Maybe we already have a job queued up? In which case we're good to go
        if (installedObject.JobCount() > 0)
            return;

        // We're not full, but we don't have a job either
        /// Two possibilities: Either we have SOME inventory OR we have NO inventory

        // Check if there is an empty stockpile => should NOT happen
        if (installedObject.Tile.LooseObject != null && installedObject.Tile.LooseObject.StackSize == 0)
        {
            Debug.LogError("Stockpile has a 0 sized stack!");
            installedObject.ClearJobs();
            return;
        }

        // TODO: In the future stockpiles => rather than being a bunch of individual 1x1 tiles, should create one large single object.

        // Temp array of items that are required for the new job
        LooseObject[] requiredItems;

        // Stockpile is empty => ask/accept ANYTHING
        if (installedObject.Tile.LooseObject == null)
            requiredItems = Stockpile_GetItemsFromFilter();

        // There is already a stack that isn't full yet => add more stuff
        else
        {
            // Is there already a job => if so return
            if (installedObject.JobCount() > 0)
                return;

            LooseObject desiredLooseObject = installedObject.Tile.LooseObject.Clone();
            desiredLooseObject.maxStackSize -= desiredLooseObject.StackSize;
            desiredLooseObject.StackSize = 0;

            requiredItems = new LooseObject[] { desiredLooseObject };
        }

        // Create the new job with requiredItems
        Job job = new Job(
                installedObject.Tile,
                null,
                null,
                0,
                requiredItems
            );

        // TODO: Later on, add stockpile priorities, so that we can take from a lower priority for a higher one.
        job.canTakeFromStockpile = false;

        // Register callback and add job
        job.RegisterJobProgressedCallback(Stockpile_JobProgressed);
        installedObject.AddJob(job);
    }

    static void Stockpile_JobProgressed(Job job)
    {
        job.tile.InstalledObject.RemoveJob(job);

        // TODO: Change this when the all/any pickup job is fixed
        foreach(LooseObject looseObject in job.looseObjectRequirements.Values)
        {
            // The first looseObject that has a stacksize of greater than 0 => Create a stockpile sprite for that
            if (looseObject.StackSize > 0)
            {
                job.tile.World.inventoryManager.PlaceLooseObjectOnTile(job.tile, looseObject);

                // Return because it can only fit one item/material/looseObject
                return;
            }
        }
    }
}
