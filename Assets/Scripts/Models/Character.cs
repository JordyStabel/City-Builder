//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using System;

public class Character {

    public float X { get { return Mathf.Lerp(currentTile.X, destinationTile.X, movementProgression); } }
    public float Y { get { return Mathf.Lerp(currentTile.Y, destinationTile.Y, movementProgression); } }

    Tile currentTile;

    // If character isn't moving -- destinationTile = currentTile
    Tile destinationTile;

    // 0 to 1 Progression going from currentTile to destinationTile
    float movementProgression;

    // Tiles per second a character can move
    float movementSpeed = 2f;

    Action<Character> cb_CharacterChanged;
    Job currentJob;

    public Character(Tile tile)
    {
        currentTile = destinationTile = tile;
    }

    /// <summary>
    /// Update function for a character. Needs a delta time from somewhere else.
    /// </summary>
    /// <param name="deltaTime">The amount of time passed since last update. Higher = faster.</param>
    public void UpdateCharacter(float deltaTime)
    {
        if (currentJob == null)
        {
            // Grab job from jobQueue
            currentJob = currentTile.World.jobQueue.Dequeue();

            if (currentJob != null)
            {
                destinationTile = currentJob.Tile;
                currentJob.RegisterJobCancelCallback(OnJobEnded);
                currentJob.RegisterJobCompleteCallback(OnJobEnded);
            }
        }

        // Is this character at the correct destination?
        if (currentTile == destinationTile)
        {
            if (currentJob != null)
            {
                currentJob.DoWork(deltaTime);
            }
            return;
        }

        // Total distance from A to B
        float totalDistanceToTravel = Mathf.Sqrt(Mathf.Pow(currentTile.X - destinationTile.X, 2) + Mathf.Pow(currentTile.Y - destinationTile.Y, 2));

        // Distance to travel in one tick (one frame)
        float distanceThisTick = movementSpeed * deltaTime;

        // Progression in one tick
        float progressionThisTick = distanceThisTick / totalDistanceToTravel;

        // Increase the movement progression each tick
        movementProgression += progressionThisTick;

        // Has the character reached it's destination yet?
        if (movementProgression >= 1)
        {
            currentTile = destinationTile;
            movementProgression = 0;
        }

        if (cb_CharacterChanged != null)
            cb_CharacterChanged(this);
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
            Debug.LogError("Character being tola about a job that isn't his -- forgot to unregister...?");
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
