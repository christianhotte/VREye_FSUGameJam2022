//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.1
//     from Assets/FPSControls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @FPSControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @FPSControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""FPSControls"",
    ""maps"": [
        {
            ""name"": ""Map"",
            ""id"": ""73afea79-a078-4cd1-948b-cdc50b78caf7"",
            ""actions"": [
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""960207f1-6464-4c7d-afd5-28fc7046ae42"",
                    ""expectedControlType"": ""Delta"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Walk"",
                    ""type"": ""Value"",
                    ""id"": ""420afb12-a62a-4650-8347-766c63a68e7b"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""c08a4d94-51f3-4ccc-83cf-179103dc459a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Shoot"",
                    ""type"": ""Button"",
                    ""id"": ""e14d8d77-8377-413d-b8c6-2a4f1e9bddeb"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Button"",
                    ""id"": ""da05a86b-02c4-44d8-92c3-eba9f9b698cd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Crouch"",
                    ""type"": ""Button"",
                    ""id"": ""8d9b7f93-1771-4825-aa87-bd36afb3d798"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Die"",
                    ""type"": ""Button"",
                    ""id"": ""2e1e3468-a62a-4628-b971-89f104af1872"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Clone"",
                    ""type"": ""Button"",
                    ""id"": ""4c8dfdd6-3e91-499b-9eb2-e0a3931ad300"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Invis"",
                    ""type"": ""Button"",
                    ""id"": ""780daa67-6220-4006-9152-29f2f3e54490"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Escape"",
                    ""type"": ""Button"",
                    ""id"": ""32f37444-ccb3-46ce-aca6-cc4e44935ee8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ADS"",
                    ""type"": ""Button"",
                    ""id"": ""f9914a19-8300-4f56-943e-de97d23dea1a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""af25c94d-d1a8-4570-ab02-4036c223ac6d"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""e1922d65-d21f-477d-b048-c9f682d01086"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Walk"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""95a7fc2d-dff4-4c37-aca0-fb98d470dd63"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Walk"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""fb2eb415-5418-401d-ba38-989449078431"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Walk"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""42bcd518-a6e6-4aba-ba33-5a03ede48695"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Walk"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""6e3eb0eb-42b1-4794-a705-4ffb96041683"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Walk"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""6db801fc-6703-44a6-bd1e-2e05057d18e3"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c133964-9130-4399-85f1-b45c698c0c35"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1bcaa65b-6d97-40e6-8ece-277e971a93be"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9173d9fd-aec2-47f1-b7a4-18c69c89150b"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Crouch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3c8ee055-2486-4202-a282-28de02297930"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Die"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d36d009b-5530-4701-9b64-46e86d082b65"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Clone"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c8d30e2c-3554-4e42-893a-03213f653266"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Invis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6e369138-e7be-45f5-ba12-d24c3da69eb5"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Escape"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""443ec7f5-f729-4ec6-8305-e84a135cb7ab"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ADS"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Map
        m_Map = asset.FindActionMap("Map", throwIfNotFound: true);
        m_Map_Look = m_Map.FindAction("Look", throwIfNotFound: true);
        m_Map_Walk = m_Map.FindAction("Walk", throwIfNotFound: true);
        m_Map_Jump = m_Map.FindAction("Jump", throwIfNotFound: true);
        m_Map_Shoot = m_Map.FindAction("Shoot", throwIfNotFound: true);
        m_Map_Sprint = m_Map.FindAction("Sprint", throwIfNotFound: true);
        m_Map_Crouch = m_Map.FindAction("Crouch", throwIfNotFound: true);
        m_Map_Die = m_Map.FindAction("Die", throwIfNotFound: true);
        m_Map_Clone = m_Map.FindAction("Clone", throwIfNotFound: true);
        m_Map_Invis = m_Map.FindAction("Invis", throwIfNotFound: true);
        m_Map_Escape = m_Map.FindAction("Escape", throwIfNotFound: true);
        m_Map_ADS = m_Map.FindAction("ADS", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Map
    private readonly InputActionMap m_Map;
    private IMapActions m_MapActionsCallbackInterface;
    private readonly InputAction m_Map_Look;
    private readonly InputAction m_Map_Walk;
    private readonly InputAction m_Map_Jump;
    private readonly InputAction m_Map_Shoot;
    private readonly InputAction m_Map_Sprint;
    private readonly InputAction m_Map_Crouch;
    private readonly InputAction m_Map_Die;
    private readonly InputAction m_Map_Clone;
    private readonly InputAction m_Map_Invis;
    private readonly InputAction m_Map_Escape;
    private readonly InputAction m_Map_ADS;
    public struct MapActions
    {
        private @FPSControls m_Wrapper;
        public MapActions(@FPSControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Look => m_Wrapper.m_Map_Look;
        public InputAction @Walk => m_Wrapper.m_Map_Walk;
        public InputAction @Jump => m_Wrapper.m_Map_Jump;
        public InputAction @Shoot => m_Wrapper.m_Map_Shoot;
        public InputAction @Sprint => m_Wrapper.m_Map_Sprint;
        public InputAction @Crouch => m_Wrapper.m_Map_Crouch;
        public InputAction @Die => m_Wrapper.m_Map_Die;
        public InputAction @Clone => m_Wrapper.m_Map_Clone;
        public InputAction @Invis => m_Wrapper.m_Map_Invis;
        public InputAction @Escape => m_Wrapper.m_Map_Escape;
        public InputAction @ADS => m_Wrapper.m_Map_ADS;
        public InputActionMap Get() { return m_Wrapper.m_Map; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MapActions set) { return set.Get(); }
        public void SetCallbacks(IMapActions instance)
        {
            if (m_Wrapper.m_MapActionsCallbackInterface != null)
            {
                @Look.started -= m_Wrapper.m_MapActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnLook;
                @Walk.started -= m_Wrapper.m_MapActionsCallbackInterface.OnWalk;
                @Walk.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnWalk;
                @Walk.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnWalk;
                @Jump.started -= m_Wrapper.m_MapActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnJump;
                @Shoot.started -= m_Wrapper.m_MapActionsCallbackInterface.OnShoot;
                @Shoot.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnShoot;
                @Shoot.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnShoot;
                @Sprint.started -= m_Wrapper.m_MapActionsCallbackInterface.OnSprint;
                @Sprint.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnSprint;
                @Sprint.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnSprint;
                @Crouch.started -= m_Wrapper.m_MapActionsCallbackInterface.OnCrouch;
                @Crouch.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnCrouch;
                @Crouch.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnCrouch;
                @Die.started -= m_Wrapper.m_MapActionsCallbackInterface.OnDie;
                @Die.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnDie;
                @Die.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnDie;
                @Clone.started -= m_Wrapper.m_MapActionsCallbackInterface.OnClone;
                @Clone.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnClone;
                @Clone.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnClone;
                @Invis.started -= m_Wrapper.m_MapActionsCallbackInterface.OnInvis;
                @Invis.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnInvis;
                @Invis.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnInvis;
                @Escape.started -= m_Wrapper.m_MapActionsCallbackInterface.OnEscape;
                @Escape.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnEscape;
                @Escape.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnEscape;
                @ADS.started -= m_Wrapper.m_MapActionsCallbackInterface.OnADS;
                @ADS.performed -= m_Wrapper.m_MapActionsCallbackInterface.OnADS;
                @ADS.canceled -= m_Wrapper.m_MapActionsCallbackInterface.OnADS;
            }
            m_Wrapper.m_MapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
                @Walk.started += instance.OnWalk;
                @Walk.performed += instance.OnWalk;
                @Walk.canceled += instance.OnWalk;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Shoot.started += instance.OnShoot;
                @Shoot.performed += instance.OnShoot;
                @Shoot.canceled += instance.OnShoot;
                @Sprint.started += instance.OnSprint;
                @Sprint.performed += instance.OnSprint;
                @Sprint.canceled += instance.OnSprint;
                @Crouch.started += instance.OnCrouch;
                @Crouch.performed += instance.OnCrouch;
                @Crouch.canceled += instance.OnCrouch;
                @Die.started += instance.OnDie;
                @Die.performed += instance.OnDie;
                @Die.canceled += instance.OnDie;
                @Clone.started += instance.OnClone;
                @Clone.performed += instance.OnClone;
                @Clone.canceled += instance.OnClone;
                @Invis.started += instance.OnInvis;
                @Invis.performed += instance.OnInvis;
                @Invis.canceled += instance.OnInvis;
                @Escape.started += instance.OnEscape;
                @Escape.performed += instance.OnEscape;
                @Escape.canceled += instance.OnEscape;
                @ADS.started += instance.OnADS;
                @ADS.performed += instance.OnADS;
                @ADS.canceled += instance.OnADS;
            }
        }
    }
    public MapActions @Map => new MapActions(this);
    public interface IMapActions
    {
        void OnLook(InputAction.CallbackContext context);
        void OnWalk(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnShoot(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnCrouch(InputAction.CallbackContext context);
        void OnDie(InputAction.CallbackContext context);
        void OnClone(InputAction.CallbackContext context);
        void OnInvis(InputAction.CallbackContext context);
        void OnEscape(InputAction.CallbackContext context);
        void OnADS(InputAction.CallbackContext context);
    }
}
