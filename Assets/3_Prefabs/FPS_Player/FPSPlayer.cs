using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FPSPlayer : MonoBehaviour
{
    static FPSPlayer inst;

    Rigidbody rb;
    Collider col;
    AudioSource aud;

    [SerializeField] Animator torso;
    Quaternion torsoStartRotation;
    [SerializeField] Animator legs;
    [SerializeField] SkinnedMeshRenderer bodyMesh;
    [SerializeField] SkinnedMeshRenderer gunMesh;
    [SerializeField] Animator fpsCrossbow;
    [SerializeField] Animator fpsCanvas;
    [SerializeField] Animator cloneCanvas;
    [SerializeField] Animator invisCanvas;
    [SerializeField] Animation canvasBackColor;
    [SerializeField] List<Animator> canvasHearts; 

    [SerializeField] TrailRenderer shadowTrail;
    float shadowTrailITime;
    [SerializeField] Light faceLight;
    float faceLightStartLight;
    float faceLightILight;


    [SerializeField] Camera cam;
    [SerializeField] Transform armsHolder;
    [SerializeField] Transform projectileSpawnPoint;
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
    [SerializeField] float loseControlTime = 0.5f;
    [SerializeField] float squishScale = 0.07f;

    [SerializeField] bool looseWeaponSway;
    [SerializeField] float weaponSwayX;
    [SerializeField] float weaponSwayY;
    [SerializeField] float weaponReturn;

    [SerializeField] Transform rocketPrefab;
    [SerializeField] FPS_Clone clonePrefab;
    [SerializeField] GameObject deathCanvasPrefab;
    [SerializeField] GameObject winCanvasPrefab;
    [SerializeField] LayerMask groundLayers;

    [SerializeField] PhysicMaterial NoFric;
    [SerializeField] PhysicMaterial Fric;

    bool dead = false;

    float cameraPitch = 0;

    Vector3 xzMovement;
    Vector3 toMove;
    Vector3 itoMove;

    float customFall = -13.0f;
    int jumps;

    int isWalking_hash = Animator.StringToHash("isWalking");
    int isShooting_hash = Animator.StringToHash("FiringGun");
    int canvasReload_hash = Animator.StringToHash("FPS_Canvas_Reload");

    bool grounded = false;
    bool bowLoaded = true;

    float cloneCooldown = 0;
    float invisCooldown = 0;

    int hp = 3;
    float controlTimeRemaining = 0;

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
        col = GetComponent<Collider>();
        aud = GetComponent<AudioSource>();
        camStartPos = cam.transform.localPosition;
        camIPos = cam.transform.localPosition;
        inst = this;
        torsoStartRotation = torso.transform.localRotation;
        faceLightStartLight = faceLight.intensity;
        faceLightILight = faceLightStartLight;
    }

    private bool GroundCheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.3f))
        {
            return true;
        }
        return false;
    }

    private void FixedUpdate()
    {
        if (controlTimeRemaining != 0)
        {
            controlTimeRemaining = Mathf.Max(0, controlTimeRemaining - Time.fixedDeltaTime);
        }

        Vector3 tempVel = rb.velocity;
        bool groundedLastFrame = grounded;
        grounded = GroundCheck();
        if (grounded && !groundedLastFrame) weaponOrigin -= Vector3.up * 0.25f;
        if (controlTimeRemaining == 0)
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
                controlTimeRemaining = 0;
            }
        }
        if (grounded)
        {
            customFall = Mathf.Lerp(customFall, 0.0f, Time.fixedDeltaTime);
        }
        else
        {
            customFall = Mathf.Lerp(customFall, -30.0f, Time.fixedDeltaTime*0.5f);
        }
        tempVel.y = customFall;
        rb.velocity = tempVel;
        if (moveState == MoveStates.Crouching)
            shadowTrailITime = rb.velocity.magnitude * 0.4f;
        else
            shadowTrailITime = rb.velocity.magnitude * 0.1f;
        if (grounded && xzMovement.magnitude < 0.3f)
            col.material = Fric;
        else
            col.material = NoFric;
    }

    private void LateUpdate()
    {
        cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, camIPos, Time.deltaTime * 5.0f);
        weaponOrigin = Vector3.Lerp(weaponOrigin, iWeaponOrigin, Time.deltaTime * 8.0f);
        armsHolder.localPosition = Vector3.Lerp(armsHolder.localPosition, weaponOrigin, Time.deltaTime * weaponReturn);
        armsHolder.localRotation = Quaternion.Lerp(armsHolder.localRotation, Quaternion.identity, Time.deltaTime * weaponReturn);
        shadowTrail.time = Mathf.MoveTowards(shadowTrail.time, shadowTrailITime, Time.deltaTime);
        faceLight.intensity = Mathf.Lerp(faceLight.intensity, faceLightILight, Time.deltaTime*3.0f);
        if (cloneCooldown > 0)
        {
            cloneCooldown -= Time.deltaTime;
            if (cloneCooldown <= 0)
            {
                cloneCanvas.Play("Clone_Canvas_Regen");
            }
        }
        if (invisCooldown > 0)
        {
            invisCooldown -= Time.deltaTime;
            if (invisCooldown <= 0)
            {
                invisCanvas.Play("Invis_Canvas_Regen");
            }
        }
        fpsCrossbow.transform.localScale = Vector3.Lerp(fpsCrossbow.transform.localScale, Vector3.one, Time.deltaTime * 2.0f);
    }

    public void MouseLook(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        Vector2 camera_turn = ctx.ReadValue<Vector2>();
        transform.Rotate(camera_turn.x * Vector3.up * mouseSpeedX);
        cameraPitch -= camera_turn.y * mouseSpeedY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80.0f, 80.0f);
        cam.transform.localEulerAngles = Vector3.right * cameraPitch;
        torso.transform.localRotation = torsoStartRotation * Quaternion.AngleAxis(-cameraPitch, Vector3.right); //-Vector3.right * (cameraPitch - 90);
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
        if (FPS_Clone.inst != null)
        {
            if (xzMovement.magnitude > 0.1f)
                FPS_Clone.inst.transform.Rotate(-camera_turn.x * Vector3.up * mouseSpeedX);
            else
                FPS_Clone.inst.transform.Rotate(-camera_turn.x * Vector3.up * mouseSpeedX * 4.0f);
        }
    }

    public void Walk(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        Vector2 movement = ctx.ReadValue<Vector2>();
        xzMovement.x = movement.x;
        xzMovement.z = movement.y;
        legs.SetBool(isWalking_hash, xzMovement.magnitude > 0.05f);
        xzMovement = xzMovement.normalized;
    }
    public void Jump(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        if (ctx.performed)
        {
            if (grounded)
            {
                customFall = jumpHeight;
                jumps = 1;
                weaponOrigin += Vector3.up * 0.2f;
            }
            else if (jumps > 0)
            {
                jumps -= 1;
                customFall = jumpHeight-1.0f;
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

    public void ReloadBow()
    {
        fpsCanvas.Play(canvasReload_hash, -1, 0.0f);
        bowLoaded = true;
    }

    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        if (!ctx.performed) return;
        if (!bowLoaded) return;
        bowLoaded = false;
        Transform newRocket = Instantiate(rocketPrefab);
        newRocket.position = projectileSpawnPoint.position;
        newRocket.rotation = cam.transform.rotation;
        weaponOrigin -= Vector3.forward * 0.6f;
        aud.Play();
        fpsCrossbow.Play(isShooting_hash,-1,0);
        fpsCanvas.Play(isShooting_hash, -1, 0.0f);
    }

    private void MovementStateUpdate()
    {
        switch (moveState)
        {
            case MoveStates.Crouching:
                camIPos = camStartPos - Vector3.up * crouchDistance;
                faceLightILight = 0.0f;
                break;
            default:
                camIPos = camStartPos;
                faceLightILight = faceLightStartLight;
                break;
        }
    }    

    public void Sprint(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        if (grounded && ctx.performed)
        {
            moveState = MoveStates.Sprinting;
            MovementStateUpdate();
        }
        else if (ctx.canceled)
        {
            moveState = MoveStates.Walking;
            MovementStateUpdate();
        }
    }

    public void Crouch(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        if (grounded && ctx.performed)
        {
            moveState = MoveStates.Crouching;
            MovementStateUpdate();
        }
        else if (ctx.canceled)
        {
            moveState = MoveStates.Walking;
            MovementStateUpdate();
        }
    }

    public void Clone(InputAction.CallbackContext ctx)
    {
        if (dead) return;
        if (cloneCooldown > 0) return;
        if (!grounded || !ctx.performed) return;
        FPS_Clone newclone = Instantiate(clonePrefab);
        newclone.transform.position = transform.position;
        newclone.transform.rotation = transform.rotation;
        newclone.walkLerpRate = sprintLerpRate;
        newclone.walkSpeed = sprintSpeed;
        newclone.xzMovement = Vector3.forward;
        cloneCooldown = 10.0f;
        cloneCanvas.Play("Clone_Canvas_Use");
    }

    public void SendOutOfControl(Vector2 _xz, float _bounce)
    {
        Vector3 tempVel = rb.velocity;
        tempVel.x = _xz.x;
        tempVel.z = _xz.y;
        tempVel.y = _bounce;
        rb.velocity = tempVel;
        toMove = tempVel;
        controlTimeRemaining = loseControlTime;
    }

    private void DeathEssentials()
    {
        xzMovement = Vector3.zero;
        dead = true;
        controlTimeRemaining = 0;
        iWeaponOrigin = -Vector3.up * 2.0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        legs.gameObject.SetActive(false);
        fpsCanvas.gameObject.SetActive(false);
        Instantiate(deathCanvasPrefab);
        rb.velocity = Vector3.zero;
    }

    public void Die()
    {
        if (dead) return;
        DeathEssentials();
        camIPos = camStartPos - Vector3.up * deathDistance;
    }

    public void Squish()
    {
        if (dead) return;
        DeathEssentials();
        camIPos = camStartPos - Vector3.up * 1.2f;
        Vector3 newPos = legs.transform.parent.position;
        newPos.y -= squishScale;
        legs.transform.parent.position = newPos;
        transform.localScale = new Vector3(1, squishScale, 1);
    }

    public void TakeDamage()
    {
        hp -= 1;
        if (hp > 0)
        {
            fpsCrossbow.transform.localScale += (Vector3.up + Vector3.right) * 0.5f;
            canvasBackColor.Stop();
            canvasBackColor.Play("BackColor_Flash", PlayMode.StopAll);
            canvasHearts[canvasHearts.Count - 1].Play("FPS_Heart_Fade", -1, 0.0f);
            canvasHearts.Remove(canvasHearts[canvasHearts.Count - 1]);
        }
        else
        {
            Die();
        }
    }

    public void Win()
    {
        if (dead) return;
        xzMovement = Vector3.zero;
        dead = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        fpsCanvas.gameObject.SetActive(false);
        Instantiate(winCanvasPrefab);
    }

    public static void StaticWin()
    {
        if (inst != null) inst.Win();
    }

    public void ForceDie(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TakeDamage();
    }

    public static void FPSShake(float intensity, int times, float curve, float lag)
    {
        inst.StartCoroutine(inst.Shake(intensity, times, curve, lag));
    }

    IEnumerator Shake(float intensity, int times, float curve, float lag)
    {
        Vector3 camStartPos = cam.transform.localPosition;
        Vector3 armsStartPos = armsHolder.localPosition;
        for (int i = 0; i < times; i++)
        {
            cam.transform.localPosition += camStartPos + new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity)
            );
            armsHolder.localPosition += armsStartPos + new Vector3(
                Random.Range(-intensity/2.0f, intensity/2.0f),
                Random.Range(-intensity/2.0f, intensity/2.0f),
                Random.Range(-intensity/2.0f, intensity/2.0f)
            );
            yield return new WaitForSeconds(lag);
            intensity *= curve;
        }
    }

    void HideToggleAll()
    {
        gunMesh.enabled = !gunMesh.enabled;
        bodyMesh.enabled = !bodyMesh.enabled;
        shadowTrail.enabled = !shadowTrail.enabled;
        faceLight.enabled = !faceLight.enabled;
    }
    void HideSetAll(bool _set)
    {
        gunMesh.enabled = _set;
        bodyMesh.enabled = _set;
        shadowTrail.enabled = _set;
        faceLight.enabled = _set;
    }
    public void DoInvis(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (invisCooldown > 0) return;
        invisCooldown = 15.0f;
        invisCanvas.Play("Invis_Canvas_Use");
        StartCoroutine(Invis());
    }
    IEnumerator Invis()
    {
        HideSetAll(true);
        for (float i = 2.0f; i < 10.0f; i++)
        {
            yield return new WaitForSeconds(0.2f/i);
            HideToggleAll();
        }
        HideSetAll(false);
        yield return new WaitForSeconds(4.0f);
        for (float i = 2.0f; i < 10.0f; i++)
        {
            yield return new WaitForSeconds(0.2f/i);
            HideToggleAll();
        }
        HideSetAll(true);
    } 

    public void Escape(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        Application.Quit();
    }

}
