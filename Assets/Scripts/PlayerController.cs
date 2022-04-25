using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform playerCamera = null;
    public float mouseSensitivity = 3.5f;
    public bool lockCursor = true;

    private float cameraPitch = 0.0f;
    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    void Update()
    {
        UpdateMouseLook();
    }

    void UpdateMouseLook()
    {
        //Get the position of the cursor:
        Vector2 mouseDelta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        this.RotateCameraVertically(mouseDelta);
        this.RotateCameraHorizontally(mouseDelta);
    }

    private void RotateCameraVertically(Vector2 mouseDelta)
    {
        /*
        Negative degrees pushes the camera UPWARDS, so we want to invert the mouse value so that the camera does not pitch in the 
        opposite direction of the cursor's movement:
        */
        cameraPitch -= mouseDelta.y * mouseSensitivity;

        //Make sure that the user cannot move the camera higher than looking straight up or looking straight down:
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
    }
    
    private void RotateCameraHorizontally(Vector2 mouseDelta)
    {
        //Rotate horizontally (i.e., AROUND the up axis) using the horizontal mouse delta:
        transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);
    }
}
