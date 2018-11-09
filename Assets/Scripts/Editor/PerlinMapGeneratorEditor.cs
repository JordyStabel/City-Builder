//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class PerlinMapGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        // Get a mapgenerator from the target, which is the object this script is sitting on
        MapGenerator mapGenerator = (MapGenerator)target;

        // If any value is changed => generate a new map
        if (DrawDefaultInspector())
        {
            if (mapGenerator.autoUpdate)
                mapGenerator.GenerateMap();
        }
            
        // If button is being pressed => generate & display a new perlin noise map
        if (GUILayout.Button("Generate Perlin Noise"))
            mapGenerator.GenerateMap();
    }
}
