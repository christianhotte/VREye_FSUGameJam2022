using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GlobalEnums;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using Unity.XR.CoreUtils;

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
    [SerializeField()] private Transform headModel;
    [Header("General Settings:")]
    [SerializeField(), Tooltip("Health the player starts at")] private int maxHealth;
    [Header("Death Sequencing:")]
    [Min(0.01f), SerializeField(), Tooltip("Time (in seconds) taken for eye to fade after VR player dies")] private float death_EyeFadeTime;
    [Min(0.01f), SerializeField(), Tooltip("Curve describing fade out effect of eye light upon death")]     private AnimationCurve death_EyeFadeCurve;
    [Header("Sounds:")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip deathSound;

    //Runtime Vars:
    internal int health;       //Amount of health player currently has
    private bool dead = false; //Whether or not VR player is currently dead

    //EVENTS & COROUTINES:
    IEnumerator DeathSequence()
    {
        //Eye fade:
        float fadeTimer = 0; //Initialize timer for checking point in sequence
        Light[] lights = GetComponentsInChildren<Light>();
        List<float> initialLightValues = new List<float>();
        foreach (Light light in lights) initialLightValues.Add(light.intensity);
        while (fadeTimer <= death_EyeFadeTime)
        {
            fadeTimer += Time.fixedDeltaTime;
            float fadeValue = death_EyeFadeCurve.Evaluate(fadeTimer / death_EyeFadeTime);
            for (int i = 0; i < lights.Length; i++) lights[i].intensity = initialLightValues[i] * fadeValue;
            yield return new WaitForFixedUpdate();
        }

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
    }
    private void Start()
    {
        audioSource.PlayOneShot(spawnSound);
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Called when the VR player is killed.
    /// </summary>
    private void Die()
    {
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

            //Side-specific setup:
            Vector3 newScale = newHand.transform.localScale; //Get scale
            newScale.x *= -1;                                //Flip scale along X axis
            //newHand.transform.localScale = newScale;       //Apply new scale to hand
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
}
