//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using System.IO;

public class WorldController : MonoBehaviour {

    // Creating an instance of 'WorldController' which is accessible from all classes
    public static WorldController Instance { get; protected set; }

    // The world, holds all tile data
    public World World { get; protected set; }

    // static so that it doesn't get changed on re-loading a scene during run time
    static bool loadWorld = false;

    /// <summary>
    /// Create new world
    /// OnEnable instead of start, so it runs first (before any start/update function)
    /// </summary>
    void OnEnable () {

        // Setting the instance equal to this current one (with check)
        if (Instance != null)
            Debug.LogError("There shouldn't be two world controllers.");
        else
            Instance = this;

        if (loadWorld)
        {
            loadWorld = false;
            CreateWorldFromSaveFile();
        }
        else
        {
            // Create new world with empty tiles
            CreateEmptyWorld();
        }

        // Randomize all tiles in the world this was just created (thus calling the 'OnTileChanged' function for each tile)
        //World.RandomizeTiles();
	}

    void Update()
    {
        World.UpdateWorld(Time.deltaTime);
    }

    /// <summary>
    /// Check which tile the mouse hovers over and then return that tile
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>Return a tile at the current mouse coordinates</returns>
    public Tile GetTileAtWorldCoordinate(Vector2 coordinates)
    {
        // Round float to int, since the tiles are all 1 by 1 unit
        int x = Mathf.FloorToInt(coordinates.x);
        int y = Mathf.FloorToInt(coordinates.y);

        return World.GetTileAt(x, y);
    }

    /// <summary>
    /// Reload the currently loaded scene. Effectively creating a 'fresh' new world. But NOT saving the old one.
    /// </summary>
    public void NewWorld()
    {
        Debug.Log("New World button clicked.");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // CreateEmptyWorld();
    }

    public void SaveWorld()
    {
        Debug.Log("Save World button clicked.");

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(World));
        TextWriter textWriter = new StringWriter();
        xmlSerializer.Serialize(textWriter, World);
        textWriter.Close();

        PlayerPrefs.SetString("SaveGame_01", textWriter.ToString());

        string path = "C:\\Users\\Jordy\\Desktop\\Test_Save.txt";

        StreamWriter streamWriter = new StreamWriter(path);
        streamWriter.Write(textWriter.ToString());
        streamWriter.Close();

        Debug.Log(textWriter.ToString());
    }

    public void LoadWorld()
    {
        Debug.Log("Load World button clicked.");

        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Create a new empty world
    /// </summary>
    void CreateEmptyWorld()
    {
        // Create new world with empty tiles
        World = new World(100, 100);

        // Center camera in the world
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
    }

    /// <summary>
    /// Create a world with data from a save file 
    /// </summary>
    void CreateWorldFromSaveFile()
    {
        Debug.Log("CreateWorldFromSaveFile -- fired");

        // Create world from save file data
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame_01"));
        World = (World)xmlSerializer.Deserialize(reader);
        reader.Close();

        // Center camera in the world
        Camera.main.transform.position = new Vector3(World.Width / 2, World.Height / 2, Camera.main.transform.position.z);
    }
}
