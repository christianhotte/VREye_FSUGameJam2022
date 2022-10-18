using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSPlayer : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField] Camera cam;
    [SerializeField] Transform armsHolder;
    [SerializeField] float mouseSpeedX;
    [SerializeField] float mouseSpeedY;
    [SerializeField] float walkSpeed;
    [SerializeField] float walkLerpRate;
    [SerializeField] float sprintSpeed;
    [SerializeField] float sprintLerpRate;
    [SerializeField] float crouchSpeed;
    [SerializeField] float crouchLerpRate;
    [SerializeField] float jumpHeight;
    [SerializeField] float crouchDistance;
    [SerializeField] float deathDistance;

    [SerializeField] bool looseWeaponSway;
    [SerializeField] float weaponSwayX;
    [SerializeField] float weaponSwayY;
    [SerializeField] float weaponReturn;

    [SerializeField] Transform rocketPrefab;
    [SerializeField] LayerMask groundLayers;

    bool dead = false;

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

    Vector3 camStartPos;
    Vector3 camIPos;

    Vector3 weaponOrigin = Vector3.zero;
    Vector3 iWeaponOrigin = Vector3.zero;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        camStartPos = cam.transform.localPosition;
        camIPos = cam.transform.localPosition;
    }

    bool grounded = false;
    private bool GroundCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f))
        {
            return true;
        }
        return false;
    }

    private void FixedUpdate()
    {
        grounded = GroundCheck();
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
            itoMove = Quaternion.Euler(0, transform.eulerAngles.y, 0) * xzMovement * cspeed;
            toMove = Vector3.Lerp(toMove, itoMove, Time.fixedDeltaTime * clerprate);
            Vector3 tempVel = rb.velocity;
            tempVel.x = toMove.x;
            tempVel.z = toMove.z;
            rb.velocity = tempVel;
            if (!dead)
            {
                if (grounded)
                    iWeaponOrigin.y = -(new Vector2(rb.velocity.x, rb.velocity.z).magnitude) * 0.01f;
                else
                    iWeaponOrigin.y = 0.0f;
            }
        }
        else
        {
            if (!dead)
                iWeaponOrigin.y = 0.4f;
            if (GroundCheck())
            {
                inControl = true;
            }
        }
    }

    private void LateUpdate()
    {
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, camIPos, Time.deltaTime * 5.0f);
        weaponOrigin = Vector3.Lerp(weaponOrigin, iWeaponOrigin, Time.deltaTime * 8.0f);
        armsHolder.localPosition = Vector3.Lerp(armsHolder.localPosition, weaponOrigin, Time.deltaTime * weaponReturn);
        armsHolder.localRotation = Quaternion.Lerp(armsHolder.localRotation, Quaternion.identity, Time.deltaTime * weaponReturn);
    }

    public void MouseLook(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        Vector2 camera_turn = ctx.ReadValue<Vector2>();
        transform.Rotate(camera_turn.x * Vector3.up * mouseSpeedX);
        cameraPitch -= camera_turn.y * mouseSpeedY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80.0f, 80.0f);
        cam.transform.localEulerAngles = Vector3.right * cameraPitch;
        if (looseWeaponSway)
        {
            armsHolder.Translate(armsHolder.right * camera_turn.x * Time.deltaTime * weaponSwayX);
            armsHolder.Translate(armsHolder.up * -camera_turn.y * Time.deltaTime * weaponSwayY);
        }
        else
        {
            armsHolder.localPosition += Vector3.right * camera_turn.x * Time.deltaTime * weaponSwayX;
            armsHolder.localEulerAngles += Vector3.up * camera_turn.x * Time.deltaTime * weaponSwayX * 20.0f;
            armsHolder.localPosition += Vector3.up * camera_turn.y * Time.deltaTime * weaponSwayY;
            armsHolder.localEulerAngles += Vector3.right * camera_turn.y * Time.deltaTime * weaponSwayY * 20.0f;
        }
    }

    public void Walk(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        Vector2 movement = ctx.ReadValue<Vector2>();
        xzMovement.x = movement.x;
        xzMovement.z = movement.y;
        xzMovement = xzMovement.normalized;
    }
    public void Jump(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        if (ctx.performed)
        {
            if (grounded)
            {
                Vector3 tempVel = rb.velocity;
                tempVel.y = jumpHeight;
                rb.velocity = tempVel;
                jumps = 1;
                weaponOrigin += Vector3.up * 0.2f;
            }
            else if (jumps > 0)
            {
                jumps -= 1;
                Vector3 tempVel = rb.velocity;
                tempVel.y = jumpHeight-1.0f;
                rb.velocity = tempVel;
                weaponOrigin += Vector3.up * 0.25f;
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
        if (dead) return;
        if (!ctx.performed) return;
        if (moveState == MoveStates.Sprinting && grounded) return;
        Transform newRocket = Instantiate(rocketPrefab);
        newRocket.position = cam.transform.position + cam.transform.forward + cam.transform.right*0.3f - cam.transform.up*0.3f;
        newRocket.rotation = cam.transform.rotation;
    }

    public void Sprint(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        if (grounded && ctx.performed)
        {
            moveState = MoveStates.Sprinting;
        }
        else if (ctx.canceled) moveState = MoveStates.Walking;
    }

    public void Crouch(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        if (grounded && ctx.performed)
        {
            moveState = MoveStates.Crouching;
            camIPos = camStartPos - Vector3.up * crouchDistance;
        }
        else if (ctx.canceled)
        {
            moveState = MoveStates.Walking;
            camIPos = camStartPos;
        }
    }

    public void SendOutOfControl(Vector2 _xz, float _bounce)
    {
        Vector3 tempVel = rb.velocity;
        tempVel.x = _xz.x;
        tempVel.z = _xz.y;
        tempVel.y = _bounce;
        rb.velocity = tempVel;
        inControl = false;
    }

    public void Die()
    {
        camIPos = camStartPos - Vector3.up * deathDistance;
        dead = true;
        inControl = false;
        iWeaponOrigin = -Vector3.up*2.0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ForceDie(InputAction.CallbackContext ctx)
    {
        /*
        if (!ctx.performed) return;
        Die();
        */
        SendOutOfControl(Vector3.right * 5.0f, 4.0f);
    }

}
