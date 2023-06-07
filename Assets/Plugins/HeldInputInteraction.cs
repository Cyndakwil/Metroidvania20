using UnityEngine;
using UnityEngine.InputSystem;

//!!>> This script should NOT be placed in an "Editor" folder. Ideally placed in a "Plugins" folder.
namespace Invertex.UnityInputExtensions.Interactions
{
    //https://gist.github.com/Invertex
    /// <summary>
    /// Custom Hold interaction for New Input System.
    /// With this, the .performed callback will be called everytime the Input System updates. 
    /// Allowing a purely callback based approach to a button hold instead of polling it in an Update() loop and using bools
    /// .started will be called when the 'pressPoint' threshold has been met and held for the 'duration'.
    /// .performed will continue to be called each frame after `.started` has triggered.
    /// .cancelled will be called when no-longer actuated (but only if the input has actually 'started' triggering
    /// </summary>
#if UNITY_EDITOR
    //Allow for the interaction to be utilized outside of Play Mode and so that it will actually show up as an option in the Input Manager
    [UnityEditor.InitializeOnLoad]
#endif
    [UnityEngine.Scripting.Preserve, System.ComponentModel.DisplayName("Holding"), System.Serializable]
    public class CustomHoldingInteraction : IInputInteraction
    {
        public bool useDefaultSettingsPressPoint = false;
        public float pressPoint;

        public bool useDefaultSettingsDuration = false;
        public float duration;

        private float _heldTime = 0f;

        private float pressPointOrDefault => useDefaultSettingsPressPoint || pressPoint <= 0 ? InputSystem.settings.defaultButtonPressPoint : pressPoint;
        private float durationOrDefault => useDefaultSettingsDuration || duration <= 0 ? InputSystem.settings.defaultHoldTime : duration;

        private InputInteractionContext ctx;

        private void OnUpdate()
        {
            var isActuated = ctx.ControlIsActuated(pressPointOrDefault);
            var phase = ctx.phase;

            //Cancel and cleanup our action if it's no-longer actuated or been externally changed to a stopped state.
            if (phase == InputActionPhase.Canceled || phase == InputActionPhase.Disabled || !ctx.action.actionMap.enabled || !isActuated)
            {
                Reset();
                return;
            }

            _heldTime += Time.deltaTime;

            if (_heldTime < durationOrDefault) { return; }  //Don't do anything yet, hold time not exceeded

            //We've held for long enough, start triggering the Performed state.
            if (phase == InputActionPhase.Performed || phase == InputActionPhase.Started || phase == InputActionPhase.Waiting)
            {
                ctx.PerformedAndStayPerformed();
            }
        }

        public void Process(ref InputInteractionContext context)
        {
            ctx = context; //Ensure our Update always has access to the most recently updated context

            if (!ctx.ControlIsActuated(pressPointOrDefault)) { Reset(); return; } //Actuation changed and thus no longer performed, cancel it all.

            if (ctx.phase != InputActionPhase.Performed && ctx.phase != InputActionPhase.Started)
            {
                EnableInputHooks();
            }
        }

        private void Cancel(ref InputInteractionContext context)
        {
            DisableInputHooks();

            _heldTime = 0f;

            if (context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Started)
            { //Input was being held when this call was made. Trigger the .cancelled event.
                context.Canceled();
            }
        }
        public void Reset() => Cancel(ref ctx);

        private void OnLayoutChange(string layoutName, InputControlLayoutChange change) => Reset();
        private void OnDeviceChange(InputDevice device, InputDeviceChange change) => Reset();
#if UNITY_EDITOR
        private void PlayModeStateChange(UnityEditor.PlayModeStateChange state) => Reset();
#endif
        private void EnableInputHooks()
        {
            InputSystem.onAfterUpdate -= OnUpdate; //Safeguard for duplicate registrations
            InputSystem.onAfterUpdate += OnUpdate;
            //In case layout or device changes, we'll want to trigger a cancelling of the current input action subscription to avoid errors.
            InputSystem.onLayoutChange -= OnLayoutChange;
            InputSystem.onLayoutChange += OnLayoutChange;
            InputSystem.onDeviceChange -= OnDeviceChange;
            InputSystem.onDeviceChange += OnDeviceChange;
            //Prevent the update hook from persisting across a play mode change to avoid errors.

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChange;
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChange;
#endif
        }


        private void DisableInputHooks()
        {
            InputSystem.onAfterUpdate -= OnUpdate;
            InputSystem.onLayoutChange -= OnLayoutChange;
            InputSystem.onDeviceChange -= OnDeviceChange;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChange;
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void RegisterInteraction()
        {
            if (InputSystem.TryGetInteraction("CustomHolding") == null)
            { //For some reason if this is called again when it already exists, it permanently removees it from the drop-down options... So have to check first
                InputSystem.RegisterInteraction<CustomHoldingInteraction>("CustomHolding");
            }
        }

        //Constructor will be called by our Editor [InitializeOnLoad] attribute when outside Play Mode
        static CustomHoldingInteraction() => RegisterInteraction();
    }

#if UNITY_EDITOR
    internal class CustomHoldInteractionEditor : UnityEngine.InputSystem.Editor.InputParameterEditor<CustomHoldingInteraction>
    {
        private static GUIContent pressPointWarning, holdTimeWarning, pressPointLabel, holdTimeLabel;

        protected override void OnEnable()
        {

            pressPointLabel = new GUIContent("Press Point", "The minimum amount this input's actuation value must exceed to be considered \"held\".\n" +
            "Value less-than or equal to 0 will result in the 'Default Button Press Point' value being used from your 'Project Settings > Input System'.");

            holdTimeLabel = new GUIContent("Min Hold Time", "The minimum amount of realtime seconds before the input is considered \"held\".\n" +
            "Value less-than or equal to 0 will result in the 'Default Hold Time' value being used from your 'Project Settings > Input System'.");

            pressPointWarning = UnityEditor.EditorGUIUtility.TrTextContent("Using \"Default Button Press Point\" set in project-wide input settings.");
            pressPointWarning = UnityEditor.EditorGUIUtility.TrTextContent("Using \"Default Button Press Point\" set in project-wide input settings.");
            holdTimeWarning = UnityEditor.EditorGUIUtility.TrTextContent("Using \"Default Hold Time\" set in project-wide input settings.");
        }

        public override void OnGUI()
        {
            DrawDisableIfDefault(ref target.pressPoint, ref target.useDefaultSettingsPressPoint, pressPointLabel, pressPointWarning);
            DrawDisableIfDefault(ref target.duration, ref target.useDefaultSettingsDuration, holdTimeLabel, holdTimeWarning);
        }

        private void DrawDisableIfDefault(ref float value, ref bool useDefault, GUIContent fieldName, GUIContent warningText)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();

            UnityEditor.EditorGUI.BeginDisabledGroup(useDefault);
            value = UnityEditor.EditorGUILayout.FloatField(fieldName, value);
            UnityEditor.EditorGUI.EndDisabledGroup();
            useDefault = UnityEditor.EditorGUILayout.ToggleLeft("Default", useDefault);
            UnityEditor.EditorGUILayout.EndHorizontal();

            if (useDefault || value <= 0)
            {
                UnityEditor.EditorGUILayout.HelpBox(warningText);
            }
        }
    }
#endif
}