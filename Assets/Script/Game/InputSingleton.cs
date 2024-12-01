using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Game
{
    public class InputSingleton : MonoBehaviour
    {
        public static event Action Jump; 
        public static event Action Up; 
        public static event Action Down; 
        public static event Action Right; 
        public static event Action Left; 
        public static event Action CullMode;
        public static event Action DigDown;
        public static event Action DigUp;
        public static event Action OrbitLeft;
        public static event Action OrbitRight;
        public static event Action<float> Zoom;
        public static event Action<float> CullLevel;
        public static event Action<float> HotBar;

        private Dictionary<KeyCode, Action> keyBindings;
        private Dictionary<List<KeyCode>, Action<float>> scrollBindings;
        private Action actionToRebind;
        private Action<float> scrollActionToRebind;
        private bool isRebindingModifier;
        private List<KeyCode> newModifierKeys;

        void Start()
        {
            InitializeKeyBindings();
            InitializeScrollBindings();
        }

        void Update()
        {
            if (actionToRebind != null || scrollActionToRebind != null)
            {
                if (isRebindingModifier)
                {
                    CheckForModifierKeyPress();
                }
                else
                {
                    CheckForRebindKeyPress();
                }
            }
            else
            {
                CheckForActionKeyPress();
                CheckForScrollWheelActions();
            }
        }

        private void InitializeKeyBindings()
        {
            keyBindings = new Dictionary<KeyCode, Action>
            {
                { KeyCode.Space, Jump },
                { KeyCode.W, Up },
                { KeyCode.S, Down },
                { KeyCode.D, Right },
                { KeyCode.A, Left },
                { KeyCode.Z, CullMode },
                { KeyCode.X, DigDown },
                { KeyCode.C, DigUp },
                { KeyCode.Q, OrbitLeft },
                { KeyCode.E, OrbitRight }
            };
        }

        private void InitializeScrollBindings()
        {
            scrollBindings = new Dictionary<List<KeyCode>, Action<float>>
            {
                { new List<KeyCode> { KeyCode.LeftShift }, CullLevel },
                { new List<KeyCode> { KeyCode.LeftAlt }, HotBar }
            };
        }

        private void CheckForRebindKeyPress()
        {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    if (scrollActionToRebind != null)
                    {
                        RebindScrollModifier(scrollActionToRebind, new List<KeyCode> { keyCode });
                        scrollActionToRebind = null;
                    }
                    else
                    {
                        RebindKey(actionToRebind, keyCode);
                        actionToRebind = null;
                    }
                    return;
                }
            }
        }

        private void CheckForModifierKeyPress()
        {
            newModifierKeys = new List<KeyCode>();
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    newModifierKeys.Add(keyCode);
                }
            }
            if (newModifierKeys.Count > 0)
            {
                isRebindingModifier = false;
                RebindScrollModifier(scrollActionToRebind, newModifierKeys);
                scrollActionToRebind = null;
            }
        }

        private void CheckForActionKeyPress()
        {
            foreach (var binding in keyBindings)
            {
                if (Input.GetKeyDown(binding.Key) && binding.Value != null)
                {
                    binding.Value.Invoke();
                }
            }
        }

        private void CheckForScrollWheelActions()
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            if (scrollDelta != 0)
            {
                foreach (var binding in scrollBindings)
                {
                    bool modifiersPressed = true;
                    foreach (var key in binding.Key)
                    {
                        if (!Input.GetKey(key))
                        {
                            modifiersPressed = false;
                            break;
                        }
                    }
                    if (modifiersPressed && binding.Value != null)
                    {
                        binding.Value.Invoke(scrollDelta);
                        return;
                    }
                }

                // Handle zoom if no modifier keys are pressed
                Zoom?.Invoke(scrollDelta);
            }
        }

        public void StartRebinding(Action action)
        {
            actionToRebind = action;
            isRebindingModifier = false;
        }

        public void StartRebindingModifier(Action<float> action)
        {
            scrollActionToRebind = action;
            isRebindingModifier = true;
        }

        private void RebindKey(Action action, KeyCode newKey)
        {
            foreach (var keyBinding in keyBindings)
            {
                if (keyBinding.Value == action)
                {
                    keyBindings.Remove(keyBinding.Key);
                    keyBindings[newKey] = action;
                    Debug.Log($"Rebound action to key: {newKey}");
                    break;
                }
            }
        }

        private void RebindScrollModifier(Action<float> action, List<KeyCode> newModifierKeys)
        {
            foreach (var binding in scrollBindings)
            {
                if (binding.Value == action)
                {
                    scrollBindings.Remove(binding.Key);
                    scrollBindings[newModifierKeys] = action;
                    Debug.Log($"Rebound scroll action to modifier keys: {string.Join(", ", newModifierKeys)}");
                    break;
                }
            }
        }
    }
}
