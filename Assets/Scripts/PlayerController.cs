using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform playerCamera = null;
    public float mouseSensitivity = 3.5f;
    public bool lockCursor = true;
    public float walkSpeed = 6.0f;
    [Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;

    private float cameraPitch = 0.0f;
    CharacterController controller = null;

    Vector2 currentDirection = Vector2.zero;
    Vector2 currentDirectionVelocity = Vector2.zero;

    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    void Update()
    {
        UpdateMouseLook();
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        //Get the direction of the input from the WASD keys:
        Vector2 targetDirection = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
            );

        //We have to normalize the input vector because the diagonal vectors are slightly longer than the H and V vectors;
        targetDirection.Normalize();

        currentDirection = Vector2.SmoothDamp(currentDirection, targetDirection, ref currentDirectionVelocity, moveSmoothTime);

        Vector3 velocity = (transform.forward * targetDirection.y + transform.right * targetDirection.x * walkSpeed);

        controller.Move(velocity * Time.deltaTime);

    }
    private void UpdateMouseLook()
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
