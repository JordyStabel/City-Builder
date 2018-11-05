//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System;

public class LooseObject {
    // Things like resource piles, or not yet installed objects

    public string objectType = "Bricks";
    public int maxStackSize = 64;

    protected int stackSize = 1;

    public int StackSize
    {
        get { return stackSize; }
        set { if (stackSize != value)
            {
                stackSize = value;
                if (cb_LooseObjectChanged != null)
                    cb_LooseObjectChanged(this);
            }
        }
    }

    // Callback action for changing LooseObject
    Action<LooseObject> cb_LooseObjectChanged;

    public Tile tile;
    public Character character;

    // Default constructor used for loading and saving
    public LooseObject() { }

    public LooseObject(string objectType, int maxStackSize, int stackSize)
    {
        this.objectType = objectType;
        this.maxStackSize = maxStackSize;
        StackSize = stackSize;
    }

    /// <summary>
    /// Copy constructor, protected so only callable from this class (and derived classes)
    /// </summary>
    /// <param name="other">The object to copy</param>
    protected LooseObject(LooseObject other)
    {
        objectType = other.objectType;
        maxStackSize = other.maxStackSize;
        StackSize = other.StackSize;
    }

    /// <summary>
    /// Make a copy of this object
    /// Virtual so that it can be overriden by other derived classes.
    /// Also, this way we always call the correct 'LooseObject' constructor.
    /// </summary>
    /// <param name="other">New LooseObject clone</param>
    public virtual LooseObject Clone()
    {
        return new LooseObject(this);
    }

    #region (Un)Register callback(s)
    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void RegisterLooseObjectChanged(Action<LooseObject> callbackFunction)
    {
        cb_LooseObjectChanged += callbackFunction;
    }

    /// <summary>
    /// Unregister action with given function
    /// </summary>
    /// <param name="callbackFunction">The function that is going to get unregistered.</param>
    public void UnregisterLooseObjectChanged(Action<LooseObject> callbackFunction)
    {
        cb_LooseObjectChanged -= callbackFunction;
    }
    #endregion
}
