//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using UnityEditor;

/// <summary>
/// Add extra editor functions: like buttons and run functions in the editor, so not during run-time
/// </summary>
[CustomEditor(typeof(WorldController))]
public class WorldControllerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        // Make normal properties show up in inspector
        DrawDefaultInspector();

        // Add UI components to the inspector
        if (GUILayout.Button("Randomize world"))
        {
            if (WorldController.Instance != null)
                WorldController.Instance.World.RandomizeTiles();
            else
                Debug.LogWarning("Can't access World.");
        }
    }
}
