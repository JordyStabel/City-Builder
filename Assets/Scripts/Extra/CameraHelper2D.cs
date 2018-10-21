//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class CameraHelper2D : MonoBehaviour {

    [SerializeField]
    [Tooltip("Should be the same as 'Pixels Per Unit' for a sprite.")]
    private float pixelsPerUnit = 32f;

    [SerializeField]
    [Tooltip("")]
    private float zoom = 240f;

    [SerializeField]
    [Tooltip("")]
    private bool usePixelScale = false;

    [SerializeField]
    [Tooltip("")]
    private float pixelScale = 4f;

    Vector3 cameraPosition = Vector3.zero;

    public void ApplyZoom()
    {
        if (usePixelScale == false)
        {
            // Use either Screen.height or Screen.width as smallestDimenstion, depending on which one is the smallest
            float smallestDimenstion = (Screen.height < Screen.width ? Screen.height : Screen.width);

            // Find the closest pixelScale depending on the zoom
            pixelScale = Mathf.Clamp((smallestDimenstion / zoom), 1f, 8f);
        }

        /// Set the orthographicSize of the main camera
        /// orthographicSize is the amount of pixels in heigh of half the screen.
        /// So devide the height by (pixelsPerUnit * pixelScale) to get the total size.
        /// But we need half that because orthographicSize is only half the screen height
        Camera.main.orthographicSize = (Screen.height / (pixelsPerUnit * pixelScale) * 0.5f);
    }

    public float RoundToNearestPixel(float position)
    {
        // Get the actual amount of pixels used on the screen per unit
        float screenPixelsPerUnit = Screen.height / (Camera.main.orthographicSize * 2f);

        // Convert the position into position in screen pixels.
        // Round the number to a int
        float pixelValue = Mathf.Round(position * screenPixelsPerUnit);

        // Convert it to Unity units and return
        return pixelValue / screenPixelsPerUnit;
    }

    public void AdjustCamera()
    {
        // Set the camera to it's new position using RoundToNearestPixel
        Camera.main.transform.position = new Vector3(
            RoundToNearestPixel(cameraPosition.x),
            RoundToNearestPixel(cameraPosition.y),
            -5f);
    }

    public void MoveCamera(Vector3 direction)
    {
        ApplyZoom();
        cameraPosition += direction;
        AdjustCamera();
    }

    public void MoveCameraTo(Vector3 position)
    {
        ApplyZoom();
        cameraPosition = position;
        AdjustCamera();
    }
}
