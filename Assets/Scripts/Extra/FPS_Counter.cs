//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using System.Collections;
using UnityEngine;

public class FPS_Counter : MonoBehaviour {

    Rect fpsRect;
    GUIStyle style;
    float fps;

	void Start () {
        fpsRect = new Rect(20, 20, 200, 50);
        //style = new GUIStyle();
        //style.fontSize = 20;

        StartCoroutine(RecalculateFPS());
	}

    private IEnumerator RecalculateFPS()
    {
        while (true)
        {
            fps = Mathf.Round(fps = 1 / Time.deltaTime);

            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnGUI()
    {
        GUI.Label(fpsRect, "FPS: " + fps);
    }
}
