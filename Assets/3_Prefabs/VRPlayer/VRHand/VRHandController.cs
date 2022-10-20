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
    internal VRHandController otherHand; //Reference to controller script for opposite hand
    private Transform obstructedTarget;  //Target position which does not collide with objects in scene
    private Transform[] fingerTargets;   //Array of all targets used to position finger IK rigs
    private Transform[] fingerRoots;     //Root positions of hand fingers (should eventually be animated)
    private InputActionMap inputMap;     //The input map for this hand
    private FABRIK[] ikSolvers;          //Array of all IK solvers in hand

    //Settings:
    [Header("Gamefeel Settings:")]
    [SerializeField, Tooltip("Moves where on controller hands stick to")]                                             private Vector3 followOffset;
    [Min(0), SerializeField, Tooltip("Maximum speed at which hand can move")]                                         private float maxSpeed;
    [Min(0), SerializeField, Tooltip("Rate at which hand seeks target position")]                                     private float linearFollowSpeed;
    [Min(0), SerializeField, Tooltip("Rate at which hand seeks target rotation")]                                     private float angularFollowSpeed;
    [Min(0), SerializeField, Tooltip("Maximum speed at which individual fingers can adjust their positions")]         private float maxFingerMoveSpeed;
    [SerializeField, Tooltip("Raise to make dropping the hand quicker")]                                              private float dropSpeedMultiplier;
    [SerializeField, Tooltip("Lower to make lifting the hand slower")]                                                private float raiseSpeedMultiplier;
    [Range(0, 90),SerializeField, Tooltip("Outer angle which determines where drop multiplier begins to factor in")]  private float dropMultiplierAngle;
    [Range(0, 90),SerializeField, Tooltip("Outer angle which determines where raise multiplier begins to factor in")] private float raiseMultiplierAngle;
    [Header("Mobility Settings:")]
    [Min(0), SerializeField, Tooltip("Smoothness of player grip movement")]                              private float gripMoveLerpRate;
    [Min(0), SerializeField, Tooltip("Maximum speed at which player can gripMove themselves")]           private float maxGripMoveSpeed;
    [Min(0), SerializeField, Tooltip("Rate at which player origin velocity slows after releasing grip")] private float smoothGripStopRate;
    [Min(0), SerializeField, Tooltip("Maximum distance from center of world player can move")]           private float outerBoundRadius;
    [Header("Physics Settings:")]
    [Min(0), SerializeField, Tooltip("Size of main hand collider")]                                                 private float palmCollisionRadius;
    [Min(0), SerializeField, Tooltip("Radius of fingertip colliders")]                                              private float fingerTipRadius;
    [Min(0), SerializeField, Tooltip("Maximum depth inside colliders which finger targets can penetrate")]          private float maxProjectionDepth;
    [SerializeField, Tooltip("Physics layers which fingers on hand are able to collide with")]                      private LayerMask obstructionLayers;
    [Min(1), SerializeField, Tooltip("Maximum number of obstructions hand and fingers can collide with per frame")] private int maxObstacleCollisions = 1;
    [Header("Input Settings:")]
    [Range(0, 1), SerializeField, Tooltip("Grip threshold for turning hand into fist")] private float fistGripThreshold;
    [Range(0, 1), SerializeField, Tooltip("Grip threshold for doing grabmove")]         private float surfaceGripThreshold;
    [Space()]
    [Min(0), SerializeField] private float palmSurfaceCheckDistance;
    [Min(0), SerializeField] private float palmSurfaceCheckRadius;
    [Header("Effects:")]
    [Min(0), SerializeField, Tooltip("Ease by which fingers are affected by changes in velocity")]         private float fingerVelocityDragFactor;
    [Min(0), SerializeField, Tooltip("Maximum distance by which fingers can be dragged back by velocity")] private float maxFingerVelocityDrag;

    //Runtime Variables:
    /// <summary>
    /// Which side this hand is on.
    /// </summary>
    internal HandType side = HandType.None; //Initialize at "None" to indicate hand has not yet been associated with player controller
    
    //Input Variables:
    private float gripValue;                    //How closed this hand currently is
    internal GripType gripType = GripType.Open; //What grip form the hand is currently in
    private bool palmOnSurface = false;         //Whether or not flat palm is on a surface
    private Vector3 surfaceGripTarget;          //Target which moves to position gripped by hand
    private Vector3 lastOriginVelocity;

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

            newRoot.parent = transform;                                    //TEMP child root directly to hand
            newRoot.position = fabrik.solver.bones[^1].transform.position; //TEMP move root to position of fingertip
        }
        fingerTargets = newFingerTargets.ToArray(); //Store finger target list
        fingerRoots = newFingerRoots.ToArray();     //Store finger root list
    }
    private void Start()
    {
        //Initialize:
        if (controllerTarget != null)
        {
            obstructedTarget.position = controllerTarget.position; //Set target reference position if possible
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
        //Perform positional update:
        if (controllerTarget != null && obstructedTarget != null) //Both targeting references need to be present for system to function
        {
            //Initialize:
            float scaleMultiplier = 1; if (VRPlayerController.main != null) scaleMultiplier = VRPlayerController.main.transform.localScale.x; //Get multiplier for maintaining properties despite changes in scaling
            Vector3 prevPosition = transform.position;                                                                                        //Get position before all checks
            Vector3 offsetTargetPos = controllerTarget.position + controllerTarget.TransformVector(followOffset);                             //Get actual target position to seek (ALWAYS home toward this, not controllerTarget)

            //Special grip behaviors:
            switch (gripType)
            {
                case GripType.GrabLocked: //Player is grabbing something and is locked into the world
                    //Move player origin:
                    Vector3 currentOriginPos = VRPlayerController.main.origin.position; //Get quick reference for current position of origin
                    Vector3 targetOriginPos = currentOriginPos;                         //Initialize positional target value at position of player
                    targetOriginPos += surfaceGripTarget - controllerTarget.position;   //Modify position based on hand movement

                    targetOriginPos = Vector3.Lerp(currentOriginPos, targetOriginPos, gripMoveLerpRate * Time.deltaTime); //Lerp toward target position
                    float adjMaxSpeed = maxGripMoveSpeed * scaleMultiplier * Time.deltaTime;                              //Get effective max speed
                    if (Vector3.Distance(targetOriginPos, currentOriginPos) > adjMaxSpeed) //Player is moving too fast
                    {
                        targetOriginPos = Vector3.MoveTowards(currentOriginPos, targetOriginPos, adjMaxSpeed); //Move origin at max speed toward target
                    }
                    Vector2 flatOriginPosition = new Vector2(targetOriginPos.x, targetOriginPos.z); //Player is trying to move outside boundary
                    if (Vector2.Distance(flatOriginPosition, Vector2.zero) > outerBoundRadius) //Player is trying to move out of bounds
                    {
                        flatOriginPosition = flatOriginPosition.normalized * outerBoundRadius;                        //Move player to edge of boundary
                        targetOriginPos = new Vector3(flatOriginPosition.x, targetOriginPos.y, flatOriginPosition.y); //Commit movement
                    }
                    lastOriginVelocity = targetOriginPos - currentOriginPos;   //Record velocity
                    VRPlayerController.main.origin.position = targetOriginPos; //Apply positional change
                    break;
                case GripType.Fist: //Player's hand is balled in a fist
                    break;
                case GripType.Slap: //Player's hand is open and is traveling at slapping speed
                    break;
                case GripType.Open: //Player's hand is open
                    //Palm state check:
                    if (Physics.CheckSphere(transform.position + (palmSurfaceCheckDistance * scaleMultiplier * transform.right), palmSurfaceCheckRadius * scaleMultiplier, obstructionLayers)) //Palm is touching surface
                    {
                        palmOnSurface = true; //Indicate that palm is now on surface
                    }
                    else palmOnSurface = false; //Indicate that palm is not touching a surface
                    break;
                default: break;
            }

            //Unrestrained motion:
            if (gripType != GripType.GrabLocked) //Only rotate if not locked to a surface
            {
                //Find rotation target:
                Quaternion rotTarget = controllerTarget.rotation; //Get default rotation target from controller

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
                    Vector3 newFingerPos = fingerTargets[i].position;
                    newFingerPos += Vector3.ClampMagnitude(-(newFingerPos - prevFingerPositions[i]) * fingerVelocityDragFactor, maxFingerVelocityDrag * scaleMultiplier); //Add velocity drag effect to finger
                    newFingerPos = GetObstructedPosition(prevFingerPositions[i], newFingerPos, fingerTipRadius * scaleMultiplier); //Keep each finger individually obstructed (scrubbing out velocity movement)
                    fingerTargets[i].position = newFingerPos; //Set new finger position
                }

                //Extra origin motion:
                if (lastOriginVelocity != Vector3.zero) //Origin has some velocity remaining from movement
                {
                    lastOriginVelocity = Vector3.Lerp(lastOriginVelocity, Vector3.zero, smoothGripStopRate * Time.deltaTime);
                    VRPlayerController.main.origin.position += lastOriginVelocity;
                    if (Vector3.Distance(lastOriginVelocity, Vector3.zero) < 0.001f) lastOriginVelocity = Vector3.zero;
                }
            }
            else
            {
                offsetTargetPos = surfaceGripTarget;
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

            //Get velocity:
            Vector3 currentVelocity = (newPosition - transform.position) / Time.deltaTime; //Get velocity this frame (in units per second)
            currentVelocity = Vector3.ProjectOnPlane(currentVelocity, transform.forward);
            
            //Project & obstruct fingers:
            obstructedTarget.position = GetObstructedPosition(obstructedTarget.position, offsetTargetPos, scaledRadius); //Update position of obstructed target
            Vector3 projectionDepth = offsetTargetPos - obstructedTarget.position;                                       //Get amount by which fingers should be projected out based on obstruction
            projectionDepth = Vector3.ClampMagnitude(projectionDepth, maxProjectionDepth);                               //Clamp projectionDepth magnitude
            scaledRadius = fingerTipRadius * scaleMultiplier;                                                            //Update scaled radius so it is useable for fingertips
            for (int i = 0; i < fingerTargets.Length; i++) //Iterate through fingerTargets array
            {
                //Generate target for finger:
                Vector3 fingerIdealPos = fingerRoots[i].position;                                       //Get ideal target relative to finger root
                if (unobstructedPosition != newPosition) fingerIdealPos += projectionDepth * gripValue; //Project fingers away from roots if hand is obstructed

                fingerIdealPos += Vector3.ClampMagnitude(-currentVelocity * fingerVelocityDragFactor, maxFingerVelocityDrag * scaleMultiplier); //Add velocity drag effect to finger

                Vector3 newFingerPos = Vector3.MoveTowards(fingerTargets[i].position, fingerIdealPos, maxFingerMoveSpeed * scaleMultiplier * Time.deltaTime); //Move fingers towards target relative to root
                newFingerPos = GetObstructedPosition(fingerTargets[i].localPosition + prevPosition, newFingerPos, scaledRadius);                              //Keep each finger individually obstructed (scrubbing out velocity movement)
                fingerTargets[i].position = newFingerPos;                                                                                                     //Set new finger position
            }

            //Commit hand movement:
            transform.position = newPosition; //Set new position
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
                    gripType = GripType.Open; //Release hand
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
                if (palmOnSurface && prevGrip < surfaceGripThreshold && gripValue >= surfaceGripThreshold) //Player is grabbing a surface
                {
                    //Initialize grip:
                    gripType = GripType.GrabLocked;                  //Indicate that player is gripping surface
                    surfaceGripTarget = controllerTarget.position;   //Set position of surface target

                    if (otherHand.gripType == GripType.GrabLocked) otherHand.gripType = GripType.Open; //Open other hand to prevent double-gripping
                }
                else if (gripValue >= fistGripThreshold) //Player is making a fist
                {
                    gripType = GripType.Fist; //Indicate that player is clenching fist

                }
                break;
        }
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
