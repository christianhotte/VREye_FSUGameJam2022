using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSPlayer : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float mouseSpeedX;
    [SerializeField] float mouseSpeedY;
    [SerializeField] float movementSpeed;
    [SerializeField] float directionLerpRate;
    [SerializeField] float speedLerpRate;

    [SerializeField] Transform rocketPrefab;

    CharacterController cc;
    float cameraPitch = 0;

    Vector3 movementDir;
    Vector3 movementIDir;
    Vector3 toMove;
    Vector3 iToMove;

    float currentGrav = 10.0f;
    float iGravScalar = 1.0f;
    float gravScalar = 1.0f;

    float coyoteTime = 0;
    float bufferTime = 0;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        movementDir = Vector3.Lerp(movementDir, movementIDir, directionLerpRate * Time.deltaTime);
        iToMove = ((transform.forward * movementDir.z) + (transform.right * movementDir.x)) * (movementSpeed * Time.deltaTime);
        toMove = Vector3.Lerp(toMove, iToMove, speedLerpRate * Time.deltaTime * gravScalar);
        cc.Move(toMove);

        if (cc.isGrounded)
        {
            currentGrav = 2.0f;
        }
        else
        {
            currentGrav += Time.deltaTime * 20.0f * gravScalar;
            if (currentGrav > 60 ) currentGrav = 60;
        }

        cc.Move(currentGrav * -Vector3.up * Time.deltaTime * gravScalar);

        if (cc.isGrounded && bufferTime > 0)
        {
            ActuallyJump();
        }
        if (bufferTime > 0) bufferTime -= Time.deltaTime;

        gravScalar = Mathf.Lerp(gravScalar, iGravScalar, Time.deltaTime * 3.5f);
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
        movementIDir.x = movement.x;
        movementIDir.z = movement.y;
        movementIDir = movementIDir.normalized;
    }
    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            bufferTime = 0.5f;
        }
        else if (ctx.canceled)
        {
            if (currentGrav < 0)
                currentGrav *= 0.5f;
        }
    }

    private void ActuallyJump()
    {
        bufferTime = 0;
        currentGrav = -8.0f;
    }

    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Transform newRocket = Instantiate(rocketPrefab);
        newRocket.position = cam.transform.position + cam.transform.forward + cam.transform.right*0.3f - cam.transform.up*0.2f;
        newRocket.rotation = cam.transform.rotation;
        gravScalar *= 0.1f;
    }

}
