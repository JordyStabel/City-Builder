﻿//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System;

public class Character {

    public float X { get { return Mathf.Lerp(currentTile.X, nextTile.X, movementProgression); } }
    public float Y { get { return Mathf.Lerp(currentTile.Y, nextTile.Y, movementProgression); } }

    Tile currentTile;

    // If character isn't moving -- destinationTile = currentTile
    Tile destinationTile;

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

    public Character(Tile tile)
    {
        currentTile = destinationTile = nextTile = tile;
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
            // Grab job from jobQueue
            currentJob = currentTile.World.jobQueue.Dequeue();

            if (currentJob != null)
            {
                // TODO: Check if job is reachable

                destinationTile = currentJob.Tile;
                currentJob.RegisterJobCancelCallback(OnJobEnded);
                currentJob.RegisterJobCompleteCallback(OnJobEnded);
            }
        }

        // Is this character at the correct destination?
        if (currentTile == destinationTile)
        // Character is adjacent to the job-side => execute job
        //if (path_AStar != null && path_AStar.Length() == 1)
        {
            // Execute job if there is one
            if (currentJob != null)
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
        if (currentTile == destinationTile)
        {
            // Reset the current A* path
            path_AStar = null;
            return;
        }

        // Grab 'next' nextTile
        if (nextTile == null || nextTile == currentTile)
        {
            // Get next tile from the path (calculted path wiht A*)
            if (path_AStar == null || path_AStar.Length() == 0)
            {
                // Generate new A* path from currentTile to destinationTile
                path_AStar = new Path_AStar(currentTile.World, currentTile, destinationTile);

                // Check if path isn't null
                if (path_AStar.Length() == 0)
                {
                    Debug.LogError("Path_AStar: Returned no path to destination!");
                    // FIXME: Job should get added back to queue
                    AbandonJob();
                    path_AStar = null;
                    return;
                }
            }

            // Grab the next tile
            nextTile = path_AStar.DequeueNextTile();

            if (nextTile == currentTile)
                Debug.LogError("UpdateCharacter_Movement: nextTile == currentTile? -- Only valid for startingTile.");
        }

        //if (path_AStar.Length() == 1)
        //{
        //    return;
        //}

        // At this point there a valid nextTile to move to.

        // Total distance from A to B (pythagorean theorem)
        float totalDistanceToTravel = Mathf.Sqrt(
            Mathf.Pow(currentTile.X - nextTile.X, 2) + 
            Mathf.Pow(currentTile.Y - nextTile.Y, 2));

        // Distance to travel in one tick (one frame)
        float distanceThisTick = movementSpeed * deltaTime;

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
    /// Abandon a job, reset all the properties and re-enqueue the abandoned job.
    /// So that an other character can exectue the job (or the same character, but at a later time)
    /// </summary>
    public void AbandonJob()
    {
        nextTile = destinationTile = currentTile;
        path_AStar = null;
        currentTile.World.jobQueue.Enqueue(currentJob);
        currentJob = null;
    }

    public void SetDestination(Tile tile, bool diagonalAllowed = false)
    {
        if (currentTile.IsAdjacent(tile, diagonalAllowed) == false)
            Debug.Log("Character::SetDestination -- Destination tile isn't adjacent to current tile!");

        destinationTile = tile;
    }

    void OnJobEnded(Job job)
    {
        // Check if character is working on the correct job
        if (job != currentJob)
        {
            Debug.LogError("Character being told about a job that isn't his -- forgot to unregister...?");
            return;
        }

        // Reset job to null
        currentJob = null;
    }

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