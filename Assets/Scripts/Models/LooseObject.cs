//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

public class LooseObject {
    // Things like resource piles, or not yet installed objects

    public string objectType = "Bricks";
    public int maxStackSize = 64;
    public int stackSize = 1;

    public Tile tile;
    public Character character;

    // Default constructor
    public LooseObject() { }

    /// <summary>
    /// Copy constructor, protected so only callable from this class (and derived classes)
    /// </summary>
    /// <param name="other">The object to copy</param>
    protected LooseObject(LooseObject other)
    {
        objectType = other.objectType;
        maxStackSize = other.maxStackSize;
        stackSize = other.stackSize;
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
}
