using Elementum.Project.Data;
using Elementum.Project.Input;
using Elementum.Project.Systems;
using UnityEngine;

namespace Elementum.Project.Core
{
    public sealed class CompositionRoot : MonoBehaviour
    {
        [Header("Shared")]
        [SerializeField]
        private SignalBusSO _signalBus;

        [Header("Data Configs")]
        [SerializeField]
        private InputConfigSO _inputConfig;

        [SerializeField]
        private CoreLoopConfigSO _coreLoopConfig;

        [SerializeField]
        private SaveConfigSO _saveConfig;

        [Header("Systems")]
        [SerializeField]
        private InputAdapter _inputAdapter;

        [SerializeField]
        private CoreLoopController _coreLoopController;

        [SerializeField]
        private PauseService _pauseService;

        [SerializeField]
        private SaveService _saveService;

        private void Awake()
        {
            if (_signalBus == null)
            {
                Debug.LogError("[CompositionRoot] SignalBusSO is not assigned.", this);
                return;
            }

            if (_inputAdapter != null)
            {
                _inputAdapter.Construct(_signalBus, _inputConfig);
            }

            if (_coreLoopController != null)
            {
                _coreLoopController.Construct(_signalBus, _coreLoopConfig);
            }

            if (_pauseService != null)
            {
                _pauseService.Construct(_signalBus);
            }

            if (_saveService != null)
            {
                _saveService.Construct(_signalBus, _saveConfig);
            }
        }
    }
}
