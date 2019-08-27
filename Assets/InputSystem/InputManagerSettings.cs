using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Sirenix.OdinInspector;

namespace TeamfightTactics.InputSystem
{
    /// <summary>
    /// An interface for Unity's input system.
    /// This class creates a mapping of strings to KeyCodes to aid in the readability of inputs.
    /// </summary>
    [CreateAssetMenu(fileName = "New InputManagerSettings", menuName = "InputSystem/InputManagerSettings")]
    public class InputManagerSettings : SerializedScriptableObject
    {
        [SerializeField, HideInInspector]
        public KeyCode PauseKey
        {
            get { return Application.isEditor? KeyCode.T: KeyCode.Escape; }
        } 

        HashSet<string> SimulatedGetKeyDowns { get; set; } = new HashSet<string>();
        HashSet<string> SimulatedGetKeyUps { get; set; } = new HashSet<string>();
        HashSet<string> SimulatedGetKeys { get; set; } = new HashSet<string>();

        [SerializeField]
        public Dictionary<string, InputCodeAggregate> InputCodeMapping { get; set; } = new Dictionary<string, InputCodeAggregate>()
        {
            {"Strafe Up",                   new InputCodeAggregate(new KeyCode[] { KeyCode.W })},
            {"Strafe Left",                 new InputCodeAggregate(new KeyCode[] { KeyCode.A })},
            {"Strafe Down",                 new InputCodeAggregate(new KeyCode[] { KeyCode.S })},
            {"Strafe Right",                new InputCodeAggregate(new KeyCode[] { KeyCode.D })},

            {"Jump",                        new InputCodeAggregate(new KeyCode[] { KeyCode.Space })},
            {"Stroll",                      new InputCodeAggregate(new KeyCode[] { KeyCode.LeftShift })},

            {"Attack1",                     new InputCodeAggregate(new KeyCode[] { KeyCode.Mouse0 })},
            {"Attack2",                     new InputCodeAggregate(new KeyCode[] { KeyCode.Mouse1 })},

            {"Zoom In",                     new InputCodeAggregate(extendedKeyCodes: new ExtendedKeyCode[] { ExtendedKeyCode.MouseScrollWheelUp })},
            {"Zoom Out",                    new InputCodeAggregate(extendedKeyCodes: new ExtendedKeyCode[] { ExtendedKeyCode.MouseScrollWheelDown })}
        };

        /// <summary>
        /// Gets the keypress status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool GetKey(string key)
        {
            if (SimulatedGetKeys.Contains(key))
                return true;

            InputCodeMapping.TryGetValue(key, out InputCodeAggregate inputCodes);
            if (inputCodes != null)
                return inputCodes.GetKey();

            return false;
        }

        /// <summary>
        /// Gets the keypress-down status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool GetKeyDown(string key)
        {
            if (SimulatedGetKeyDowns.Contains(key))
                return true;

            InputCodeMapping.TryGetValue(key, out InputCodeAggregate inputCodes);
            if (inputCodes != null)
                return inputCodes.GetKeyDown();

            return false;
        }

        /// <summary>
        /// Gets the keypress-up status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool GetKeyUp(string key)
        {
            if (SimulatedGetKeyUps.Contains(key))
                return true;

            InputCodeMapping.TryGetValue(key, out InputCodeAggregate inputCodes);
            if (inputCodes != null)
                return inputCodes.GetKeyUp();

            return false;
        }

        /// <summary>
        /// Gets the pause-keypress-down status.
        /// </summary>
        /// <returns></returns>
        public bool GetPauseKeyDown()
        {
            return Input.GetKeyDown(PauseKey);
        }

        /// <summary>
        /// Returns the current cursor position.
        /// If the position is simulated, the simulation is wiped after.
        /// </summary>
        public Vector3 GetMousePosition()
        {
            return Input.mousePosition;
        }

        public float GetAxisRaw(string axisName)
        {
            return Input.GetAxisRaw(axisName);
        }

        /// <summary>
        /// Returns true if the pointer is over a game object.
        /// If the pointer is simulated, it always returns false.
        /// </summary>
        public bool IsPointerOverGameObject()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        ///// <summary>
        ///// Overwrites the keybind for specified key, val, and index.
        ///// </summary>
        ///// <param name="key">The key.</param>
        ///// <param name="val">The value.</param>
        ///// <param name="index">The index.</param>
        //public static void OverwriteKeybind(string key, KeyCode val, int index)
        //{
        //    if (!_inputCodeDict.ContainsKey(key) || (index < 0 && index >= 1))
        //        return;

        //    _inputCodeDict[key][index] = val;
        //}

        /// <summary>
        /// Programmatically simulate a down keypress.
        /// </summary>
        public void SimulateKeyDown(string key)
        {
            SimulatedGetKeyDowns.Add(key);
            SimulatedGetKeys.Add(key);
        }

        public void StopSimulatingKeyDown(string key)
        {
            SimulatedGetKeyDowns.Remove(key);
        }

        public void SimulateKeyUp(string key)
        {
            SimulatedGetKeyUps.Add(key);
            SimulatedGetKeys.Remove(key);
        }

        public void StopSimulatingKeyUp(string key)
        {
            SimulatedGetKeyUps.Remove(key);
        }
    }

    public enum ExtendedKeyCode
    {
        None,
        MouseScrollWheelDown,
        MouseScrollWheelUp
    }

    [System.Serializable]
    public class InputCodeAggregate
    {
        public InputCodeAggregate(KeyCode[] keyCodes = null, ExtendedKeyCode[] extendedKeyCodes = null)
        {
            if (keyCodes != null)
                KeyCodes = keyCodes.ToList();

            if (extendedKeyCodes != null)
                ExtendedKeyCodes = extendedKeyCodes.ToList();

            if (KeyCodes == null)
                KeyCodes = new List<KeyCode>();

            if (ExtendedKeyCodes == null)
                ExtendedKeyCodes = new List<ExtendedKeyCode>();
        }

        [SerializeField]
        List<KeyCode> KeyCodes { get; set; }

        [SerializeField]
        List<ExtendedKeyCode> ExtendedKeyCodes { get; set; }

        public bool GetKey()
        {
            if (ExtendedKeyCodes != null && ExtendedKeyCodes.Contains(ExtendedKeyCode.MouseScrollWheelDown) && Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
                return true;

            if (ExtendedKeyCodes != null && ExtendedKeyCodes.Contains(ExtendedKeyCode.MouseScrollWheelUp) && Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
                return true;

            if (KeyCodes != null)
                return KeyCodes.Any((keyCode) => Input.GetKey(keyCode));

            return false;
        }

        public bool GetKeyDown()
        {
            if (ExtendedKeyCodes != null && ExtendedKeyCodes.Contains(ExtendedKeyCode.MouseScrollWheelDown) && Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
                return true;

            if (ExtendedKeyCodes != null && ExtendedKeyCodes.Contains(ExtendedKeyCode.MouseScrollWheelUp) && Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
                return true;
            
            if (KeyCodes != null)
                return KeyCodes.Any((keyCode) => Input.GetKeyDown(keyCode));

            return false;
        }

        public bool GetKeyUp()
        {
            if (KeyCodes != null)
                return KeyCodes.Any((keyCode) => Input.GetKeyUp(keyCode));

            return false;
        }
    }
}
