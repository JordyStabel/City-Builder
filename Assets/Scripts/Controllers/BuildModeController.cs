//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class BuildModeController : MonoBehaviour{

    // Creating an instance of 'BuildModeController' which is accessible from all classes
    public static BuildModeController Instance { get; protected set; }

    bool buildModeIsObjects = false;
    TileType buildModeTileType = TileType.Floor;
    string buildModeObjectType;

    void OnEnable()
    {
        // Setting the instance equal to this current one (with check)
        if (Instance != null)
            Debug.LogError("There shouldn't be two buildmode controllers.");
        else
            Instance = this;
    }

    /// <summary>
    /// Quick debug function, generate small map with installedObjects and floors
    /// </summary>
    public void DoPathfindingTest()
    {
        WorldController.Instance.World.SetupPathfindingExample();

        Path_TileGraph path_TileGraph = new Path_TileGraph(WorldController.Instance.World);
    }

    /// <summary>
    /// Set mode to 'build floor'
    /// </summary>
    public void SetMode_BuildFloor()
    {
        buildModeIsObjects = false;
        buildModeTileType = TileType.Floor;
    }

    /// <summary>
    /// Set mode to 'destroy'
    /// </summary>
    public void SetMode_Destroy()
    {
        buildModeIsObjects = false;
        buildModeTileType = TileType.Empty;
    }

    /// <summary>
    /// Set the build-mode and object-type
    /// </summary>
    /// <param name="objectType">The objectType the player wants to use</param>
    public void SetMode_BuildInstalledObject(string objectType)
    {
        // A wall isn't a Tile type. Wall is an "InstalledObject" that exists on top of a Tile.
        buildModeIsObjects = true;
        buildModeObjectType = objectType;
    }

    public void ExecuteBuild(Tile tile)
    {
        if (buildModeIsObjects)
        {
            // Building Objects

            // FIXME: This instantly builds installedObjects
            //WorldController.Instance.World.PlaceInstalledObject(buildModeObjectType, tile);

            string installedObjectType = buildModeObjectType;

            // Can we build installedObjects in the seleceted tile? Run validation function
            if (WorldController.Instance.World.IsInstalledObjectPlacementValid(installedObjectType, tile) && tile.pendingInstalledObjectJob == null)
            {
                // Create new job with a callback action passed with it
                Job job;

                // Check if the jobtype already excists or we need to create a new one
                if (WorldController.Instance.World.installedJobBaseObjects.ContainsKey(installedObjectType))
                {
                    // Create a clone of the installedJobBaseObject
                    job = WorldController.Instance.World.installedJobBaseObjects[installedObjectType].Clone();

                    // Assign the correct tile to the job
                    job.tile = tile;
                }
                else
                {
                    Debug.LogError("There is no installedJobBaseObjects for: " + installedObjectType);
                    job = new Job(tile, installedObjectType, InstalledObjectActions.JobComplete_InstalledObject, 0.1f, null);
                }
                
                // Placing the new job in the jobQueue
                WorldController.Instance.World.jobQueue.Enqueue(job);

                // Assigning tile with the new job
                tile.pendingInstalledObjectJob = job;

                // Remove job from tile when job gets canceled
                job.RegisterJobCancelCallback((theJob) => { tile.pendingInstalledObjectJob = null; });
            }
        }
        else
        {
            tile.Type = buildModeTileType;
        }
    }
}
