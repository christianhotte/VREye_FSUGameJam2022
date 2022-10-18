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
    [SerializeField] float jumpHeight;

    [SerializeField] Transform rocketPrefab;

    CharacterController cc;
    float cameraPitch = 0;

    Vector3 movementDir;
    Vector3 toMove;
    Vector3 iToMove;

    float currentGrav = 10.0f;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        iToMove = ((transform.forward * movementDir.z) + (transform.right * movementDir.x)) * (movementSpeed * Time.deltaTime);
        toMove = Vector3.Lerp(toMove, iToMove, speedLerpRate * Time.deltaTime);
        cc.Move(toMove);

        if (cc.isGrounded)
        {
            currentGrav = 2.0f;
        }
        else
        {
            currentGrav += Time.deltaTime * 30.0f;
            if (currentGrav > 60 ) currentGrav = 60;
        }

        cc.Move(currentGrav * -Vector3.up * Time.deltaTime);
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
        movementDir.x = movement.x;
        movementDir.z = movement.y;
        movementDir = movementDir.normalized;
    }
    public void Jump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (cc.isGrounded)
            {
                currentGrav = -jumpHeight;
            }
        }
        else if (ctx.canceled)
        {
            if (currentGrav < 0)
                currentGrav *= 0.5f;
        }
    }


    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Transform newRocket = Instantiate(rocketPrefab);
        newRocket.position = cam.transform.position + cam.transform.forward + cam.transform.right*0.3f - cam.transform.up*0.2f;
        newRocket.rotation = cam.transform.rotation;
    }

}
