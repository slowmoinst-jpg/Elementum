using System;
using UnityEngine;

namespace Elementum.Project.Core
{
    [CreateAssetMenu(
        fileName = "SignalBus",
        menuName = "Elementum/Core/Signal Bus")]
    public sealed class SignalBusSO : ScriptableObject
    {
        public event Action<Vector2> Tap;
        public event Action<Vector2, Vector2, float> Swipe;
        public event Action<Vector2, float> Hold;

        public event Action<GameState> GameStateChanged;
        public event Action<int, int, string> MassChanged;
        public event Action<int> CardPickRequested;
        public event Action CollapseStarted;
        public event Action WinReached;

        public event Action AdOpened;
        public event Action AdClosed;
        public event Action RewardGranted;

        public event Action SaveRequested;

        public void RaiseTap(Vector2 screenPosition) => Tap?.Invoke(screenPosition);

        public void RaiseSwipe(Vector2 startScreenPosition, Vector2 endScreenPosition, float durationSeconds) =>
            Swipe?.Invoke(startScreenPosition, endScreenPosition, durationSeconds);

        public void RaiseHold(Vector2 screenPosition, float holdDurationSeconds) =>
            Hold?.Invoke(screenPosition, holdDurationSeconds);

        public void RaiseGameStateChanged(GameState state) => GameStateChanged?.Invoke(state);

        public void RaiseMassChanged(int currentMass, int deltaMass, string reason) =>
            MassChanged?.Invoke(currentMass, deltaMass, reason);

        public void RaiseCardPickRequested(int checkpointPercent) =>
            CardPickRequested?.Invoke(checkpointPercent);

        public void RaiseCollapseStarted() => CollapseStarted?.Invoke();

        public void RaiseWinReached() => WinReached?.Invoke();

        public void RaiseAdOpened() => AdOpened?.Invoke();

        public void RaiseAdClosed() => AdClosed?.Invoke();

        public void RaiseRewardGranted() => RewardGranted?.Invoke();

        public void RaiseSaveRequested() => SaveRequested?.Invoke();

        private void OnDisable()
        {
            ClearAllSubscribers();
        }

        private void ClearAllSubscribers()
        {
            Tap = null;
            Swipe = null;
            Hold = null;

            GameStateChanged = null;
            MassChanged = null;
            CardPickRequested = null;
            CollapseStarted = null;
            WinReached = null;

            AdOpened = null;
            AdClosed = null;
            RewardGranted = null;

            SaveRequested = null;
        }
    }
}
