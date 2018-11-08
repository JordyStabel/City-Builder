//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Character : IXmlSerializable {

    public float X { get { return Mathf.Lerp(currentTile.X, nextTile.X, movementProgression); } }
    public float Y { get { return Mathf.Lerp(currentTile.Y, nextTile.Y, movementProgression); } }

    // The tile the character currently is located on
    Tile currentTile;

    // If character isn't moving -- destinationTile = currentTile
    private Tile _destinationTile;
    Tile DestinationTile {
        get { return _destinationTile; }
        set
        {
            // If this is a new destination, set destinationTile and invalidate path A*
            if (_destinationTile != value)
            {
                _destinationTile = value;
                path_AStar = null;
            }
        }
    }

    // The next tile in the pathfinding sequence
    Tile nextTile;

    // A* path
    Path_AStar path_AStar;

    // 0 to 1 Progression going from currentTile to destinationTile
    float movementProgression;

    // Tiles per second a character can move
    float movementSpeed = 5f;

    Action<Character> cb_CharacterChanged;
    Job currentJob;

    // The item the character is carrying at the moment
    public LooseObject CurrentLooseObject { get; set; }

    // Used for saving and loading (serialization)
    public Character() { }

    public Character(Tile tile)
    {
        currentTile = DestinationTile = nextTile = tile;
    }

    /// <summary>
    /// Update function for a character. Needs a delta time from somewhere else.
    /// </summary>
    /// <param name="deltaTime">The amount of time passed since last update. Higher = faster.</param>
    public void UpdateCharacter(float deltaTime)
    {
        UpdateCharacter_Job(deltaTime);
        UpdateCharacter_Movement(deltaTime);

        if (cb_CharacterChanged != null)
            cb_CharacterChanged(this);
    }

    /// <summary>
    /// Update all movement related things of a character
    /// </summary>
    /// <param name="deltaTime">The time passed since last tick</param>
    void UpdateCharacter_Job(float deltaTime)
    {
        if (currentJob == null)
        {
            GetNewJob();

            // There is no job on the queue, so stop moving and return
            if (currentJob == null)
            {
                DestinationTile = currentTile;
                return;
            }
        }

        // From this point there is a reachable job!

        // STEP 1: Does the job have all the materials it needs?
        if (currentJob.HasAllMaterials() == false)
        {
            // Job is still missing some or all materials!

            // STEP 2: Is the character CARRYING anything that this job requires?
            if (CurrentLooseObject != null)
            {
                // If the materials the character is carrying are the correct materials needed for the current job
                if (currentJob.RequiredAmount(CurrentLooseObject) > 0)
                {
                    // Deliver the materials.
                    if (currentTile == currentJob.tile)
                    {
                        // Place LooseObject on the job tile
                        currentTile.World.inventoryManager.PlaceLooseObjectOnJob(currentJob, CurrentLooseObject);

                        // This will call cb_JobProgressedCallbacks, because even though there might not be any work done.
                        // Updating materials might be usefull or tell that the requirements are met.
                        currentJob.DoWork(0);

                        // Is character still carrying stuff?
                        if (CurrentLooseObject.StackSize == 0)
                            CurrentLooseObject = null;
                        else
                        {
                            Debug.LogError("Character is still carrrying materials, which it shouldn't be. FIX THIS");
                            // Temp. solution
                            CurrentLooseObject = null;
                        }
                    }
                    // Move to the job tile and deliver the materials to the job-side
                    else
                    {
                        DestinationTile = currentJob.tile;
                        return;
                    }
                }
                // Character is carrying something but not what the job requires
                else
                {
                    // TODO: Move to nearest empty tile and dump materials there.
                    if (currentTile.World.inventoryManager.PlaceLooseObjectOnTile(currentTile, CurrentLooseObject) == false)
                    {
                        Debug.LogError("Character tried to dump inventory into invalid tile!");
                        // FIXME: Don't dump materials, just doing that so code can continue
                        CurrentLooseObject = null;
                    }
                }
            }
            else
            {
                // It this point the job still requires more materials but the character isn't carrying any => go get the required materials.

                // Is this this character already standing on a tile with the required materials for the current job?
                // Pick up the materials
                if (currentTile.LooseObject != null &&
                    // Either the currentTile has no installedObject OR it's NOT a stockpile OR the job can take stuff from a stockpile
                    (currentJob.canTakeFromStockpile || currentTile.InstalledObject == null || currentTile.InstalledObject.IsStockpile() == false) 
                    && currentJob.RequiredAmount(currentTile.LooseObject) > 0)
                {
                    currentTile.World.inventoryManager.PlaceLooseObjectOnCharacter(
                        this, 
                        currentTile.LooseObject, 
                        currentJob.RequiredAmount(currentTile.LooseObject));
                }
                // Otherwise move towards the tile containing the required materials.
                else
                {
                    // Find the first material/looseObject in the job that isn't yet supplied
                    LooseObject required = currentJob.GetFirstRequiredLooseObject();

                    LooseObject supplier = currentTile.World.inventoryManager.GetNearestLooseObjectOfType(
                        required.objectType, 
                        currentTile, 
                        (required.maxStackSize - required.StackSize), 
                        currentJob.canTakeFromStockpile);

                    // There simple aren't any materials in the world that the character's job requires.
                    if (supplier == null)
                    {
                        // TODO: Leave character in idle state if there are no other jobs to do aka stop the update loop
                        Debug.LogWarning("No tile contains looseObjects of type: " + required.objectType + " to satisfy the job's requirements.");
                        AbandonJob();
                        return;
                    }
                    DestinationTile = supplier.tile;
                    return;

                    // FIXME: This a very wrong way of doing it!

                    // If already on a tile with the required materials, pick them up.
                    //destinationTile = someTileWithTheMaterials;
                }
            }

            // Can't continue, untill the job has all required materials.
            return;
        }

        // Job contains all required materials.
        // Set the destination tile to the job-side tile
        DestinationTile = currentJob.tile;

        // Is this character at the correct destination AND is there a job to do?
        if (currentTile == DestinationTile && currentJob != null)
        {
            // Character is at the correct tile and has a job, so execute the job's "DoWork".
            // This is mostly counting down jobTime and might call its "Job Complete" callback action.
            currentJob.DoWork(deltaTime);
        }
    }

    /// <summary>
    /// Update all movement related things of a character
    /// </summary>
    /// <param name="deltaTime">The time passed since last tick</param>
    void UpdateCharacter_Movement(float deltaTime)
    {
        // Is movement needed? Is character already at the correct location
        if (currentTile == DestinationTile)
        {
            // Reset the current A* path
            path_AStar = null;
            return;
        }

        // currentTile = The tile a character is currently standing on and might be in the process of leaving
        // nextTile = The tile a character is entering
        // destinationTile = The final destination -- a character never gets here directly, but it's used for pathfinding instead

        // Grab 'next' nextTile
        if (nextTile == null || nextTile == currentTile)
        {
            // Get next tile from the path (calculted path wiht A*)
            if (path_AStar == null || path_AStar.Length() == 0)
            {
                // Generate new A* path from currentTile to destinationTile
                path_AStar = new Path_AStar(currentTile.World, currentTile, DestinationTile);

                // Check if path isn't null
                if (path_AStar.Length() == 0)
                {
                    Debug.LogError("Path_AStar: Returned no path to destination!");
                    // FIXME: Job should get added back to queue
                    AbandonJob();
                    return;
                }

                // Grab the first tile and right away override it.
                // First tile is the file the character is standing on, this results in some problems. (tile might have a movementCost of 0, like a wall that was just build by the character)
                nextTile = path_AStar.DequeueNextTile();
            }

            // Grab the next tile
            nextTile = path_AStar.DequeueNextTile();

            // Check for error
            if (nextTile == currentTile)
                Debug.LogError("UpdateCharacter_Movement: nextTile == currentTile? -- Only valid for startingTile.");
        }

        // At this point there a valid nextTile to move to.

        // Total distance from A to B (pythagorean theorem)
        float totalDistanceToTravel = Mathf.Sqrt(
            Mathf.Pow(currentTile.X - nextTile.X, 2) + 
            Mathf.Pow(currentTile.Y - nextTile.Y, 2));

        /// Check if movementCost is 0, which it never should be.
        /// Set nextTile to null, so that the character won't move.
        /// Set path_AStart to null, this one is no longer valid.
        /// MovementCost can be 0 if something was build in the mean time
        if (nextTile.IsEnterable() == EnterAbility.Never)
        {
            Debug.LogError("FIXME: A charcter tried to enter an unwalkable tile!");
            nextTile = null;
            path_AStar = null;
            return;
        }
        // Character can't enter right now, but will be in the near future. (like a door or something)
        else if (nextTile.IsEnterable() == EnterAbility.Soon)
        {
            // Temp
            return;
        }

        // Distance to travel in one tick (one frame)
        // nextTile.MovementCost can be 0 (which would throw an error) but it should never happen.
        // Moving to a tile with a movementCost of 0 is NOT allowed
        float distanceThisTick = (movementSpeed / nextTile.MovementCost)* deltaTime;

        // Progression in one tick
        float progressionThisTick = distanceThisTick / totalDistanceToTravel;

        // Increase the movement progression each tick
        movementProgression += progressionThisTick;

        // Has the character reached it's destination yet?
        if (movementProgression >= 1)
        {
            currentTile = nextTile;
            movementProgression = 0;
        }
    }

    /// <summary>
    /// Grab a new job from the queue. This will first check if the tile is reachable.
    /// </summary>
    private void GetNewJob()
    {
        // Grab job from jobQueue
        currentJob = currentTile.World.jobQueue.Dequeue();

        if (currentJob == null)
            return;

        // Set the destinationtile
        DestinationTile = currentJob.tile;

        // Add job callback actions
        currentJob.RegisterJobCancelCallback(OnJobEnded);
        currentJob.RegisterJobCompleteCallback(OnJobEnded);

        // Check if the job tile is reachable
        // NOTE: We might not be pathing to it right away (due to requiring materials),
        // but we still need to verify that the final tile is reachalbe.
        #region Check tile is reachable
        // Generate new A* path from currentTile to destinationTile
        path_AStar = new Path_AStar(currentTile.World, currentTile, DestinationTile);

        // Check if path isn't null
        if (path_AStar.Length() == 0)
        {
            Debug.LogError("Path_AStar: Returned no path to target job tile!");
            // FIXME: Job should get added back to queue
            AbandonJob();
            DestinationTile = currentTile;
        }
        #endregion
    }

    /// <summary>
    /// Abandon a job, reset all the properties and re-enqueue the abandoned job.
    /// So that an other character can exectue the job (or the same character, but at a later time)
    /// </summary>
    public void AbandonJob()
    {
        nextTile = DestinationTile = currentTile;
        currentTile.World.jobQueue.Enqueue(currentJob);
        currentJob = null;
    }

    public void SetDestination(Tile tile, bool diagonalAllowed = false)
    {
        if (currentTile.IsAdjacent(tile, diagonalAllowed) == false)
            Debug.Log("Character::SetDestination -- Destination tile isn't adjacent to current tile!");

        DestinationTile = tile;
    }

    void OnJobEnded(Job job)
    {
        job.UnregisterJobCancelCallback(OnJobEnded);
        job.UnregisterJobCompleteCallback(OnJobEnded);

        // Check if character is working on the correct job
        if (job != currentJob)
        {
            Debug.LogError("Character being told about a job that isn't his -- forgot to unregister...?");
            return;
        }

        // Reset job to null
        currentJob = null;
    }

    #region Saving & Loading
    public XmlSchema GetSchema()
    {
        // Just here so IXmlSerializable doesn't throw an error :)
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // Save data here
        // Currenty only saving character position NOT the job, movementspeed, ect.
        writer.WriteAttributeString("X", currentTile.X.ToString());
        writer.WriteAttributeString("Y", currentTile.Y.ToString());
    }

    /// <summary>
    /// Read everything from a Xml-file
    /// </summary>
    /// <param name="reader">Needs XmlReader, so it's all from the same reader</param>
    public void ReadXml(XmlReader reader)
    {
        // Load data from Xml-file

        // Nothing for right now
    }
    #endregion

    #region (Un)Register callback(s)
    /// <summary>
    /// Register action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get registered.</param>
    public void RegisterCharacterChangedCallback(Action<Character> callbackFunction)
    {
        cb_CharacterChanged += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterCharacterChangedCallback(Action<Character> callbackFunction)
    {
        cb_CharacterChanged -= callbackFunction;
    }
    #endregion
}
