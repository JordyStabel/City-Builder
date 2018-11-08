//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System;
using System.Collections.Generic;

public class Job {

    /* This class holds data for a queued job, which can include things like:
       placing wall, buidlings, machines, moving resources, work, etc. */

    // The tile the job 'sits' on and the time it will take to complete the job.
    public Tile tile;
    public float JobTime { get; protected set; }

    // What type of job is this...
    public string JobObjectType { get; protected set; }

    // Indicates if it is accepting all type of inventory/materials/resources
    public bool acceptsAnyLooseObjectItem = false;

    // Actions for completing a job and canceling a job.
    Action<Job> cb_JobComplete;
    Action<Job> cb_JobCancel;
    Action<Job> cb_JobProgressed;

    // Map a job to a looseObject, so the job 'knows' what materials it needs
    public Dictionary<string, LooseObject> looseObjectRequirements;

    /// <summary>
    /// Job constructor, create a new job
    /// </summary>
    /// <param name="tile">Tile to place job on.</param>
    /// <param name="cb_JobComplete">Function to call after job completes.</param>
    /// <param name="jobTime">How long does this job take.</param>
    public Job(Tile tile, string jobObjectType, Action<Job> cb_JobComplete, float jobTime, LooseObject[] looseObjectRequirements)
    {
        this.tile = tile;
        JobObjectType = jobObjectType;
        this.cb_JobComplete = cb_JobComplete;
        this.JobTime = jobTime;

        this.looseObjectRequirements = new Dictionary<string, LooseObject>();

        // Map all requirements with a clone of the LooseObject, so that changing it later on won't actually change the LooseObject passed
        if (looseObjectRequirements != null)
        {
            foreach (LooseObject looseObject in looseObjectRequirements)
                this.looseObjectRequirements[looseObject.objectType] = looseObject.Clone();
        }
    }

    /// <summary>
    /// Copy constructor, protected so only callable from this class (and derived classes)
    /// </summary>
    /// <param name="other">The object to copy</param>
    protected Job(Job other) {
        tile = other.tile;
        JobObjectType = other.JobObjectType;
        cb_JobComplete = other.cb_JobComplete;
        JobTime = other.JobTime;

        this.looseObjectRequirements = new Dictionary<string, LooseObject>();

        // Map all requirements with a clone of the LooseObject, so that changing it later on won't actually change the LooseObject passed
        if (looseObjectRequirements != null)
        {
            foreach (LooseObject looseObject in other.looseObjectRequirements.Values)
                this.looseObjectRequirements[looseObject.objectType] = looseObject.Clone();
        }
    }

    /// <summary>
    /// Make a copy of the given 'input' object
    /// Virtual so that it can be overriden by other derived classes.
    /// Also, this way we always call the correct 'job' constructor.
    /// </summary>
    /// <param name="other">New job</param>
    virtual public Job Clone()
    {
        return new Job(this);
    }

    /// <summary>
    /// Execute a job
    /// </summary>
    /// <param name="workTime">Time to deduct from jobTime. Higher = more work done.</param>
    public void DoWork(float workTime)
    {
        JobTime -= workTime;

        // Notify that there is work done
        if (cb_JobProgressed != null)
            cb_JobProgressed(this);

        if (JobTime <= 0)
        {
            // If there is a JobComplete callback, call it
            if (cb_JobComplete != null)
            {
                cb_JobComplete(this);
            }
        }
    }

    /// <summary>
    /// Cancel job
    /// </summary>
    public void CancelJob()
    {
        // If there is a JobCancel callback, call it
        if (cb_JobCancel != null)
            cb_JobCancel(this);

        // Remove this job
        tile.World.jobQueue.Remove(this);
    }

    /// <summary>
    /// Check to see if the job has contains all the materials it needs to execute the job.
    /// </summary>
    /// <returns>hasAllMaterials</returns>
    public bool HasAllMaterials()
    {
        // Check all materials the job needs
        foreach (LooseObject looseObject in looseObjectRequirements.Values)
        {
            // If it need more materials than it currently has => return false
            if (looseObject.maxStackSize > looseObject.StackSize)
                return false;
        }

        // Else return true
        return true;
    }
    
    /// <summary>
    /// Checks if the LooseObject is required for the job and if the job doesn't already have enough.
    /// Than returns the amount still required.
    /// </summary>
    /// <param name="looseObject">LooseObject to check</param>
    /// <returns>Amount required</returns>
    public int RequiredAmount(LooseObject looseObject)
    {
        // If the job accepts any type of material, it will return the max-amount it can take
        if (acceptsAnyLooseObjectItem == true)
            return looseObject.maxStackSize;

        // Material is not a required material, because it's not in the dictionary
        if (looseObjectRequirements.ContainsKey(looseObject.objectType) == false)
            return 0;

        // Job already contain all the materials (at least for this type)
        if (looseObjectRequirements[looseObject.objectType].StackSize >= looseObjectRequirements[looseObject.objectType].maxStackSize)
            return 0;

        // The material is correct and the job needs some or more than it correctly contains
        return (looseObjectRequirements[looseObject.objectType].maxStackSize - looseObjectRequirements[looseObject.objectType].StackSize);
    }

    /// <summary>
    /// Find the first looseObject that is required for this job.
    /// </summary>
    /// <returns>Required looseObject</returns>
    public LooseObject GetFirstRequiredLooseObject()
    {
        foreach (LooseObject looseObject in looseObjectRequirements.Values)
            if (looseObject.maxStackSize > looseObject.StackSize)
                return looseObject;

        return null;
    }

    #region (Un)Register callback(s)
    /// <summary>
    /// Register action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get registered.</param>
    public void RegisterJobCompleteCallback(Action<Job> callbackFunction)
    {
        cb_JobComplete += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterJobCompleteCallback(Action<Job> callbackFunction)
    {
        cb_JobComplete -= callbackFunction;
    }

    /// <summary>
    /// Register action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get registered.</param>
    public void RegisterJobCancelCallback(Action<Job> callbackFunction)
    {
        cb_JobCancel += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterJobCancelCallback(Action<Job> callbackFunction)
    {
        cb_JobCancel -= callbackFunction;
    }

    /// <summary>
    /// Register action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get registered.</param>
    public void RegisterJobProgressedCallback(Action<Job> callbackFunction)
    {
        cb_JobProgressed += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterJobProgressedCallback(Action<Job> callbackFunction)
    {
        cb_JobProgressed -= callbackFunction;
    }
    #endregion
}
