//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class AutomaticVerticalSize : MonoBehaviour {

    [Header("Height of button")]
    [Tooltip("Default: 35, higher = bigger")]
    public float buttonHeight = 35f;

	void Start () {
        AdjustSize();
    }
	
    /// <summary>
    /// Set the height of the container equal to the child count multiplied by the childheight.
    /// </summary>
    public void AdjustSize()
    {
        Vector2 newSize = this.GetComponent<RectTransform>().sizeDelta;
        newSize.y = this.transform.childCount * buttonHeight;
        this.GetComponent<RectTransform>().sizeDelta = newSize;
    }
}
