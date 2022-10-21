using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalEnums;
using RootMotion.FinalIK;
using UnityEngine.InputSystem;

public class VRHandController : MonoBehaviour
{
    //Objects & Components:
    /// <summary>
    /// Controller transform which this hand is following.
    /// </summary>
    public Transform controllerTarget;
    public GameObject gibPrefab;
    internal VRHandController otherHand; //Reference to controller script for opposite hand
    private Transform obstructedTarget;  //Target position which does not collide with objects in scene
    private Transform[] fingerTargets;   //Array of all targets used to position finger IK rigs
    private Transform[] fingerRoots;     //Root positions of hand fingers (should eventually be animated)
    private InputActionMap inputMap;     //The input map for this hand
    private FABRIK[] ikSolvers;          //Array of all IK solvers in hand
    private AudioSource audioSource;     //Audiosource component for this hand

    //Settings:
    [Header("Hand Control:")]
    [SerializeField, Tooltip("Moves where on controller hands stick to")]                                                                      private Vector3 followOffset;
    [Min(0), SerializeField, Tooltip("Maximum speed at which hand can move")]                                                                  private float maxSpeed;
    [Min(0), SerializeField, Tooltip("Rate at which hand seeks target position")]                                                              private float linearFollowSpeed;
    [Min(0), SerializeField, Tooltip("Rate at which hand seeks target rotation")]                                                              private float angularFollowSpeed;
    [SerializeField, Tooltip("Raise to make dropping the hand quicker")]                                                                       private float dropSpeedMultiplier;
    [SerializeField, Tooltip("Lower to make lifting the hand slower")]                                                                         private float raiseSpeedMultiplier;
    [Range(0, 90),SerializeField, Tooltip("Outer angle which determines where drop multiplier begins to factor in")]                           private float dropMultiplierAngle;
    [Range(0, 90),SerializeField, Tooltip("Outer angle which determines where raise multiplier begins to factor in")]                          private float raiseMultiplierAngle;
    [Min(0), SerializeField, Tooltip("Increasing this increases the amount by which hands can push player origin up, even when not grabbing")] private float neutralOriginPushFactor;
    [Header("Mobility Settings:")]
    [Min(0), SerializeField, Tooltip("Smoothness of player grip movement")]                               private float gripMoveLerpRate;
    [Min(0), SerializeField, Tooltip("Maximum speed at which player can gripMove themselves")]            private float maxGripMoveSpeed;
    [Min(0), SerializeField, Tooltip("Rate at which player origin velocity slows after releasing grip")]  private float smoothGripStopRate;
    [Min(0), SerializeField, Tooltip("Maximum distance from center of world player can move")]            private float outerBoundRadius;
    [Header("Physics Settings:")]
    [Min(0), SerializeField, Tooltip("Size of main hand collider")]                                                 private float palmCollisionRadius;
    [SerializeField, Tooltip("Physics layers which fingers on hand are able to collide with")]                      private LayerMask obstructionLayers;
    [Min(1), SerializeField, Tooltip("Maximum number of obstructions hand and fingers can collide with per frame")] private int maxObstacleCollisions = 1;
    [Min(0), SerializeField, Tooltip("Separation distance from palm used to check for grabbable surfaces")]         private float palmSurfaceCheckDistance;
    [Min(0), SerializeField, Tooltip("Radius around palm point used to check for grabbable surfaces")]              private float palmSurfaceCheckRadius;
    [Header("Input Settings:")]
    [Range(0, 1), SerializeField, Tooltip("Grip threshold for turning hand into fist")] private float fistGripThreshold;
    [Range(0, 1), SerializeField, Tooltip("Grip threshold for doing grabmove")]         private float surfaceGripThreshold;
    [Header("Finger Behavior:")]
    [Min(0), SerializeField, Tooltip("Radius of fingertip colliders")]                                                    private float fingerTipRadius;
    [Min(0), SerializeField, Tooltip("Maximum speed at which individual fingers can adjust their positions")]             private float maxFingerMoveSpeed;
    [Min(0), SerializeField, Tooltip("Use to make fingers lerp to obstructed positions, instead of just snapping there")] private float fingerObstructLerpRate;
    [Min(0), SerializeField, Tooltip("Ease by which fingers are affected by changes in velocity")]                        private float fingerVelocityDragFactor;
    [Min(0), SerializeField, Tooltip("Maximum distance by which fingers can be dragged back by velocity")]                private float maxFingerVelocityDrag;
    [Min(0), SerializeField, Tooltip("Maximum depth inside colliders which finger targets can penetrate")]                private float maxProjectionDepth;
    [Min(0), SerializeField, Tooltip("Base amount of projection added when grabbing")]                                    private float baseGrabProjection;
    [Range(0, 1), SerializeField, Tooltip("Base amount of projection added when pushing flat hand against surface")]      private float baseNeutralProjection;
    [Header("Haptics:")]
    [SerializeField, Tooltip("")] private Vector2 minSlamHaptics;
    [SerializeField, Tooltip("")] private Vector2 maxSlamHaptics;
    [SerializeField, Tooltip("")] private Vector2 grabHaptics;
    [SerializeField, Tooltip("")] private Vector2 hurtHaptics;
    [SerializeField, Tooltip("")] private Vector2 arrowCatchHaptics;
    [Header("Sounds:")]
    [SerializeField, Tooltip("")] private AudioClip muffledStompSound;
    [SerializeField, Tooltip("")] private AudioClip stompSound;
    [SerializeField, Tooltip("")] private AudioClip grabSound;
    [SerializeField, Tooltip("")] private AudioClip buildingGrabSound;
    [Header("Effect Controls:")]
    [SerializeField, Tooltip("Min and max impact deceleration for stomp effect to register")] private Vector2 stompEffectImpactRange;
    [SerializeField, Tooltip("Min and max impact deceleration for stomp effect to register")] private Vector2 earthQuakeRadius;
    [SerializeField, Tooltip("Min and max impact deceleration for stomp effect to register")] private Vector2 earthQuakeIntensityRange;

    //Runtime Variables:
    /// <summary>
    /// Which side this hand is on.
    /// </summary>
    internal HandType side = HandType.None;                     //Initialize at "None" to indicate hand has not yet been associated with player controller
    private List<Collider> palmSurfaces = new List<Collider>(); //Surfaces player palm is currently touching
    internal Building grabbedBuilding;
    private Vector3 prevVelocity;
    private Vector3 lastOriginVelocity;
    private bool prevObstructed;
    private float shookPlayerTime;

    //Input Variables:
    private float gripValue;                    //How closed this hand currently is
    internal GripType gripType = GripType.Open; //What grip form the hand is currently in
    private Vector3 surfaceGripTarget;          //Target which moves to position gripped by hand
    private Vector3 prevControllerTarget;

    //RUNTIME METHODS:
    private void Awake()
    {
        //Set up hand targets:
        obstructedTarget = new GameObject().transform;                                                    //Instantiate a new transform object
        obstructedTarget.name = "ObstructedControllerPos";                                                //Give target a descriptive name
        if (VRPlayerController.main != null) obstructedTarget.parent = VRPlayerController.main.transform; //Child obstructedTarget marker to VR player if possible

        //Set up finger IK:
        List<Transform> newFingerTargets = new List<Transform>();   //Initialize a list within which to store finger targets
        List<Transform> newFingerRoots = new List<Transform>();     //Initialize a list within which to store finger roots
        ikSolvers = GetComponentsInChildren<FABRIK>(); //Get a list of each FABRIK chain in hand
        foreach (FABRIK fabrik in ikSolvers) //Iterate through each finger IK solver in hand
        {
            //Set up new target:
            Transform newTarget = new GameObject().transform;            //Instantiate new target object
            newFingerTargets.Add(newTarget);                             //Add new target to list
            newTarget.name = "F" + newFingerTargets.Count + "_IKTarget"; //Name finger target

            newTarget.parent = transform;                                    //Child target to hand
            newTarget.position = fabrik.solver.bones[^1].transform.position; //Align target position with fingertip
            fabrik.solver.target = newTarget;                                //Set newly-spawned transform as target of this IK chain

            //Set up root position:
            Transform newRoot = new GameObject().transform;          //Instantiate new root object
            newFingerRoots.Add(newRoot);                             //Add new root to list
            newRoot.name = "F" + newFingerTargets.Count + "_IKRoot"; //Name root

            newRoot.parent = transform;            //TEMP child root directly to hand
            newRoot.position = newTarget.position; //TEMP move root to position of fingertip
        }
        fingerTargets = newFingerTargets.ToArray(); //Store finger target list
        fingerRoots = newFingerRoots.ToArray();     //Store finger root list

        //Get objects & components:
        audioSource = GetComponent<AudioSource>(); //Get audio source
    }
    private void Start()
    {
        //Initialize:
        if (controllerTarget != null)
        {
            obstructedTarget.position = controllerTarget.position; //Set target reference position if possible
            prevControllerTarget = controllerTarget.position;
        }
            
        if (side == HandType.Right)
        {
            followOffset.x *= -1;
        }
    }
    private void OnDisable()
    {
        if (inputMap != null) inputMap.actionTriggered -= OnInput;
    }
    private void Update()
    {
        if (shookPlayerTime > 0) shookPlayerTime = Mathf.Max(0, shookPlayerTime - Time.deltaTime);

        //Perform positional update:
        if (controllerTarget != null && obstructedTarget != null) //Both targeting references need to be present for system to function
        {
            //Initialize:
            float scaleMultiplier = 1; if (VRPlayerController.main != null) scaleMultiplier = VRPlayerController.main.transform.localScale.x;  //Get multiplier for maintaining properties despite changes in scaling
            Vector3 prevPosition = transform.position;                                                                                         //Get position before all checks
            Vector3 offsetTargetPos = controllerTarget.position + controllerTarget.TransformVector(followOffset);                              //Get actual target position to seek (ALWAYS home toward this, not controllerTarget)
            float sideMult = 1; if (side == HandType.Right) sideMult = -1;                                                                     //Initialize a multiplier for flipping direction depending on hand side
            bool pullingBuilding = false; if (grabbedBuilding != null && otherHand.grabbedBuilding == grabbedBuilding) pullingBuilding = true; //Indicate whether or not player is pulling on a building

            //Special grip behaviors:
            switch (gripType)
            {
                case GripType.GrabLocked: //Player is grabbing something and is locked into the world
                    if (!pullingBuilding) //Player can only move when not pulling on a building
                    {
                        //Move player origin:
                        Vector3 currentOriginPos = VRPlayerController.main.origin.position; //Get quick reference for current position of origin
                        Vector3 targetOriginPos = currentOriginPos;                         //Initialize positional target value at position of player
                        targetOriginPos += surfaceGripTarget - offsetTargetPos;             //Modify position based on hand movement

                        targetOriginPos = Vector3.Lerp(currentOriginPos, targetOriginPos, gripMoveLerpRate * Time.deltaTime); //Lerp toward target position
                        float adjMaxSpeed = maxGripMoveSpeed * scaleMultiplier * Time.deltaTime;                              //Get effective max speed
                        if (Vector3.Distance(targetOriginPos, currentOriginPos) > adjMaxSpeed) //Player is moving too fast
                        {
                            targetOriginPos = Vector3.MoveTowards(currentOriginPos, targetOriginPos, adjMaxSpeed); //Move origin at max speed toward target
                        }

                        //Clamp bounds:
                        Vector2 flatOriginPosition = new Vector2(targetOriginPos.x, targetOriginPos.z); //Player is trying to move outside boundary
                        if (Vector2.Distance(flatOriginPosition, Vector2.zero) > outerBoundRadius) //Player is trying to move out of bounds
                        {
                            flatOriginPosition = flatOriginPosition.normalized * outerBoundRadius;                        //Move player to edge of boundary
                            targetOriginPos = new Vector3(flatOriginPosition.x, targetOriginPos.y, flatOriginPosition.y); //Commit movement
                        }

                        //Cleanup
                        lastOriginVelocity = targetOriginPos - currentOriginPos;   //Record velocity
                        VRPlayerController.main.origin.position = targetOriginPos; //Apply positional change
                    }
                    break;
                case GripType.Fist: //Player's hand is balled in a fist
                    break;
                case GripType.Slap: //Player's hand is open and is traveling at slapping speed
                    break;
                case GripType.Open: //Player's hand is open
                    //Palm state check:
                    Vector3 palmCastPos = transform.position + (palmSurfaceCheckDistance * scaleMultiplier * sideMult * transform.right);               //Get position palm is casting from relative to hand pivot
                    palmSurfaces = new List<Collider>(Physics.OverlapSphere(palmCastPos, palmSurfaceCheckRadius * scaleMultiplier, obstructionLayers)); //Get colliders player is currently touching
                    break;
                default: break;
            }

            //Unrestrained motion:
            if (gripType != GripType.GrabLocked) //Only rotate if not locked to a surface
            {
                //Find rotation target:
                Quaternion rotTarget = controllerTarget.rotation; //Get default rotation target from controller
                if (side == HandType.Right) //Flip rotation for right hand
                {
                    rotTarget = Quaternion.AngleAxis(180, Vector3.right) * rotTarget;
                    Vector3 rotEuler = rotTarget.eulerAngles;
                    rotEuler.y *= -1; rotEuler.z *= -1;
                    rotTarget = Quaternion.Euler(rotEuler);
                }

                //Rotate system:
                Quaternion newRotation = Quaternion.Slerp(transform.rotation, rotTarget, angularFollowSpeed * Time.deltaTime); //Get new rotation which homes toward rotation target
                List<Vector3> prevFingerPositions = new List<Vector3>();                                                       //Initialize list to store finger positions before rotation
                foreach (Transform finger in fingerTargets) //Uterate through position of each finger
                {
                    prevFingerPositions.Add(finger.position); //Record pre-rotation position
                }
                transform.rotation = newRotation; //Set new rotation
                for (int i = 0; i < fingerTargets.Length; i++) //Iterate through fingerTargets array
                {
                    //Process positions:
                    Vector3 newFingerPos = fingerTargets[i].position;
                    newFingerPos += Vector3.ClampMagnitude(-(newFingerPos - prevFingerPositions[i]) * fingerVelocityDragFactor, maxFingerVelocityDrag * scaleMultiplier); //Add velocity drag effect to finger
                    newFingerPos = GetObstructedPosition(prevFingerPositions[i], newFingerPos, fingerTipRadius * scaleMultiplier); //Keep each finger individually obstructed (scrubbing out velocity movement)

                    //Apply:
                    fingerTargets[i].position = newFingerPos; //Set absolute target position
                }

                //Extra origin motion:
                if (lastOriginVelocity != Vector3.zero) //Origin has some velocity remaining from movement
                {
                    lastOriginVelocity = Vector3.Lerp(lastOriginVelocity, Vector3.zero, smoothGripStopRate * Time.deltaTime);
                    
                    Vector3 targetOriginPos = VRPlayerController.main.origin.position + lastOriginVelocity;
                    Vector2 flatOriginPosition = new Vector2(targetOriginPos.x, targetOriginPos.z); //Player is trying to move outside boundary
                    if (Vector2.Distance(flatOriginPosition, Vector2.zero) > outerBoundRadius) //Player is trying to move out of bounds
                    {
                        flatOriginPosition = flatOriginPosition.normalized * outerBoundRadius;                        //Move player to edge of boundary
                        targetOriginPos = new Vector3(flatOriginPosition.x, targetOriginPos.y, flatOriginPosition.y); //Commit movement
                    }

                    //Cleanup:
                    VRPlayerController.main.origin.position = targetOriginPos;
                    if (Vector3.Distance(lastOriginVelocity, Vector3.zero) < 0.001f) lastOriginVelocity = Vector3.zero;
                }
            }
            else //Player is grabMoving
            {
                offsetTargetPos = surfaceGripTarget; //Freeze hands while grabmoving
            }

            //Modify position:
            Vector3 newPosition = transform.position; //Initialize movement change container
            if (transform.position != offsetTargetPos) //Hand has moved
            {
                //Get multiplier:
                float multiplier = 1;
                Vector3 moveDir = (offsetTargetPos - transform.position).normalized; //Get direction of movement
                float multAngle = Vector3.Angle(moveDir, Vector3.down);
                if (multAngle < dropMultiplierAngle) //Hand is moving at least slightly downward
                {
                    float angleValue = 1 - (multAngle / dropMultiplierAngle);
                    multiplier = Mathf.Lerp(1, dropSpeedMultiplier, angleValue);
                }
                else
                {
                    multAngle = Vector3.Angle(moveDir, Vector3.up);
                    if (multAngle < raiseMultiplierAngle)
                    {
                        float angleValue = 1 - (multAngle / raiseMultiplierAngle);
                        multiplier = Mathf.Lerp(1, raiseSpeedMultiplier, angleValue);
                    }
                }

                //Modify position:
                newPosition = Vector3.Lerp(transform.position, offsetTargetPos, linearFollowSpeed * multiplier * Time.deltaTime);
                float effectiveMaxSpeed = maxSpeed * scaleMultiplier * multiplier * Time.deltaTime;
                if (Vector3.Distance(transform.position, newPosition) > effectiveMaxSpeed)
                {
                    newPosition = Vector3.MoveTowards(transform.position, newPosition, effectiveMaxSpeed);
                }
            }

            //Obstruct movement:
            Vector3 unobstructedPosition = newPosition;                                         //Save position before obstruction
            float scaledRadius = palmCollisionRadius * scaleMultiplier;                         //Get radius of palm scaled to player size
            newPosition = GetObstructedPosition(transform.position, newPosition, scaledRadius); //Obstruct position
            Vector3 currentVelocity = (newPosition - transform.position) / Time.deltaTime;      //Get velocity this frame (in units per second)
            
            
            //Project & obstruct fingers:
            obstructedTarget.position = GetObstructedPosition(obstructedTarget.position, offsetTargetPos, scaledRadius);                                //Update position of obstructed target
            Vector3 projectionDepth = offsetTargetPos - obstructedTarget.position;                                                                      //Get amount by which fingers should be projected out based on obstruction
            Vector3 projectionDepthMod = Vector3.ClampMagnitude(projectionDepth, maxProjectionDepth);                                                   //Clamp projectionDepth magnitude (also make into separate variable
            if (gripType == GripType.GrabLocked || gripType == GripType.GrabFree) projectionDepthMod = transform.right * baseGrabProjection * sideMult; //Use standard grab projection depth
            else { projectionDepthMod *= Mathf.Clamp01(baseNeutralProjection + (gripValue / surfaceGripThreshold)); }                                   //Use projection depth based on grip amount

            Vector3 flatCurrentVelocity = Vector3.ProjectOnPlane(currentVelocity, transform.forward); //Get modified current velocity for finger drag
            scaledRadius = fingerTipRadius * scaleMultiplier;                                         //Update scaled radius so it is useable for fingertips
            for (int i = 0; i < fingerTargets.Length; i++) //Iterate through fingerTargets array
            {
                //Initialization:
                Vector3 fingerIdealPos = fingerRoots[i].position; //Get ideal target relative to finger root

                //Modifiers:
                if (unobstructedPosition != newPosition || gripType == GripType.GrabLocked || gripType == GripType.GrabFree) fingerIdealPos += projectionDepthMod; //Add obstruction projection effect to finger
                fingerIdealPos += Vector3.ClampMagnitude(-flatCurrentVelocity * fingerVelocityDragFactor, maxFingerVelocityDrag * scaleMultiplier);                //Add velocity drag effect to finger

                Vector3 targetFingerPos = Vector3.MoveTowards(fingerTargets[i].position, fingerIdealPos, maxFingerMoveSpeed * scaleMultiplier * Time.deltaTime); //Move fingers toward target relative to root
                targetFingerPos = GetObstructedPosition(fingerTargets[i].localPosition + prevPosition, targetFingerPos, scaledRadius);                           //Filter designated target position through obstructions

                //Apply position
                fingerTargets[i].position = targetFingerPos; //Set absolute finger position
            }

            //Force effects:
            if (projectionDepth.y < 0 && gripType == GripType.Open)
            {
                Vector3 currentOriginPos = VRPlayerController.main.origin.position; //Get quick reference for current position of origin
                Vector3 targetOriginPos = currentOriginPos;                         //Initialize positional target value at position of player
                targetOriginPos += projectionDepth.y * neutralOriginPushFactor * Vector3.down; //Modify position based on hand projection

                targetOriginPos = Vector3.Lerp(currentOriginPos, targetOriginPos, gripMoveLerpRate * Time.deltaTime); //Lerp toward target position
                float adjMaxSpeed = maxGripMoveSpeed * scaleMultiplier * Time.deltaTime;                              //Get effective max speed
                if (Vector3.Distance(targetOriginPos, currentOriginPos) > adjMaxSpeed) //Player is moving too fast
                {
                    targetOriginPos = Vector3.MoveTowards(currentOriginPos, targetOriginPos, adjMaxSpeed); //Move origin at max speed toward target
                }

                //Clamp bounds:
                Vector2 flatOriginPosition = new Vector2(targetOriginPos.x, targetOriginPos.z); //Player is trying to move outside boundary
                if (Vector2.Distance(flatOriginPosition, Vector2.zero) > outerBoundRadius) //Player is trying to move out of bounds
                {
                    flatOriginPosition = flatOriginPosition.normalized * outerBoundRadius;                        //Move player to edge of boundary
                    targetOriginPos = new Vector3(flatOriginPosition.x, targetOriginPos.y, flatOriginPosition.y); //Commit movement
                }

                //Cleanup
                lastOriginVelocity += targetOriginPos - currentOriginPos;  //Record velocity
                VRPlayerController.main.origin.position = targetOriginPos; //Apply positional change
            }
            if (gripType == GripType.GrabLocked && pullingBuilding) //Both hands are grabbing the same building
            {
                grabbedBuilding.Pull((controllerTarget.position - prevControllerTarget) * Time.deltaTime);
            }

            //Commit hand movement:
            transform.position = newPosition; //Set new position

            //Effects:
            if (unobstructedPosition != newPosition && gripType != GripType.GrabLocked && gripType != GripType.GrabFree) //Hand was obstructed this frame
            {
                if (!prevObstructed) //Hand has just become obstructed
                {
                    float strikeForce = prevVelocity.magnitude;
                    strikeForce = Mathf.InverseLerp(stompEffectImpactRange.x, stompEffectImpactRange.y, strikeForce);
                    VRPlayerController.SendHapticImpulse(side, Vector2.Lerp(minSlamHaptics, maxSlamHaptics, strikeForce));
                    audioSource.PlayOneShot(stompSound, strikeForce);

                    //float playerDistance = Vector3.Distance(transform.position, FPSPlayer.inst.transform.position);
                    //playerDistance = Mathf.InverseLerp(earthQuakeRadius.x, earthQuakeRadius.y, playerDistance);
                    //FPSPlayer.FPSShake(Mathf.Lerp(earthQuakeIntensityRange.x, earthQuakeIntensityRange.y, playerDistance), 10, 0.15f, Mathf.Lerp(earthQuakeTimeRange.x, earthQuakeTimeRange.y, playerDistance));
                    //if (shookPlayerTime == 0 && FPSPlayer.inst.grounded) { FPSPlayer.FPSShake(0.03f, 10, 0.5f, 0.05f); shookPlayerTime = 0.7f; }
                }

                //Cleanup:
                prevObstructed = true;
            }
            else //Hand was not obstructed this frame
            {

                //Cleanup:
                prevObstructed = false;
            }
            prevVelocity = currentVelocity; //Save current velocity
            prevControllerTarget = controllerTarget.position;
        }
    }

    //INPUT METHODS:
    public void OnInput(InputAction.CallbackContext context)
    {
        //Input determination:
        if (context.action.name == "Grip") OnGripInput(context); //Pass grip input
    }
    private void OnGripInput(InputAction.CallbackContext context)
    {
        //Initialization:
        float prevGrip = gripValue;             //Temporarily store last grip value
        gripValue = context.ReadValue<float>(); //Record grip value

        //State update:
        switch (gripType)
        {
            case GripType.Fist:
                if (gripValue < fistGripThreshold) //Player is no longer gripping fist
                {
                    gripType = GripType.Open; //Open fist
                }
                break;
            case GripType.GrabLocked:
                if (gripValue < surfaceGripThreshold) //Player is no longer gripping surface
                {
                    OpenHand();
                }
                break;
            case GripType.GrabFree:
                if (gripValue < surfaceGripThreshold) //Player is no longer gripping object
                {

                }
                break;
            case GripType.Slap:
                break;
            case GripType.Open:
                if (palmSurfaces.Count > 0 && prevGrip < surfaceGripThreshold && gripValue >= surfaceGripThreshold) //Player is grabbing a surface
                {
                    //Initialize grip:
                    gripType = GripType.GrabLocked;         //Indicate that player is gripping surface
                    surfaceGripTarget = transform.position; //Set position of surface target

                    //Check for building:
                    foreach (Collider collider in palmSurfaces) //Iterate through each collider palm is touching
                    {
                        if (collider.transform.parent.TryGetComponent(out Building building)) //Check to see if collider is a building
                        {
                            building.Grab(this);        //Indicate to building that it has been grabbed
                            grabbedBuilding = building; //Save reference to grabbed building
                            break;                      //Ignore other colliders
                        }
                    }
                    if (grabbedBuilding != null && otherHand.grabbedBuilding == grabbedBuilding) //Both hands are grabbing the same building
                    {

                    }
                    else if (otherHand.gripType == GripType.GrabLocked) //Grab exclusivity system
                    {
                        otherHand.OpenHand();
                    }

                    //Effects:
                    VRPlayerController.SendHapticImpulse(side, grabHaptics);                 //Play grab haptics
                    if (grabbedBuilding != null) audioSource.PlayOneShot(buildingGrabSound); //Play building grab sound if a building was grabbed
                    else audioSource.PlayOneShot(grabSound);                                 //Play normal grab sound
                }
                else if (gripValue >= fistGripThreshold) //Player is making a fist
                {
                    gripType = GripType.Fist; //Indicate that player is clenching fist

                }
                break;
        }
    }
    /// <summary>
    /// Forces hand open.
    /// </summary>
    public void OpenHand()
    {
        //Building check:
        if (grabbedBuilding != null)
        {
            grabbedBuilding.Release(this); //Release held building
            grabbedBuilding = null;        //Indicate hand is no longer holding building
        }

        //Cleanup:
        gripType = GripType.Open; //Open hand
    }

    //FUNCTIONALITY METHODS:

    //OPERATION METHODS:
    public void SetTarget(Transform newTarget)
    {
        controllerTarget = newTarget;
        //obstructedTarget.position = newTarget.position;
    }
    public void SetupInput(InputActionMap map)
    {
        if (inputMap != null) inputMap.actionTriggered -= OnInput;
        inputMap = map;
        map.actionTriggered += OnInput;
    }
    public void Gib()
    {
        Transform gibs = Instantiate(gibPrefab).transform;
        gibs.parent = transform.parent;
        gibs.position = transform.position;
        gibs.rotation = transform.rotation;
        gibs.localScale = transform.localScale;
        Destroy(gameObject);
    }

    //UTILITY METHODS:
    /// <summary>
    /// Returns position closest to end of given ray but accounting for obstructions.
    /// </summary>
    private Vector3 GetObstructedPosition(Vector3 origin, Vector3 target, float radius)
    {
        if (origin == target) return target; //Ignore if object is not being moved
        Vector3 newPos = target; //Get container for return value
        for (int i = 0; i < maxObstacleCollisions; i++) //Repeat collision check up to a set number of times
        {
            Ray projectionRay = new Ray(origin, newPos - origin); //Create a ray between target position and origin
            if (Physics.SphereCast(projectionRay, radius, out RaycastHit hit, Vector3.Distance(newPos, origin), obstructionLayers)) //Check for collisions between position and target
            {
                newPos = hit.point + (radius * 1.01f * hit.normal); //Get obstructed position
            }
            else break; //Stop once all obstructions have been processed
        }
        if (Physics.CheckSphere(newPos, radius, obstructionLayers)) newPos = origin; //Cancel entire movement if collisions are not resolved
        return newPos; //Return obstructed position
    }
    /// <summary>
    /// Sets weight for every IK solver in hand.
    /// </summary>
    private void SetIKWeights(float newWeight)
    {
        foreach (FABRIK solver in ikSolvers) solver.solver.IKPositionWeight = newWeight;
    }
}
