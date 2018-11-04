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

        // FIXME: This
        // Temp check to see if job isn't already in the queue
        if (jobGameObjectMap.ContainsKey(job))
        {
            Debug.LogError("OnJobCreated for a job_GameObject that already exists -- likely a job being re-enqueued instead of actually getting created.");
            return;
        }

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

        if (job.JobObjectType == "Door")
        {
            /// By default the door sprite is for walls to the east & west
            /// Check to see if this door is meant for walls to the north & south
            /// If so, rotate this gameobject by 90 degrees

            Tile northTile = job.Tile.World.GetTileAt(job.Tile.X, job.Tile.Y + 1);
            Tile southTile = job.Tile.World.GetTileAt(job.Tile.X, job.Tile.Y - 1);

            // If true, there are wall to the north and south => rotate the GO 90 degress
            if (northTile != null &&
                southTile != null &&
                northTile.InstalledObject != null &&
                southTile.InstalledObject != null &&
                northTile.InstalledObject.ObjectType == "Wall" &&
                southTile.InstalledObject.ObjectType == "Wall")
            {
                job_GameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }

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
