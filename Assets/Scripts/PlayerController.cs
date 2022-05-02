using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Body")]
    public bool displayPlayerBody = true;
    public GameObject playerBody = null;
    public GameObject cameraDirectionIndicator = null;

    [Header("Camera")]
    public float minimumVerticalClamp = -30.0f;
    public float maximumVerticalClamp = 30.0f;
    public Transform playerCamera = null;
    public bool rotatePlayerBodyWithCamera = true;
    public Transform verticalSwivel = null;
    public Transform horizontalSwivel = null;

    [Header("Character Controller")]
    public CharacterController controller = null;

    [Header("Cursor")]
    public bool lockCursor = true;
    public float mousePlayerRotationSensitivity = 10.0f;
    public bool enableMouseXAxisControl = true;
    public float mouseXCameraSensitivity = 50.0f;
    public bool enableMouseYAxisControl = true;
    public float mouseYCameraSensitivity = 3.5f;

    [Header("Debug Settings")]
    public bool enableDebugMode = true;
    public bool showDirectionIndicators = true;
    public GameObject leftDirectionIndicator = null;
    public GameObject rightDirectionIndicator = null;
    public GameObject forwardsDirectionIndicator = null;
    public GameObject backwardsDirectionIndicator = null;

    [Header("Gravity")]
    public float forceOfGravity = -13.0f;

    [Header("Jumping")]
    public bool enableJumping = true;
    public string jumpKey = "space";
    public float jumpVelocity = 10.0f;

    [Header("Walk Settings")]
    public float slopeLimit = 45.0f;
    public float walkSpeed = 6.0f;

    [Range(0.0f, 0.5f)] float moveSmoothTime = .25f;
    [Range(0.0f, 0.05f)] float mouseSmoothTime = .25f;

    private float cameraPitch = 0.0f;
    private float velocityY = 0.0f;

    Vector2 currentDirection = Vector2.zero;
    Vector2 currentDirectionVelocity = Vector2.zero;

    Vector2 currentMouseDelta = Vector2.zero;
    Vector2 currentMouseDeltaVelocity = Vector2.zero;

    
    void Start()
    {
        //Retrieve the CharacterController from the current GameObject:
        //TODO: Throw an error if the current GameObject does NOT contain a valid CharacterController.

        //Set the slope limit to what is configured in the Inspector:
        controller.slopeLimit = this.slopeLimit;
        
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }

        if (this.enableDebugMode)
        {
            if (this.showDirectionIndicators) {
                this.setDirectionIndicatorsActive(true);
            } else
            {
                this.setDirectionIndicatorsActive(false);
            }
        }

        if (this.displayPlayerBody)
        {
            this.playerBody.SetActive(true);
            this.cameraDirectionIndicator.SetActive(true);
        } else
        {
            this.playerBody.SetActive(false);
            this.cameraDirectionIndicator.SetActive(false);
        }
    }

    private void setDirectionIndicatorsActive(bool isActive)
    {
        GameObject[] directionIndicators = new GameObject[] {
            this.rightDirectionIndicator,
            this.leftDirectionIndicator,
            this.forwardsDirectionIndicator,
            this.backwardsDirectionIndicator
        };
        foreach(GameObject directionIndicator in directionIndicators)
        {
            directionIndicator.SetActive(isActive);
        }
    }

    void Update()
    {
        UpdateMouseLook();
        UpdateMovement();
    }

    private float ApplyJump(float velocityY)
    {
        return this.velocityY + this.jumpVelocity;
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

        currentDirection = Vector2.SmoothDamp(
            currentDirection,
            targetDirection,
            ref currentDirectionVelocity,
            moveSmoothTime
        );

        //If the CharacterController was touching the ground during the last move, "zero-out" the vertical velocity.
        if (controller.isGrounded)
        {
            this.velocityY = 0.0f;

            //Only allow the player to jump IF they are touching the ground:
            if (this.enableJumping && Input.GetKeyDown(this.jumpKey))
            {
                this.velocityY = ApplyJump(this.velocityY);
            }
        }

        this.velocityY = ApplyGravity(this.velocityY);

        Vector3 velocity = (
            (transform.forward * currentDirection.y * walkSpeed) + //Apply the forwards/backwards velocity.
            (transform.right * currentDirection.x * walkSpeed) + //Apply the left/right velocity
            (Vector3.up * velocityY) //The value of "velocityY" is negative, so Vector3.down would apply gravity in the opposite direction.
            ); 

        controller.Move(velocity * Time.deltaTime);

    }

    private float ApplyGravity(float velocityY)
    {
        return velocityY + this.forceOfGravity * 0.01f;
    }

    private void UpdateMouseLook()
    {
        //Get the position of the cursor:
        Vector2 targetMouseDelta = getMouseDelta();

        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);


        if (this.rotatePlayerBodyWithCamera)
        {
            if (this.enableMouseXAxisControl)
            {
                this.RotatePlayerHorizontally(ref currentMouseDelta);
            }
            if (this.enableMouseYAxisControl)
            {
                Vector3 verticalCameraVector = this.RotateCameraVertically(ref currentMouseDelta);
                this.verticalSwivel.localEulerAngles = 1 * verticalCameraVector;
            }
        }
        else
        {
            if (this.enableMouseXAxisControl)
            {
                Vector3 horizontalCameraVector = this.RotateCameraHorizontally(ref currentMouseDelta);
                this.horizontalSwivel.Rotate(horizontalCameraVector);
            }
            if (this.enableMouseYAxisControl){
                Vector3 verticalCameraVector = this.RotateCameraVertically(ref currentMouseDelta);
                this.verticalSwivel.localEulerAngles = verticalCameraVector;

            }
        }

    }

    private Vector2 getMouseDelta()
    {
        return new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );
    }

    private Vector3 RotateCameraVertically(ref Vector2 currentMouseDelta)
    {
        /*
        Negative degrees pushes the camera UPWARDS, so we want to invert the mouse value so that the camera does not pitch in the 
        opposite direction of the cursor's movement:
        */
        cameraPitch -= currentMouseDelta.y * mouseYCameraSensitivity;

        //Make sure that the user cannot move the camera higher than looking straight up or looking straight down:

        cameraPitch = Mathf.Clamp(cameraPitch, this.minimumVerticalClamp, this.maximumVerticalClamp);

        return Vector3.right * cameraPitch;
    }
    
    private Vector3 RotateCameraHorizontally(ref Vector2 currentMouseDelta)
    {        
        float cameraRotation = currentMouseDelta.x * mouseXCameraSensitivity;
        return Vector3.up * cameraRotation;
    }

    private void RotatePlayerHorizontally (ref Vector2 currentMouseDelta)
    {
        //Rotate horizontally (i.e., AROUND the up axis) using the horizontal mouse delta:
        transform.Rotate(Vector3.up * currentMouseDelta.x * mousePlayerRotationSensitivity);
    }
}
