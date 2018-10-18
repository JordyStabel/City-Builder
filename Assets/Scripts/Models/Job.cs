//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System;

public class Job {

    /* This class holds data for a queued job, which can include things like:
       placing wall, buidlings, machines, moving resources, work, etc. */
    
    // The tile the job 'sits' on and the time it will take to complete the job.
    public Tile Tile { get; protected set; }
    float jobTime = 1f;

    // What type of job is this...
    public string JobObjectType { get; protected set; }

    // Actions for completing a job and canceling a job.
    Action<Job> cb_JobComplete;
    Action<Job> cb_JobCancel;

    /// <summary>
    /// Job constructor, create a new job
    /// </summary>
    /// <param name="tile">Tile to place job on.</param>
    /// <param name="cb_JobComplete">Function to call after job completes.</param>
    /// <param name="jobTime">How long does this job take.</param>
    public Job(Tile tile, string jobObjectType, Action<Job> cb_JobComplete, float jobTime = .1f)
    {
        Tile = tile;
        JobObjectType = jobObjectType;
        this.cb_JobComplete = cb_JobComplete;
        this.jobTime = jobTime;
    }

    /// <summary>
    /// Execute a job
    /// </summary>
    /// <param name="workTime">Time to deduct from jobTime. Higher = more work done.</param>
    public void DoWork(float workTime)
    {
        jobTime -= workTime;

        if (jobTime <= 0)
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
    #endregion
}
