//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class CameraController : MonoBehaviour {

    public CameraHelper2D cameraHelper2D;
    public float cameraMovementSpeed = 0.1f;

    Vector3 randomPosition = Vector3.zero;
    float randomTimer = 0.1f;

    //void Update()
    //{
    //    randomTimer -= Time.deltaTime;

    //    if (randomTimer <= 0f)
    //    {
    //        randomTimer = 2f;
    //        randomPosition = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), transform.position.z);
    //    }

    //    Vector3 moveDirection = (randomPosition - cameraHelper2D.transform.position).normalized;
    //    cameraHelper2D.MoveCamera(moveDirection * cameraMovementSpeed * Time.deltaTime);
    //}
}
