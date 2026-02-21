using UnityEngine;

namespace Elementum.Project.Data
{
    [CreateAssetMenu(
        fileName = "InputConfig",
        menuName = "Elementum/Data/Input Config")]
    public sealed class InputConfigSO : ScriptableObject
    {
        [Header("Input Action Paths")]
        [SerializeField, Tooltip("Путь к action позиции в формате Map/Action.")]
        private string _pointActionPath = "UI/Point";

        [SerializeField, Tooltip("Путь к action кнопки/нажатия в формате Map/Action.")]
        private string _pressActionPath = "UI/Click";

        [Header("Gesture Thresholds")]
        [SerializeField, Range(0.05f, 0.6f), Tooltip("Максимальная длительность тапа.")]
        private float _tapMaxDurationSeconds = 0.25f;

        [SerializeField, Range(0.2f, 2f), Tooltip("Минимальная длительность удержания.")]
        private float _holdMinDurationSeconds = 0.45f;

        [SerializeField, Range(5f, 600f), Tooltip("Минимальная дистанция свайпа в пикселях.")]
        private float _swipeMinDistancePixels = 80f;

        public string PointActionPath => _pointActionPath;
        public string PressActionPath => _pressActionPath;
        public float TapMaxDurationSeconds => _tapMaxDurationSeconds;
        public float HoldMinDurationSeconds => _holdMinDurationSeconds;
        public float SwipeMinDistancePixels => _swipeMinDistancePixels;
    }
}
