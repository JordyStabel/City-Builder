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


    public static void Stockpile_UpdateAction(InstalledObject installedObject, float deltaTime)
    {
        /// Ensure that there is a job in the queue asking for either:
        ///     (if this.stockpile empty): That ANY material/items/looseObjects can be brought to us.
        ///     (if this.stockpile contains something already): If there is still room for more, bring it to this.stockpile

        // Stockpile is empty => ask/accept ANYTHING
        if (installedObject.Tile.LooseObject == null)
        {
            // Is there already a job => if so return
            if (installedObject.JobCount() > 0)
                return;

            Job job = new Job(
                installedObject.Tile,
                null,
                null,
                0,
                // FIXME: Need to be able to indicate all/any type is okay
                // For now hard-coded 'Bricks'
                new LooseObject[1] {new LooseObject("Bricks", 64, 0)} 
            );
            job.RegisterJobProgressedCallback(Stockpile_JobProgressed);

            installedObject.AddJob(job);
        }
        // There is already a stack but it isn't full yet => add more stuff
        else if (installedObject.Tile.LooseObject.StackSize < installedObject.Tile.LooseObject.maxStackSize)
        {
            // Is there already a job => if so return
            if (installedObject.JobCount() > 0)
                return;

            LooseObject optimalLooseObject = installedObject.Tile.LooseObject.Clone();
            optimalLooseObject.maxStackSize -= optimalLooseObject.StackSize;
            optimalLooseObject.StackSize = 0;

            Job job = new Job(
                installedObject.Tile,
                null,
                null,
                0,
                // Pass it's own inventory
                new LooseObject[1] { installedObject.Tile.LooseObject }
            );
            job.RegisterJobProgressedCallback(Stockpile_JobProgressed);

            installedObject.AddJob(job);
        }
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
