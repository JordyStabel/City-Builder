//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;
using System;

public class JobQueue {

    // Holds all queued jobs
    Queue<Job> jobQueue;

    // Actions for creating a job
    Action<Job> cb_JobCreated;

    public JobQueue()
    {
        jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job job)
    {
        jobQueue.Enqueue(job);

        if (cb_JobCreated != null)
            cb_JobCreated(job);
    }

    /// <summary>
    /// Get a job from the jobqueue
    /// </summary>
    /// <returns>A job</returns>
    public Job Dequeue()
    {
        if (jobQueue.Count == 0)
            return null;

        return jobQueue.Dequeue();
    }

    #region (Un)Register callback(s)
    /// <summary>
    /// Register action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get registered.</param>
    public void RegisterJobCreatedCallback(Action<Job> callbackFunction)
    {
        cb_JobCreated += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterJobCreatedCallback(Action<Job> callbackFunction)
    {
        cb_JobCreated -= callbackFunction;
    }
    #endregion
}
