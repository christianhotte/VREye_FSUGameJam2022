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
    private Transform obstructedTarget; //Target position which does not collide with objects in scene
    private Transform[] fingerTargets;  //Array of all targets used to position finger IK rigs
    private Transform[] fingerRoots;    //Root positions of hand fingers (should eventually be animated)

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
    [Header("Physics Settings:")]
    [Min(0), SerializeField, Tooltip("Size of main hand collider")]                                                 private float palmCollisionRadius;
    [Min(0), SerializeField, Tooltip("Radius of fingertip colliders")]                                              private float fingerTipRadius;
    [Min(0), SerializeField, Tooltip("Maximum depth inside colliders which finger targets can penetrate")]          private float maxProjectionDepth;
    [SerializeField, Tooltip("Physics layers which fingers on hand are able to collide with")]                      private LayerMask obstructionLayers;
    [Min(1), SerializeField, Tooltip("Maximum number of obstructions hand and fingers can collide with per frame")] private int maxObstacleCollisions = 1;
    [Header("Effects:")]
    [Min(0), SerializeField, Tooltip("Ease by which fingers are affected by changes in velocity")]         private float fingerVelocityDragFactor;
    [Min(0), SerializeField, Tooltip("Maximum distance by which fingers can be dragged back by velocity")] private float maxFingerVelocityDrag;

    //Runtime Variables:
    /// <summary>
    /// Which side this hand is on.
    /// </summary>
    internal HandType side = HandType.None; //Initialize at "None" to indicate hand has not yet been associated with player controller
    private float gripValue; //How closed this hand currently is

    //RUNTIME METHODS:
    private void Awake()
    {
        //Set up hand target:
        obstructedTarget = new GameObject().transform;                                                    //Instantiate a new transform object
        obstructedTarget.name = "ObstructedControllerPos";                                                //Give target a descriptive name
        if (VRPlayerController.main != null) obstructedTarget.parent = VRPlayerController.main.transform; //Child obstructedTarget marker to VR player if possibe

        //Set up finger IK:
        List<Transform> newFingerTargets = new List<Transform>();   //Initialize a list within which to store finger targets
        List<Transform> newFingerRoots = new List<Transform>();     //Initialize a list within which to store finger roots
        FABRIK[] fabrikSolvers = GetComponentsInChildren<FABRIK>(); //Get a list of each FABRIK chain in hand
        foreach (FABRIK fabrik in fabrikSolvers) //Iterate through each finger IK solver in hand
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
    private void Update()
    {
        //Perform positional update:
        if (controllerTarget != null && obstructedTarget != null) //Both targeting references need to be present for system to function
        {
            //Initialize:
            float scaleMultiplier = 1; if (VRPlayerController.main != null) scaleMultiplier = VRPlayerController.main.transform.localScale.x; //Get multiplier for maintaining properties despite changes in scaling
            Vector3 prevPosition = transform.position; //Get position before all checks
            Vector3 offsetTargetPos = controllerTarget.position + controllerTarget.TransformVector(followOffset);

            //Rotate system:
            Quaternion newRotation = Quaternion.Slerp(transform.rotation, controllerTarget.rotation, angularFollowSpeed * Time.deltaTime);
            List<Vector3> prevFingerPositions = new List<Vector3>();
            foreach (Transform finger in fingerTargets)
            {
                prevFingerPositions.Add(finger.position);
            }
            transform.rotation = newRotation; //Set new rotation
            for (int i = 0; i < fingerTargets.Length; i++) //Iterate through fingerTargets array
            {
                Vector3 newFingerPos = GetObstructedPosition(prevFingerPositions[i], fingerTargets[i].position, fingerTipRadius * scaleMultiplier); //Keep each finger individually obstructed (scrubbing out velocity movement)
                fingerTargets[i].position = newFingerPos;                                                                                           //Set new finger position
            }

            //Modify position:
            Vector3 newPosition = transform.position;
            if (transform.position != offsetTargetPos)
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

            //Commit hand movement:
            Vector3 currentVelocity = (newPosition - transform.position) / Time.deltaTime; //Get velocity this frame (in units per second)
            transform.position = newPosition; //Set new position

            //Project & obstruct fingers:
            obstructedTarget.position = GetObstructedPosition(obstructedTarget.position, offsetTargetPos, scaledRadius); //Update position of obstructed target
            Vector3 projectionDepth = offsetTargetPos - obstructedTarget.position;                                       //Get amount by which fingers should be projected out based on obstruction
            projectionDepth = Vector3.ClampMagnitude(projectionDepth, maxProjectionDepth);                               //Clamp projectionDepth magnitude
            scaledRadius = fingerTipRadius * scaleMultiplier;                                                            //Update scaled radius so it is useable for fingertips
            for (int i = 0; i < fingerTargets.Length; i++) //Iterate through fingerTargets array
            {
                //Generate target for finger:
                Vector3 fingerIdealPos = fingerRoots[i].position;                                                                               //Get ideal target relative to finger root
                if (unobstructedPosition != newPosition) fingerIdealPos += projectionDepth;                                                     //Project fingers away from roots if hand is obstructed

                //Vector3 fingerVelocity = (fingerRoots[i].position - prevFingerPositions[i]) / Time.deltaTime;                                  //Get velocity at fingerTip
                fingerIdealPos += Vector3.ClampMagnitude(-currentVelocity * fingerVelocityDragFactor, maxFingerVelocityDrag * scaleMultiplier); //Add velocity drag effect to finger

                Vector3 newFingerPos = Vector3.MoveTowards(fingerTargets[i].position, fingerIdealPos, maxFingerMoveSpeed * scaleMultiplier * Time.deltaTime); //Move fingers towards target relative to root
                newFingerPos = GetObstructedPosition(fingerTargets[i].localPosition + prevPosition, newFingerPos, scaledRadius);                              //Keep each finger individually obstructed (scrubbing out velocity movement)
                fingerTargets[i].position = newFingerPos;                                                                                                     //Set new finger position
            }
        }
    }

    //INPUT METHODS:
    public void OnGripInput(InputAction.CallbackContext context)
    {
        gripValue = context.ReadValue<float>();
        if (gripValue > 0.1f) print(gripValue);
    }

    //FUNCTIONALITY METHODS:

    //OPERATION METHODS:
    public void SetTarget(Transform newTarget)
    {
        controllerTarget = newTarget;
        obstructedTarget.position = newTarget.position;
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
}
