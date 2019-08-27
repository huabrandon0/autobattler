using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

namespace TeamfightTactics.InputSystem
{
    /// <summary>
    /// An interface for Unity's input system.
    /// This class creates a mapping of strings to KeyCodes to aid in the readability of inputs.
    /// </summary>
    public class InputManager
    {
        static InputManager _instance = new InputManager();
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InputManager();
                return _instance;
            }
        }

        InputManager() // Private constructor
        {
            if (!InputManagerSettings)
            {
                Resources.LoadAll<InputManagerSettings>("");
                InputManagerSettings = Resources.FindObjectsOfTypeAll<InputManagerSettings>().FirstOrDefault();
                if (!InputManagerSettings)
                    Debug.LogError("Could not find an instance of InputManagerSettings");
            }
        }
        
        [SerializeField]
        public InputManagerSettings InputManagerSettings { get; set; }

        HashSet<string> SimulatedGetKeyDowns { get; set; } = new HashSet<string>();
        HashSet<string> SimulatedGetKeyUps { get; set; } = new HashSet<string>();
        HashSet<string> SimulatedGetKeys { get; set; } = new HashSet<string>();

        /// <summary>
        /// Gets the keypress status.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool GetKey(string key)
        {
            if (SimulatedGetKeys.Contains(key))
                return true;

            InputManagerSettings.InputCodeMapping.TryGetValue(key, out InputCodeAggregate inputCodes);
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

            InputManagerSettings.InputCodeMapping.TryGetValue(key, out InputCodeAggregate inputCodes);
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

            InputManagerSettings.InputCodeMapping.TryGetValue(key, out InputCodeAggregate inputCodes);
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
            return Input.GetKeyDown(InputManagerSettings.PauseKey);
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
}
