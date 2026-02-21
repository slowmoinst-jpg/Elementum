using System;
using System.IO;
using Elementum.Project.Core;
using Elementum.Project.Data;
using UnityEngine;

namespace Elementum.Project.Systems
{
    public sealed class SaveService : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SignalBusSO _signalBus;

        [SerializeField]
        private SaveConfigSO _config;

        private readonly SaveDataModel _runtimeData = new SaveDataModel();
        private bool _isSubscribed;
        private int _accumulatedMassDelta;

        public void Construct(SignalBusSO signalBus, SaveConfigSO config)
        {
            _signalBus = signalBus;
            _config = config;

            if (isActiveAndEnabled)
            {
                TrySubscribe();
            }
        }

        private void Awake()
        {
            LoadOrCreate();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (!_isSubscribed || _signalBus == null)
            {
                return;
            }

            _signalBus.MassChanged -= HandleMassChanged;
            _signalBus.GameStateChanged -= HandleGameStateChanged;
            _signalBus.SaveRequested -= HandleSaveRequested;
            _isSubscribed = false;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus || _config == null || !_config.SaveOnApplicationPause)
            {
                return;
            }

            SaveNow("ApplicationPause");
        }

        public bool TryLoadCloudJson(string cloudJson)
        {
            if (string.IsNullOrWhiteSpace(cloudJson))
            {
                return false;
            }

            SaveDataModel loaded = JsonUtility.FromJson<SaveDataModel>(cloudJson);
            if (loaded == null)
            {
                return false;
            }

            ApplyLoadedData(loaded);
            SaveNow("CloudApply");
            return true;
        }

        public string ExportCloudJson()
        {
            return JsonUtility.ToJson(_runtimeData, _config != null && _config.PrettyJson);
        }

        private void TrySubscribe()
        {
            if (_isSubscribed || _signalBus == null)
            {
                return;
            }

            _signalBus.MassChanged += HandleMassChanged;
            _signalBus.GameStateChanged += HandleGameStateChanged;
            _signalBus.SaveRequested += HandleSaveRequested;
            _isSubscribed = true;
        }

        private void HandleMassChanged(int currentMass, int deltaMass, string _)
        {
            _runtimeData.LastKnownMass = currentMass;
            if (currentMass > _runtimeData.BestMass)
            {
                _runtimeData.BestMass = currentMass;
            }

            if (_config == null || !_config.AutoSaveOnMassChange)
            {
                return;
            }

            _accumulatedMassDelta += Mathf.Abs(deltaMass);
            if (_accumulatedMassDelta < _config.MassDeltaPerAutoSave)
            {
                return;
            }

            _accumulatedMassDelta = 0;
            SaveNow("AutoMassDelta");
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state != GameState.Win || _config == null)
            {
                return;
            }

            _runtimeData.TotalWins += 1;

            if (_config.SaveOnWin)
            {
                SaveNow("WinState");
            }
        }

        private void HandleSaveRequested()
        {
            SaveNow("SignalRequest");
        }

        private void LoadOrCreate()
        {
            string path = GetFullPath();
            if (!File.Exists(path))
            {
                SaveNow("InitialCreate");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                SaveDataModel loaded = JsonUtility.FromJson<SaveDataModel>(json);

                if (loaded == null)
                {
                    Debug.LogWarning("[SaveService] Save file exists but JSON is invalid. Using defaults.", this);
                    SaveNow("InvalidJsonReset");
                    return;
                }

                ApplyLoadedData(loaded);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveService] Failed to load save file: {exception.Message}", this);
            }
        }

        private void ApplyLoadedData(SaveDataModel loaded)
        {
            SaveDataModel migrated = MigrateIfNeeded(loaded);

            _runtimeData.SaveVersion = migrated.SaveVersion;
            _runtimeData.LastKnownMass = migrated.LastKnownMass;
            _runtimeData.BestMass = migrated.BestMass;
            _runtimeData.TotalWins = migrated.TotalWins;
            _runtimeData.StarDust = migrated.StarDust;
            _runtimeData.LastElementId = string.IsNullOrWhiteSpace(migrated.LastElementId)
                ? "H"
                : migrated.LastElementId;
        }

        private SaveDataModel MigrateIfNeeded(SaveDataModel loaded)
        {
            if (loaded.SaveVersion == SaveDataModel.LatestVersion)
            {
                return loaded;
            }

            // Миграция здесь централизована: новые версии преобразуем в одном месте.
            SaveDataModel migrated = new SaveDataModel();
            migrated.SaveVersion = SaveDataModel.LatestVersion;
            migrated.LastKnownMass = Mathf.Max(0, loaded.LastKnownMass);
            migrated.BestMass = Mathf.Max(0, loaded.BestMass);
            migrated.TotalWins = Mathf.Max(0, loaded.TotalWins);
            migrated.StarDust = Mathf.Max(0, loaded.StarDust);
            migrated.LastElementId = string.IsNullOrWhiteSpace(loaded.LastElementId) ? "H" : loaded.LastElementId;

            return migrated;
        }

        private void SaveNow(string reason)
        {
            _runtimeData.SaveVersion = SaveDataModel.LatestVersion;

            if (_config == null)
            {
                Debug.LogError("[SaveService] SaveConfigSO is not assigned.", this);
                return;
            }

            string path = GetFullPath();
            string directoryPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            try
            {
                string json = JsonUtility.ToJson(_runtimeData, _config.PrettyJson);
                File.WriteAllText(path, json);
                Debug.Log($"[SaveService] Saved ({reason}) to: {path}", this);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[SaveService] Failed to save file: {exception.Message}", this);
            }
        }

        private string GetFullPath()
        {
            string fileName = _config != null && !string.IsNullOrWhiteSpace(_config.FileName)
                ? _config.FileName
                : "player_progress.json";

            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }
}
