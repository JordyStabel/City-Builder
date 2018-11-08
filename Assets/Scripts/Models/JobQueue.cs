//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;
using System;
using UnityEngine;

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
        // Job has a negative job-time => insta-complete it instead of adding it to the queue
        if (job.JobTime < 0)
        {
            job.DoWork(0);
            return;
        }

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

    /// <summary>
    /// Remove a job from the queue
    /// </summary>
    /// <param name="job">Job to remove from queue</param>
    public void Remove(Job job)
    {
        // Create a temp list of jobs
        List<Job> jobs = new List<Job>(jobQueue);

        // Check if job actually exists in the queue
        if (jobs.Contains(job) == false)
        {
            //Debug.LogError("Trying to remove a job that doesn't exist in the queue!");
            // Most likely, this job wasn't on the queue because a character was 'working' on it
            return;
        }

        // Remove the selected job
        jobs.Remove(job);

        // Set the jobQueue to the temp list without the removed job
        jobQueue = new Queue<Job>(jobs);
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
