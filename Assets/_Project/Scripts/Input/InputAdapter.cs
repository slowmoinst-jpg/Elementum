using Elementum.Project.Core;
using Elementum.Project.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementum.Project.Input
{
    public sealed class InputAdapter : MonoBehaviour
    {
        // Безопасные значения на случай отсутствия конфига.
        private const float FallbackTapMaxDurationSeconds = 0.25f;
        private const float FallbackHoldMinDurationSeconds = 0.45f;
        private const float FallbackSwipeMinDistancePixels = 80f;

        [Header("Dependencies")]
        [SerializeField, Tooltip("InputActionAsset с action paths из InputConfigSO.")]
        private InputActionAsset _inputActions;

        [SerializeField]
        private SignalBusSO _signalBus;

        [SerializeField]
        private InputConfigSO _config;

        private InputAction _pointAction;
        private InputAction _pressAction;
        private bool _isSubscribed;

        private bool _isPressed;
        private Vector2 _pressStartPosition;
        private float _pressStartTime;

        public void Construct(SignalBusSO signalBus, InputConfigSO config)
        {
            _signalBus = signalBus;
            _config = config;
            ResolveActions();

            if (isActiveAndEnabled)
            {
                TrySubscribe();
            }
        }

        private void Awake()
        {
            ResolveActions();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (!_isSubscribed || _pressAction == null)
            {
                return;
            }

            _pressAction.started -= HandlePressStarted;
            _pressAction.canceled -= HandlePressCanceled;
            _pressAction.Disable();
            _pointAction?.Disable();

            _isSubscribed = false;
            _isPressed = false;
        }

        private void ResolveActions()
        {
            if (_inputActions == null || _config == null)
            {
                return;
            }

            _pointAction = FindAction(_config.PointActionPath);
            _pressAction = FindAction(_config.PressActionPath);

            if (_pointAction == null)
            {
                Debug.LogError(
                    $"[InputAdapter] Point action not found: '{_config.PointActionPath}'. " +
                    "Assign a valid action path like 'UI/Point'.",
                    this);
            }

            if (_pressAction == null)
            {
                Debug.LogError(
                    $"[InputAdapter] Press action not found: '{_config.PressActionPath}'. " +
                    "Assign a valid action path like 'UI/Click'.",
                    this);
            }
        }

        private InputAction FindAction(string actionPath)
        {
            if (string.IsNullOrWhiteSpace(actionPath) || _inputActions == null)
            {
                return null;
            }

            InputAction action = _inputActions.FindAction(actionPath, false);
            if (action != null)
            {
                return action;
            }

            int separatorIndex = actionPath.LastIndexOf('/');
            if (separatorIndex >= 0 && separatorIndex < actionPath.Length - 1)
            {
                string shortName = actionPath.Substring(separatorIndex + 1);
                return _inputActions.FindAction(shortName, false);
            }

            return null;
        }

        private void TrySubscribe()
        {
            if (_isSubscribed || _signalBus == null)
            {
                return;
            }

            ResolveActions();
            if (_pressAction == null || _pointAction == null)
            {
                return;
            }

            _pointAction.Enable();
            _pressAction.Enable();
            _pressAction.started += HandlePressStarted;
            _pressAction.canceled += HandlePressCanceled;
            _isSubscribed = true;
        }

        private void HandlePressStarted(InputAction.CallbackContext _)
        {
            _isPressed = true;
            _pressStartTime = Time.unscaledTime;
            _pressStartPosition = ReadPointerPosition();
        }

        private void HandlePressCanceled(InputAction.CallbackContext _)
        {
            if (!_isPressed)
            {
                return;
            }

            _isPressed = false;

            Vector2 endPosition = ReadPointerPosition();
            float durationSeconds = Time.unscaledTime - _pressStartTime;
            float distancePixels = Vector2.Distance(_pressStartPosition, endPosition);

            float swipeThreshold = _config != null
                ? _config.SwipeMinDistancePixels
                : FallbackSwipeMinDistancePixels;
            float holdThreshold = _config != null
                ? _config.HoldMinDurationSeconds
                : FallbackHoldMinDurationSeconds;
            float tapThreshold = _config != null
                ? _config.TapMaxDurationSeconds
                : FallbackTapMaxDurationSeconds;

            // Единая классификация жеста для мыши и тача по одним и тем же порогам.
            if (distancePixels >= swipeThreshold)
            {
                _signalBus.RaiseSwipe(_pressStartPosition, endPosition, durationSeconds);
                return;
            }

            if (durationSeconds >= holdThreshold)
            {
                _signalBus.RaiseHold(endPosition, durationSeconds);
                return;
            }

            if (durationSeconds <= tapThreshold)
            {
                _signalBus.RaiseTap(endPosition);
                return;
            }

            _signalBus.RaiseHold(endPosition, durationSeconds);
        }

        private Vector2 ReadPointerPosition()
        {
            if (_pointAction == null)
            {
                return default;
            }

            return _pointAction.ReadValue<Vector2>();
        }
    }
}
