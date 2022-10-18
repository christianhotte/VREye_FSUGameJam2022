using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSPlayer : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] Camera cam;
    [SerializeField] float mouseSpeedX;
    [SerializeField] float mouseSpeedY;
    [SerializeField] float walkSpeed;
    [SerializeField] float walkLerpRate;
    [SerializeField] float sprintSpeed;
    [SerializeField] float sprintLerpRate;
    [SerializeField] float crouchSpeed;
    [SerializeField] float crouchLerpRate;
    [SerializeField] float jumpHeight;
    [SerializeField] Transform rocketPrefab;
    [SerializeField] LayerMask groundLayers;

    float cameraPitch = 0;
    Vector3 xzMovement;

    Vector3 toMove;
    Vector3 itoMove;

    int jumps;
    bool inControl;
    enum MoveStates
    {
        Walking,
        Sprinting,
        Crouching
    }
    MoveStates moveState = MoveStates.Walking;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }

    private bool GroundCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f))
        {
            return true;
        }
        return false;
    }

    private void Update()
    {
        if (inControl)
        {
            float cspeed = 0, clerprate = 0;
            switch (moveState)
            {
                case MoveStates.Walking:
                    cspeed = walkSpeed;
                    clerprate = walkLerpRate;
                    break;
                case MoveStates.Sprinting:
                    cspeed = sprintSpeed;
                    clerprate = sprintLerpRate;
                    break;
                case MoveStates.Crouching:
                    cspeed = crouchSpeed;
                    clerprate = crouchLerpRate;
                    break;
            }
            Vector3 tempVel = rb.velocity;
            itoMove = Quaternion.Euler(0, transform.eulerAngles.y, 0) * xzMovement * cspeed;
            toMove = Vector3.Lerp(toMove, itoMove, Time.deltaTime * clerprate);
            tempVel.x = toMove.x;
            tempVel.z = toMove.z;
            rb.velocity = tempVel;
        }
        else
        {
            if (GroundCheck())
            {
                inControl = true;
            }
        }
    }

    public void MouseLook(InputAction.CallbackContext ctx)
    {
        Vector2 camera_turn = ctx.ReadValue<Vector2>();
        transform.Rotate(camera_turn.x * Vector3.up * mouseSpeedX);
        cameraPitch -= camera_turn.y * mouseSpeedY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80.0f, 80.0f);
        cam.transform.localEulerAngles = Vector3.right * cameraPitch;
    }

    public void Walk(InputAction.CallbackContext ctx)
    {
        Vector2 movement = ctx.ReadValue<Vector2>();
        xzMovement.x = movement.x;
        xzMovement.z = movement.y;
    }
    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (GroundCheck())
            {
                Vector3 tempVel = rb.velocity;
                tempVel.y = jumpHeight;
                rb.velocity = tempVel;
                jumps = 1;
            }
            else if (jumps > 0)
            {
                jumps -= 1;
                Vector3 tempVel = rb.velocity;
                tempVel.y = jumpHeight-1.0f;
                rb.velocity = tempVel;
            }

        }
        else if (ctx.canceled)
        {
            Vector3 tempVel = rb.velocity;
            if (tempVel.y > 0) tempVel.y *= 0.75f;
            rb.velocity = tempVel;
        }
    }


    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Transform newRocket = Instantiate(rocketPrefab);
        newRocket.position = cam.transform.position + cam.transform.forward + cam.transform.right*0.3f - cam.transform.up*0.2f;
        newRocket.rotation = cam.transform.rotation;
    }

    public void Sprint(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) moveState = MoveStates.Sprinting;
        else if (ctx.canceled) moveState = MoveStates.Walking;
    }

    public void Crouch(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) moveState = MoveStates.Crouching;
        else if (ctx.canceled) moveState = MoveStates.Walking;
    }

    public void SendOutOfControl()
    {
        inControl = false;
    }

}
