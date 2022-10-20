using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS_Clone : MonoBehaviour
{
    public static FPS_Clone inst;

    Rigidbody rb;
    Collider col;
    AudioSource aud;

    [SerializeField] Animator torso;
    Quaternion torsoStartRotation;
    [SerializeField] Animator legs;

    [SerializeField] TrailRenderer shadowTrail;
    [SerializeField] CloneTrailFade cloneTrailFade;
    float shadowTrailITime;
    [SerializeField] Light faceLight;
    float faceLightStartLight;
    float faceLightILight;

    [SerializeField] internal float walkSpeed;
    [SerializeField] internal float walkLerpRate;

    [SerializeField] LayerMask groundLayers;

    bool dead = false;

    internal Vector3 xzMovement;
    Vector3 toMove;
    Vector3 itoMove;

    float customFall = -13.0f;

    int isWalking_hash = Animator.StringToHash("isWalking");

    bool grounded = false;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        aud = GetComponent<AudioSource>();
        inst = this;
        torsoStartRotation = torso.transform.localRotation;
        faceLightStartLight = faceLight.intensity;
        faceLightILight = faceLightStartLight;
    }

    private void Start()
    {
        legs.SetBool(isWalking_hash, true);
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
        Vector3 tempVel = rb.velocity;
        bool groundedLastFrame = grounded;
        grounded = GroundCheck();
        itoMove = Quaternion.Euler(0, transform.eulerAngles.y, 0) * xzMovement * walkSpeed;
        toMove = Vector3.Lerp(toMove, itoMove, Time.fixedDeltaTime * walkLerpRate);
        tempVel.x = toMove.x;
        tempVel.z = toMove.z;
        rb.velocity = tempVel;
        if (grounded)
        {
            customFall = Mathf.Lerp(customFall, 0.0f, Time.fixedDeltaTime);
        }
        else
        {
            customFall = Mathf.Lerp(customFall, -30.0f, Time.fixedDeltaTime * 0.5f);
        }
        tempVel.y = customFall;
        rb.velocity = tempVel;
    }

    private void LateUpdate()
    {
        faceLight.intensity = Mathf.Lerp(faceLight.intensity, faceLightILight, Time.deltaTime * 3.0f);
    }

    private void OnDestroy()
    {
        cloneTrailFade.BreakOff();
    }

}
