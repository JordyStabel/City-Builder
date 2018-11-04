//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections.Generic;
using UnityEngine;

public class Room
{
    float atmosO2 = 0;
    float atmosN = 0;
    float atmosCO2 = 0;

    List<Tile> tiles;

    public Room()
    {
        tiles = new List<Tile>();
    }

    public void AssignTile(Tile tile)
    {
        // Room already contains this tile
        if (tiles.Contains(tile))
            return;

        // Remove tile from room because it already belongs to another room
        if (tile.room != null)
            tile.room.tiles.Remove(tile);

        // Assign this room to the tile
        tile.room = this;
        tiles.Add(tile);
    }

    /// <summary>
    /// Unassign each tile in this room and re-assign it to the 'world' room.
    /// Then create a new tiles list
    /// </summary>
    public void UnAssignAllTiles()
    {
        for (int i = 0; i < tiles.Count; i++)
            tiles[i].room = tiles[i].World.GetWorldRoom();

        tiles = new List<Tile>();
    }

    public static void RunRoomFloodFill(InstalledObject sourceInstalledObject)
    {
        /// sourceInstalledObject is the installedObject that may be splitting two existing rooms, or may be the final
        /// enclosing piece to create a new room.
        /// Check the NESW neighbours of the installedObject's tile
        /// and run flood fill from those tiles

        /// If this installedObject was added to an existing room
        /// (which should always be true assuming the world is a room itself')
        /// delete that room and assign all tiles within that room to the 'world' room

        // Reference to the world (only here for prevent having to type: sourceInstalledObject.Tile.World all the time
        World world = sourceInstalledObject.Tile.World;

        // Reference to the oldroom
        Room oldRoom = sourceInstalledObject.Tile.room;

        // FloodFill each neighbouring tile
        foreach (Tile tile in sourceInstalledObject.Tile.GetNeighbours())
            FloodFill(tile, oldRoom);

        // Set the room of the installedObject to null => because it doesn't belong to any rooms, itself forms the room
        sourceInstalledObject.Tile.room = null;
        oldRoom.tiles.Remove(sourceInstalledObject.Tile);

        if (oldRoom != world.GetWorldRoom())
        {
            // Oldroom should not contain any tiles anymore
            // Re-assign tiles to 'world' room

            if (oldRoom.tiles.Count > 0)
                Debug.LogError("Oldroom still contains tiles!");

            world.DeleteRoom(oldRoom);
        }
    }

    protected static void FloodFill(Tile tile, Room oldRoom)
    {
        // If tile is null, it's the end border of the world.
        if (tile == null)
            return;

        // This tile was already assigned to another 'new' room.
        // Meaning that the derection picked isn't isolated and already part of a different room. Return without creating a room.
        if (tile.room != oldRoom)
            return;

        // The tile is a wall/door/ect. on it, so can't do floodfill
        if (tile.InstalledObject != null && tile.InstalledObject.RoomEnclosure == true)
            return;

        // This tile is 'empty space' or nothing (end of map)
        if (tile.Type == TileType.Empty)
            return;

        // Create a new room with an empty list of tiles
        Room newRoom = new Room();

        // Create a list of tiles that need to be checked
        Queue<Tile> tilesToCheck = new Queue<Tile>();
        tilesToCheck.Enqueue(tile);

        while (tilesToCheck.Count > 0)
        {
            Tile _tile = tilesToCheck.Dequeue();

            if (_tile.room == oldRoom)
            {
                newRoom.AssignTile(_tile);

                Tile[] neighbours = _tile.GetNeighbours();
                foreach (Tile temp in neighbours)
                {
                    /// Tile is null or nothing (end of map) so this 'room' is part of the 'world' room.
                    /// Thus end the floodfill and delete 'newRoom' and re-assign all the tile to 'world' room
                    if (temp == null || temp.Type == TileType.Empty)
                    {
                        newRoom.UnAssignAllTiles();
                        return;
                    }

                    // Temp tile isn't null (already checked above) AND does not belong to the old room AND has EITHER no installedObject OR an installedObject that can't form rooms
                    // If so, add the tile to the 'tilesToCheck' list
                    if (temp.room == oldRoom && (temp.InstalledObject == null || temp.InstalledObject.RoomEnclosure == false))
                        tilesToCheck.Enqueue(temp);
                }
            }
        }

        // Copy the room data to the new room
        newRoom.atmosO2 = oldRoom.atmosO2;
        newRoom.atmosN = oldRoom.atmosN;
        newRoom.atmosCO2 = oldRoom.atmosCO2;

        // Inform world that a new room has been created
        tile.World.AddRoom(newRoom);
    }
}
