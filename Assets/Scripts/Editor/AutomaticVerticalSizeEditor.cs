//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;
using UnityEditor;

/// <summary>
/// Add extra editor functions: like buttons and run functions in the editor, so not during run-time
/// </summary>
[CustomEditor(typeof(AutomaticVerticalSize))]
public class AutomaticVerticalSizeEditor : Editor {

    public override void OnInspectorGUI()
    {
        // Make normal properties show up in inspector
        DrawDefaultInspector();

        // Add UI components to the inspector
        if (GUILayout.Button("Recalculate size"))
            ((AutomaticVerticalSize)target).AdjustSize();
    }
}
