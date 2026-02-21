using UnityEngine;

namespace Elementum.Project.Data
{
    [CreateAssetMenu(
        fileName = "CoreLoopConfig",
        menuName = "Elementum/Data/Core Loop Config")]
    public sealed class CoreLoopConfigSO : ScriptableObject
    {
        [Header("Mass")]
        [SerializeField, Min(1), Tooltip("Целевая масса для победы.")]
        private int _targetMass = 100;

        [SerializeField, Min(0), Tooltip("Стартовая масса в начале run.")]
        private int _startingMass;

        [SerializeField, Min(1), Tooltip("Сколько массы дает успешный тап по протону.")]
        private int _massPerTap = 1;

        [Header("Progress Thresholds")]
        [SerializeField, Range(0.5f, 0.99f), Tooltip("Порог входа в состояние Collapse.")]
        private float _collapseThresholdNormalized = 0.9f;

        [SerializeField, Range(0.1f, 0.9f), Tooltip("Первый порог показа карточки.")]
        private float _firstCardPickThresholdNormalized = 0.33f;

        [SerializeField, Range(0.1f, 0.99f), Tooltip("Второй порог показа карточки.")]
        private float _secondCardPickThresholdNormalized = 0.66f;

        public int TargetMass => _targetMass;
        public int StartingMass => _startingMass;
        public int MassPerTap => _massPerTap;
        public float CollapseThresholdNormalized => _collapseThresholdNormalized;
        public float FirstCardPickThresholdNormalized => _firstCardPickThresholdNormalized;
        public float SecondCardPickThresholdNormalized => _secondCardPickThresholdNormalized;
    }
}
