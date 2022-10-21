using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GlobalEnums;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using Unity.XR.CoreUtils;
using RootMotion.FinalIK;
using UnityEngine.Events;

public class VRPlayerController : MonoBehaviour
{
    //Classes, Structs & Enums:
    

    //Static Stuff:
    /// <summary>
    /// Single instance of this script in scene.
    /// </summary>
    public static VRPlayerController main;

    //Objects & Components:
    private Transform leftController;        //Position and orientation of left controller in scene
    private Transform rightController;       //Position and orientation of right controller in scene
    private Transform head;                  //Position and orientation of head in scene
    internal Transform origin;               //Transform to use when moving VR player around
    private VRHandController leftHand;       //Controller for left hand object
    private VRHandController rightHand;      //Controller for right hand object
    private AudioSource audioSource;         //Audiosource for VR player head
    private InputActionManager inputManager; //Input management asset

    //Settings:
    [Header("References & Prefabs:")]
    [SerializeField()] private GameObject handPrefab;
    [SerializeField()] private GameObject gibbedPrefab;
    [SerializeField()] private Transform headModel;
    //[SerializeField] private 
    [Header("General Settings:")]
    [SerializeField(), Tooltip("Health the player starts at")] private int maxHealth;
    [SerializeField, Tooltip("Vertical limits which player head may not move outside of")] private Vector2 hardVerticalBounds;
    [Min(0), SerializeField, Tooltip("")]                                                  private float headRadius;
    [SerializeField, Tooltip("Physics layers which head is able to collide with")]         private LayerMask obstructionLayers;
    [Header("Death Sequencing:")]
    [SerializeField] private float[] deathSequenceTimes;
    [Min(0.01f), SerializeField(), Tooltip("Curve describing fade out effect of eye light upon death")]     private AnimationCurve death_EyeFadeCurve;
    [Header("Sounds:")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip deathSound;
    [Header("Haptics:")]
    [SerializeField] private Vector2 handExplodeHaptics;

    //Runtime Vars:
    internal int health;       //Amount of health player currently has
    private bool dead = false; //Whether or not VR player is currently dead
    private Vector3 prevHeadPos;

    //EVENTS & COROUTINES:
    /// <summary>
    /// Event called whenever boss takes damage.
    /// </summary>
    public static UnityAction isHurtEvent;

    IEnumerator DeathSequence()
    {
        //Eye fade:
        float timer = 0; //Initialize timer for checking point in sequence
        Light[] lights = GetComponentsInChildren<Light>();
        List<float> initialLightValues = new List<float>();
        foreach (Light light in lights) initialLightValues.Add(light.intensity);
        while (timer <= deathSequenceTimes[0])
        {
            timer += Time.fixedDeltaTime;
            float fadeValue = death_EyeFadeCurve.Evaluate(timer / deathSequenceTimes[0]);
            for (int i = 0; i < lights.Length; i++) lights[i].intensity = initialLightValues[i] * fadeValue;
            yield return new WaitForFixedUpdate();
        }

        //Gibs:
        yield return new WaitForSeconds(deathSequenceTimes[1]);
        leftHand.Gib();
        SendHapticImpulse(HandType.Left, handExplodeHaptics);
        yield return new WaitForSeconds(deathSequenceTimes[2]);
        rightHand.Gib();
        SendHapticImpulse(HandType.Right, handExplodeHaptics);
        yield return new WaitForSeconds(deathSequenceTimes[3]);
        Transform gibs = Instantiate(gibbedPrefab).transform;
        gibs.parent = headModel.parent;
        gibs.position = headModel.position;
        gibs.rotation = headModel.rotation;
        gibs.localScale = headModel.localScale;
        Destroy(headModel.gameObject);

        //Cleanup:
        yield return null; //End sequence
    }

    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (main == null) { main = this; } else { Destroy(gameObject); } //Destroy this instance of player if it is duplicated in scene
        health = maxHealth;                                              //Set initial health

        //Get objects & components:
        head = GetComponentInChildren<Camera>().transform; if (head == null) { Debug.LogError("VR player could not find head!"); } //Get head transform
        audioSource = GetComponent<AudioSource>();                                                                                 //Get audio source component
        inputManager = GetComponentInChildren<InputActionManager>();                                                               //Get input manager component
        origin = GetComponentInChildren<XROrigin>().transform;                                                                     //Get transform of player origin point

        //Hand setup:
        ActionBasedController[] handControllers = GetComponentsInChildren<ActionBasedController>();                                                                          //Get hand controllers
        if (handControllers.Length < 2) Debug.LogError("Could not find VR player's hands!");                                                                                 //Post error if hands could not be found
        if (handControllers[0].gameObject.name.Contains("Left")) { leftController = handControllers[0].transform; } else { rightController = handControllers[0].transform; } //Assign first hand
        if (rightController == null) { rightController = handControllers[1].transform; } else { leftController = handControllers[1].transform; }                             //Assign second hand
        InstantiateHand(HandType.Left); InstantiateHand(HandType.Right);                                                                                                     //Instantiate and set up objects for both hands

        //Event subscriptions:
        isHurtEvent += OnHurtDebug; //Base event subscription
    }
    private void OnDisable()
    {
        isHurtEvent -= OnHurtDebug;
    }
    private void Start()
    {
        //Initialize:
        audioSource.PlayOneShot(spawnSound); //Play spawn sound
        prevHeadPos = head.position;         //Get starting position of head
    }
    private void Update()
    {
        //Keep head within bounds:
        Vector3 newHeadPos = head.transform.position;
        if (newHeadPos.y < hardVerticalBounds.x) origin.Translate(0, hardVerticalBounds.x - newHeadPos.y, 0);      //Immediately move origin vertically if player's head is outside bounds
        else if (newHeadPos.y > hardVerticalBounds.y) origin.Translate(0, hardVerticalBounds.y - newHeadPos.y, 0); //Immediately move origin vertically if player's head is outside bounds

        //Check head obstruction:
        Vector3 headMovement = head.transform.position - prevHeadPos; //Get head movement since last update
        if (Physics.SphereCast(prevHeadPos, headRadius, headMovement.normalized, out RaycastHit hit, headMovement.magnitude, obstructionLayers))
        {
            float displacementLength = (headMovement.magnitude - hit.distance) + 0.001f; //Get distance by which to displace origin based on head obstruction
            Vector3 offsetAmount = -headMovement.normalized * displacementLength;        //Get amount to offset origin by
            origin.position += offsetAmount;                                             //Apply offset to origin
        }
        prevHeadPos = head.transform.position; //Record head position
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Called when the VR player is killed.
    /// </summary>
    private void Die()
    {
        if (dead) return;
        dead = true;
        StartCoroutine(DeathSequence());
        audioSource.PlayOneShot(deathSound);
        print("KILLED VR PLAYER");
    }

    //OPERATION METHODS:
    /// <summary>
    /// Deals an amount of damage to VR player.
    /// </summary>
    /// <param name="damage">How much damage to deal.</param>
    public static void DealDamage(int damage)
    {
        //Validity checks:
        if (main == null) return;      //Ignore if there is no VR player
        if (main.dead == true) return; //Ignore if VR player is already dead

        //Damage procedure:
        main.health -= damage;            //Deal designated amount of damage to VR player
        if (main.health <= 0) main.Die(); //Kill VR player if health drops below zero
        isHurtEvent();                    //Trigger hurt event
    }
    /// <summary>
    /// Sends a haptic impulse to VR player's hand(s).
    /// </summary>
    /// <param name="targetHand">Which hand(s) to send the impulse to.</param>
    /// <param name="amplitude">Strenth (0-1) of impulse.</param>
    /// <param name="duration">Length (in seconds) of impulse.</param>
    public static void SendHapticImpulse(HandType targetHand, float amplitude, float duration)
    {
        if (main == null) return;
        switch (targetHand)
        {
            case HandType.None:
                return;
            case HandType.Left:
                main.SendHapticImpulse(UnityEngine.XR.InputDeviceRole.LeftHanded, amplitude, duration);
                return;
            case HandType.Right:
                main.SendHapticImpulse(UnityEngine.XR.InputDeviceRole.RightHanded, amplitude, duration);
                return;
            case HandType.Both:
                main.SendHapticImpulse(UnityEngine.XR.InputDeviceRole.LeftHanded, amplitude, duration);
                main.SendHapticImpulse(UnityEngine.XR.InputDeviceRole.RightHanded, amplitude, duration);
                return;
        }
    }
    /// <summary>
    /// Sends a haptic impulse to VR player's hand(s).
    /// </summary>
    /// <param name="targetHand">Which hand(s) to send the impulse to.</param>
    /// <param name="amplitude">Strenth (0-1) of impulse.</param>
    /// <param name="duration">Length (in seconds) of impulse.</param>
    public static void SendHapticImpulse(HandType targetHand, Vector2 haptics)
    {
        if (main == null) return;
        switch (targetHand)
        {
            case HandType.None:
                return;
            case HandType.Left:
                main.SendHapticImpulse(UnityEngine.XR.InputDeviceRole.LeftHanded, haptics.x, haptics.y);
                return;
            case HandType.Right:
                main.SendHapticImpulse(UnityEngine.XR.InputDeviceRole.RightHanded, haptics.x, haptics.y);
                return;
            case HandType.Both:
                main.SendHapticImpulse(UnityEngine.XR.InputDeviceRole.LeftHanded, haptics.x, haptics.y);
                main.SendHapticImpulse(UnityEngine.XR.InputDeviceRole.RightHanded, haptics.x, haptics.y);
                return;
        }
    }

    //UTILITY METHODS:
    private void InstantiateHand(HandType handType)
    {
        //Validity checks:
        if (handPrefab == null) { Debug.LogError("VR player is missing hand prefab!"); return; }                                               //Make sure hand prefab is present
        if (!handPrefab.GetComponent<VRHandController>()) { Debug.LogError("VR hand prefab does not have VRHandController script!"); return; } //Make sure hand prefab has a controller
        if (handType != HandType.Left && handType != HandType.Right) return;                                                                   //Ignore if invalid hand type is given

        //Universal initialization:
        VRHandController newHand = Instantiate(handPrefab).GetComponent<VRHandController>(); //Instantiate a new hand prefab
        VRHandController otherHand = null;                                                   //Initialize container to check for and modify other hand
        newHand.transform.parent = transform;                                                //Child hand to player object
        newHand.transform.localScale = Vector3.one;                                          //Keep hands scaled to player object
        newHand.transform.position = head.position;                                          //Move hand to position of head

        //Side-specific initialization:
        newHand.side = handType; //Set side based on designated hand type
        if (handType == HandType.Left) //Left hand setup
        {
            //Symmetrical setup:
            if (leftHand != null) Destroy(leftHand.gameObject); //Destroy left hand object if it already exists
            newHand.controllerTarget = leftController;          //Set controller target
            newHand.SetupInput(inputManager.actionAssets[0].FindActionMap("XRI LeftHand Control")); //Assign hand action map
            leftHand = newHand;                                //Store reference to new left hand
            otherHand = rightHand;                             //Get other hand
        }
        else //Right hand setup
        {
            //Symmetrical setup:
            if (rightHand != null) Destroy(rightHand.gameObject); //Destroy right hand object if it already exists
            newHand.controllerTarget = rightController;           //Set controller target
            newHand.SetupInput(inputManager.actionAssets[0].FindActionMap("XRI RightHand Control")); //Assign hand action map
            rightHand = newHand;                                  //Store reference to new right hand
            otherHand = leftHand;                                 //Get other hand

            //Flip hand:
            newHand.transform.localScale = Vector3.one * -1; //Flip hand along all axes
            foreach (BoxCollider box in newHand.GetComponentsInChildren<BoxCollider>()) //Iterate through box colliders in hand
            {
                //box.transform.localScale = Vector3.one * -1;
            }
        }

        //Try to get other hand:
        if (otherHand != null) //Other hand is present
        {
            newHand.otherHand = otherHand; //Give new hand a reference to its counterpart
            otherHand.otherHand = newHand; //Give other hand a reference to new hand
        }
    }
    private void SendHapticImpulse(UnityEngine.XR.InputDeviceRole deviceRole, float amp, float dur)
    {
        //Send impulse:
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>(); //Initialize list to store input devices
        #pragma warning disable CS0618                                                     //Disable obsolescence warning
        UnityEngine.XR.InputDevices.GetDevicesWithRole(deviceRole, devices);               //Find all input devices counted as right hand
        #pragma warning restore CS0618                                                     //Re-enable obsolescence warning
        foreach (var device in devices) //Iterate through list of devices identified as right hand
        {
            if (device.TryGetHapticCapabilities(out UnityEngine.XR.HapticCapabilities capabilities)) //Device has haptic capabilities
            {
                if (capabilities.supportsImpulse) device.SendHapticImpulse(0, amp, dur); //Send impulse if supported by device
            }
        }
    }
    private void OnHurtDebug() { }
}
