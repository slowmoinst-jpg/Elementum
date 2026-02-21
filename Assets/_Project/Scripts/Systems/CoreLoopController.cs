using Elementum.Project.Core;
using Elementum.Project.Data;
using UnityEngine;

namespace Elementum.Project.Systems
{
    public sealed class CoreLoopController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SignalBusSO _signalBus;

        [SerializeField]
        private CoreLoopConfigSO _config;

        [Header("Runtime Debug")]
        [SerializeField, Min(0), Tooltip("Текущее активное число анти для проверки условия победы.")]
        private int _activeAntiCount;

        private bool _isSubscribed;
        private bool _isInitialized;

        private GameState _currentState = GameState.Playing;
        private GameState _stateBeforeAdPause = GameState.Playing;
        private int _currentMass;

        private bool _firstCardPickTriggered;
        private bool _secondCardPickTriggered;
        private bool _collapseTriggered;

        public void Construct(SignalBusSO signalBus, CoreLoopConfigSO config)
        {
            _signalBus = signalBus;
            _config = config;

            if (isActiveAndEnabled)
            {
                TrySubscribe();
                TryInitialize();
            }
        }

        private void OnEnable()
        {
            TrySubscribe();
            TryInitialize();
        }

        private void OnDisable()
        {
            if (!_isSubscribed || _signalBus == null)
            {
                return;
            }

            _signalBus.Tap -= HandleTap;
            _signalBus.AdOpened -= HandleAdOpened;
            _signalBus.AdClosed -= HandleAdClosed;

            _isSubscribed = false;
        }

        public void SetActiveAntiCount(int activeAntiCount)
        {
            _activeAntiCount = Mathf.Max(0, activeAntiCount);
            TryResolveWin();
        }

        public void RemoveMass(int amount, string reason)
        {
            if (amount <= 0)
            {
                return;
            }

            int previousMass = _currentMass;
            _currentMass = Mathf.Max(0, _currentMass - amount);

            int delta = _currentMass - previousMass;
            if (delta != 0 && _signalBus != null)
            {
                _signalBus.RaiseMassChanged(_currentMass, delta, reason);
            }
        }

        private void TrySubscribe()
        {
            if (_isSubscribed || _signalBus == null)
            {
                return;
            }

            _signalBus.Tap += HandleTap;
            _signalBus.AdOpened += HandleAdOpened;
            _signalBus.AdClosed += HandleAdClosed;
            _isSubscribed = true;
        }

        private void TryInitialize()
        {
            if (_isInitialized || _signalBus == null || _config == null)
            {
                return;
            }

            _currentMass = Mathf.Clamp(_config.StartingMass, 0, _config.TargetMass);
            _firstCardPickTriggered = false;
            _secondCardPickTriggered = false;
            _collapseTriggered = false;
            _currentState = GameState.Playing;

            _signalBus.RaiseGameStateChanged(_currentState);
            _signalBus.RaiseMassChanged(_currentMass, 0, "RunStart");

            _isInitialized = true;
        }

        private void HandleTap(Vector2 _)
        {
            if (_config == null || _signalBus == null)
            {
                return;
            }

            if (_currentState != GameState.Playing && _currentState != GameState.Collapse)
            {
                return;
            }

            AddMass(_config.MassPerTap, "ProtonTap");
        }

        private void HandleAdOpened()
        {
            if (_currentState == GameState.Win || _currentState == GameState.Lose)
            {
                return;
            }

            _stateBeforeAdPause = _currentState;
            SetState(GameState.Paused);
        }

        private void HandleAdClosed()
        {
            if (_currentState != GameState.Paused)
            {
                return;
            }

            if (_stateBeforeAdPause == GameState.Paused)
            {
                _stateBeforeAdPause = GameState.Playing;
            }

            SetState(_stateBeforeAdPause);
        }

        private void AddMass(int amount, string reason)
        {
            if (amount <= 0)
            {
                return;
            }

            int previousMass = _currentMass;
            _currentMass += amount;

            int delta = _currentMass - previousMass;
            _signalBus.RaiseMassChanged(_currentMass, delta, reason);

            EvaluateMilestones();
            TryResolveWin();
        }

        private void EvaluateMilestones()
        {
            if (_config.TargetMass <= 0)
            {
                return;
            }

            float progress = (float)_currentMass / _config.TargetMass;

            if (!_firstCardPickTriggered && progress >= _config.FirstCardPickThresholdNormalized)
            {
                _firstCardPickTriggered = true;
                _signalBus.RaiseCardPickRequested(33);
            }

            if (!_secondCardPickTriggered && progress >= _config.SecondCardPickThresholdNormalized)
            {
                _secondCardPickTriggered = true;
                _signalBus.RaiseCardPickRequested(66);
            }

            if (!_collapseTriggered && progress >= _config.CollapseThresholdNormalized)
            {
                _collapseTriggered = true;
                SetState(GameState.Collapse);
                _signalBus.RaiseCollapseStarted();
            }
        }

        private void TryResolveWin()
        {
            if (_config == null || _signalBus == null)
            {
                return;
            }

            if (_currentMass < _config.TargetMass || _activeAntiCount > 0)
            {
                return;
            }

            SetState(GameState.Win);
            _signalBus.RaiseWinReached();
            _signalBus.RaiseSaveRequested();
        }

        private void SetState(GameState state)
        {
            if (_currentState == state || _signalBus == null)
            {
                return;
            }

            _currentState = state;
            _signalBus.RaiseGameStateChanged(_currentState);
        }
    }
}
