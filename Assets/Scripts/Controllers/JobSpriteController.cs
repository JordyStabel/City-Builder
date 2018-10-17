//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System.Collections.Generic;

public class JobSpriteController : MonoBehaviour {

    // Grab instance of InstalledObjectSpriteController
    InstalledObjectSpriteController installedObjectSpriteController;

    //Bind jobs with gameobjects
    Dictionary<Job, GameObject> jobGameObjectMap;

	void Start () {

        jobGameObjectMap = new Dictionary<Job, GameObject>();

        // Register 'OnJobCreated' when job gets added to the jobQueue in the World instance
        WorldController.Instance.World.jobQueue.RegisterJobCreatedCallback(OnJobCreated);
    }

    /// <summary>
    /// Stuff to do after creating a new job:
    /// - Get the correct sprite
    /// - Register callback actions
    /// </summary>
    /// <param name="job">The newly created job.</param>
    void OnJobCreated(Job job)
    {
        // Grab reference first time creating a job
        if (installedObjectSpriteController == null)
            installedObjectSpriteController = InstalledObjectSpriteController.Instance;

        // Creating new gameObject
        GameObject job_GameObject = new GameObject();

        // Add job and gameobject to dictionary (job is the key)
        jobGameObjectMap.Add(job, job_GameObject);

        // Adding a name and position to each installedObject_GameObject
        job_GameObject.name = "JOB_" + job.JobObjectType + "_" + job.Tile.X + "_" + job.Tile.Y;
        job_GameObject.transform.position = new Vector2(job.Tile.X, job.Tile.Y);

        // Setting the new tile as a child, maintaining a clean hierarchy
        job_GameObject.transform.SetParent(this.transform, true);

        // Create spriterenderer, set its sprite and sortinglayer
        SpriteRenderer spriteRenderer = job_GameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = installedObjectSpriteController.GetSpriteForInstalledObject(job.JobObjectType);
        spriteRenderer.sortingLayerName = "Jobs";
        spriteRenderer.color = new Color(0.5f, 1f, 0.5f, 0.25f);

        // Registering callback actions for new job
        job.RegisterJobCompleteCallback(OnJobEnded);
        job.RegisterJobCancelCallback(OnJobEnded);
    }

    /// <summary>
    /// Unregister job from callbacks and remove its gameobject
    /// </summary>
    /// <param name="job">The job to end & destroy</param>
    void OnJobEnded(Job job)
    {
        GameObject jobGameObject = jobGameObjectMap[job];

        job.UnregisterJobCancelCallback(OnJobEnded);
        job.UnregisterJobCompleteCallback(OnJobEnded);

        Destroy(jobGameObject);
    }
}
