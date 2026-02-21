using UnityEngine;

namespace Elementum.Project.Data
{
    [CreateAssetMenu(
        fileName = "SaveConfig",
        menuName = "Elementum/Data/Save Config")]
    public sealed class SaveConfigSO : ScriptableObject
    {
        [Header("File")]
        [SerializeField, Tooltip("Имя локального JSON-файла для прогресса.")]
        private string _fileName = "player_progress.json";

        [SerializeField, Tooltip("Форматировать JSON для удобства чтения.")]
        private bool _prettyJson;

        [Header("Auto Save")]
        [SerializeField, Tooltip("Автосохранение при изменении массы.")]
        private bool _autoSaveOnMassChange = true;

        [SerializeField, Min(1), Tooltip("Сохранять после накопления этого delta массы.")]
        private int _massDeltaPerAutoSave = 10;

        [SerializeField, Tooltip("Сохранять автоматически при победе.")]
        private bool _saveOnWin = true;

        [SerializeField, Tooltip("Сохранять автоматически при сворачивании приложения.")]
        private bool _saveOnApplicationPause = true;

        [Header("Cloud")]
        [SerializeField, Tooltip("Ключ для облачного сохранения (Yandex SDK).")]
        private string _cloudKey = "player_progress";

        public string FileName => _fileName;
        public bool PrettyJson => _prettyJson;
        public bool AutoSaveOnMassChange => _autoSaveOnMassChange;
        public int MassDeltaPerAutoSave => _massDeltaPerAutoSave;
        public bool SaveOnWin => _saveOnWin;
        public bool SaveOnApplicationPause => _saveOnApplicationPause;
        public string CloudKey => _cloudKey;
    }
}
